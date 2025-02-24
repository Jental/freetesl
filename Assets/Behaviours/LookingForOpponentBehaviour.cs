#nullable enable

using Assets.Services;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Assets.Common;

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

        private bool progressDialogVisible = false;

        protected void Start()
        {
            if (buttonsGameObject == null) throw new InvalidOperationException($"{nameof(buttonsGameObject)} game object is expected to be set");
            if (startLookingButtonGameObject == null) throw new InvalidOperationException($"{nameof(startLookingButtonGameObject)} game object is expected to be set");
            if (startMatchButtonGameObject == null) throw new InvalidOperationException($"{nameof(startMatchButtonGameObject)} game object is expected to be set");
            if (cancelButtonGameObject == null) throw new InvalidOperationException($"{nameof(cancelButtonGameObject)} game object is expected to be set");
            if (progressDialogGameObject == null) throw new InvalidOperationException($"{nameof(progressDialogGameObject)} game object is expected to be set");
            if (playerListGameObject == null) throw new InvalidOperationException($"{nameof(playerListGameObject)} game object is expected to be set");

            _ = destroyCancellationToken;
            startLookingButtonGameObject.onClick.AddListener(OnStartButtonClick);
            cancelButtonGameObject.onClick.AddListener(OnCancelButtonClick);
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
                startMatchButtonGameObject!.gameObject.SetActive(playerListGameObject!.SelectedPlayer != null);
            }
        }

        private void OnStartButtonClick()
        {
            Debug.Log("StartLookingForOpponentButtonBehaviour.OnButtonClick");
            _ = Task.Run(async () =>
            {
                try
                {
                    await Networking.Instance.PostAsync(Constants.MethodNames.START_LOOKING_FOR_OPPONENT, destroyCancellationToken);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return;
                }

                progressDialogVisible = true;
            });
        }

        private void OnCancelButtonClick()
        {
            Debug.Log("StartLookingForOpponentButtonBehaviour.OnCancelButtonClick");
            progressDialogVisible = false;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Networking.Instance.PostAsync(Constants.MethodNames.STOP_LOOKING_FOR_OPPONENT, destroyCancellationToken);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return;
                }
            });
        }
    }
}