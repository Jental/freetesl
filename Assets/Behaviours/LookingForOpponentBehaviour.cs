#nullable enable

using Assets.Services;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Assets.Common;
using Assets.DTO;
using System.Collections.Generic;
using System.Threading;
using Assets.Enums;
using Assets.Models;

namespace Assets.Behaviours
{
    public class LookingForOpponentBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject? buttonsGameObject;
        [SerializeField] private Button? startLookingButtonGameObject;
        [SerializeField] private Button? startMatchButtonGameObject;
        [SerializeField] private Button? cancelButtonGameObject;
        [SerializeField] private GameObject? progressDialogGameObject;
        [SerializeField] private PlayerListBehaviour? playerListGameObject;
        [SerializeField] private CanvasService? canvasService;

        private bool progressDialogVisible = false;
        private volatile Timer? statusPollingTimer = null;

        protected void Start()
        {
            Debug.Log("LookingForOpponentBehaviour.Start");

            if (buttonsGameObject == null) throw new InvalidOperationException($"{nameof(buttonsGameObject)} game object is expected to be set");
            if (startLookingButtonGameObject == null) throw new InvalidOperationException($"{nameof(startLookingButtonGameObject)} game object is expected to be set");
            if (startMatchButtonGameObject == null) throw new InvalidOperationException($"{nameof(startMatchButtonGameObject)} game object is expected to be set");
            if (cancelButtonGameObject == null) throw new InvalidOperationException($"{nameof(cancelButtonGameObject)} game object is expected to be set");
            if (progressDialogGameObject == null) throw new InvalidOperationException($"{nameof(progressDialogGameObject)} game object is expected to be set");
            if (playerListGameObject == null) throw new InvalidOperationException($"{nameof(playerListGameObject)} game object is expected to be set");
            if (canvasService == null) throw new InvalidOperationException($"{nameof(canvasService)} game object is expected to be set");

            _ = destroyCancellationToken;
            startLookingButtonGameObject.onClick.AddListener(OnStartButtonClick);
            cancelButtonGameObject.onClick.AddListener(OnCancelButtonClick);
            startMatchButtonGameObject.onClick.AddListener(OnStartMatchButtonClick);
        }

        protected void OnDisable()
        {
            Debug.Log("LookingForOpponentBehaviour.OnDisable"); 
            progressDialogVisible = false;
            StopStatusPollingAsync().Wait();
        }

        protected void Update()
        {
            if (progressDialogVisible)
            {
                progressDialogGameObject!.SetActive(true);
                buttonsGameObject!.SetActive(false);
                startMatchButtonGameObject!.gameObject.SetActive(false);
            }
            else
            {
                progressDialogGameObject!.SetActive(false);
                buttonsGameObject!.SetActive(true);

                bool matchCanBeStarted =
                    playerListGameObject!.SelectedPlayer != null
                    && playerListGameObject!.SelectedPlayer.ID != GlobalStorage.Instance.PlayerID
                    && playerListGameObject.SelectedPlayer.State == PlayerState.LookingForOpponent;
                startMatchButtonGameObject!.gameObject.SetActive(matchCanBeStarted);
            }
        }

        private void OnStartButtonClick()
        {
            Debug.Log("LookingForOpponentBehaviour.OnStartButtonClick");
            _ = Task.Run(async () =>
            {
                try
                {
                    await Networking.Instance.PostAsync(Constants.MethodNames.LOOKING_FOR_OPPONENT_START, destroyCancellationToken);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return;
                }

                progressDialogVisible = true;
            });

            StartStatusPolling(destroyCancellationToken);
        }

        private void OnCancelButtonClick()
        {
            Debug.Log("LookingForOpponentBehaviour.OnCancelButtonClick");
            progressDialogVisible = false;

            _ = Task.Run(async () =>
            {
                try
                {
                    await StopStatusPollingAsync();
                    await Networking.Instance.PostAsync(Constants.MethodNames.LOOKING_FOR_OPPONENT_STOP, destroyCancellationToken);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return;
                }
            });
        }

        private void OnStartMatchButtonClick()
        {
            if (playerListGameObject == null) throw new InvalidOperationException($"{nameof(playerListGameObject)} game object is expected to be set");
            Debug.Log("LookingForOpponentBehaviour.OnStartMatchButtonClick");

            var opponentID = playerListGameObject.SelectedPlayer?.ID ?? throw new InvalidOperationException("At least one player is expected to be selected");

            _ = Task.Run(async () =>
            {
                var dto = new MatchCreateDTO { opponentID = opponentID };
                try
                {
                    var resp = await Networking.Instance.PostAsync<MatchCreateDTO, GuidIdDTO>(Constants.MethodNames.MATCH_CREATE, dto, destroyCancellationToken);
                    if (resp == null)
                    {
                        Debug.LogError($"Created match: expected non-null response");
                        return;
                    }
                    else if (resp.id == null)
                    {
                        Debug.LogError($"Created match: expected non-null match id");
                        return;
                    }
                    else
                    {
                        Debug.Log($"Created match: '{resp.id}'");
                        canvasService!.ActiveCanvas = AppCanvas.Match;
                    }
                }
                catch (ServerErrorResponseException e)
                {
                    canvasService!.ShowError($"Failed to create a match: '{e.Message}'. ErrorCode: '{e.ErrorCode}'");
                    return;
                }
                catch (Exception e)
                {
                    canvasService!.ShowError($"Failed to create a match: {e.Message}");
                    return;
                }
            });
        }

        private void StartStatusPolling(CancellationToken cancellationToken)
        {
            Debug.Log("LookingForOpponentBehaviour.StartStatusPolling");
            statusPollingTimer = new Timer(
                async (_) => { await GetStatusAsync(cancellationToken); },
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(Constants.BACKEND_POLLING_INTERVAL)
            );
        }

        private async Task StopStatusPollingAsync()
        {
            Debug.Log("LookingForOpponentBehaviour.StopStatusPollingAsync");
            if (statusPollingTimer != null)
            {
                statusPollingTimer!.Change(Timeout.Infinite, Timeout.Infinite);
                await statusPollingTimer.DisposeAsync();
                statusPollingTimer = null;
            }
        }

        private async Task GetStatusAsync(CancellationToken cancellationToken)
        {
            GuidIdDTO? dto;
            try
            {
                dto = await Networking.Instance.GetAsync<GuidIdDTO>(
                    Constants.MethodNames.LOOKING_FOR_OPPONENT_STATUS,
                    new Dictionary<string, string>(),
                    cancellationToken
                );
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            if (dto == null)
            {
                Debug.LogError($"LookingForOpponentBehaviour.GetStatusAsync: expected non-null response");
                return;
            }

            if (!string.IsNullOrEmpty(dto.id))
            {
                Debug.Log($"LookingForOpponentBehaviour.GetStatusAsync: Match id: {dto.id}");

                if (statusPollingTimer == null) {
                    Debug.LogError($"LookingForOpponentBehaviour.GetStatusAsync: expected non-null timer");
                    return;
                }

                await StopStatusPollingAsync();

                canvasService!.ActiveCanvas = AppCanvas.Match;
            }
            else
            {
                Debug.Log($"LookingForOpponentBehaviour.GetStatusAsync: Not ready");
            }
        }
    }
}