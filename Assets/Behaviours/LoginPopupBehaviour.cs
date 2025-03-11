#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Enums;
using Assets.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class LoginPopupBehaviour : MonoBehaviour
    {
        [SerializeField] private TMP_InputField? loginGameObject;
        [SerializeField] private TMP_InputField? passwordGameObject;
        [SerializeField] private TextMeshProUGUI? errorGameObject;
        [SerializeField] private TMP_Dropdown? serverSelectGameObject;
        [SerializeField] private Button? sendButtonGameObject;
        [SerializeField] private CanvasService? canvasService;

        private int inputFocused = 0;
        private string? errorMessage = null;

        protected void Start()
        {
            if (sendButtonGameObject == null) throw new InvalidOperationException($"{nameof(sendButtonGameObject)} game object is expected to be set");
            if (loginGameObject == null) throw new InvalidOperationException($"{nameof(loginGameObject)} game object is expected to be set");
            if (passwordGameObject == null) throw new InvalidOperationException($"{nameof(passwordGameObject)} game object is expected to be set");
            if (serverSelectGameObject == null) throw new InvalidOperationException($"{nameof(serverSelectGameObject)} game object is expected to be set");
            if (canvasService == null) throw new InvalidOperationException($"{nameof(canvasService)} game object is expected to be set");
            if (errorGameObject == null) throw new InvalidOperationException($"{nameof(errorGameObject)} game object is expected to be set");

            GlobalStorage.Instance.Init();

            _ = destroyCancellationToken;
            sendButtonGameObject.onClick.AddListener(OnSendButtonClick);
            loginGameObject.ActivateInputField();
        }

        protected void Update()
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame && Keyboard.current.leftShiftKey.wasPressedThisFrame)
            {
                inputFocused--;
                if (inputFocused < 0) inputFocused = 2;
                UpdateInputFocus();
            }
            else if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                inputFocused++;
                if (inputFocused > 2) inputFocused = 0;
                UpdateInputFocus();
            }

            if (errorMessage != null)
            {
                this.errorGameObject!.text = (string)errorMessage.Clone();
                this.errorGameObject.gameObject.SetActive(true);
                errorMessage = null;
            }
        }

        protected void OnDisable()
        {
            this.errorGameObject!.gameObject.SetActive(false);
        }

        private void UpdateInputFocus()
        {
            switch (inputFocused)
            {
                case 0:
                    loginGameObject!.ActivateInputField();
                    break;
                case 1:
                    passwordGameObject!.ActivateInputField();
                    break;
                case 2:
                    sendButtonGameObject!.Select();
                    break;
                default:
                    throw new InvalidOperationException($"Invalid {nameof(inputFocused)} value: '{inputFocused}'");
            }
        }

        private void OnSendButtonClick()
        {
            _ = Task.Run(async () =>
            {
                Debug.Log($"LoginPopupBehaviour.OnSendButtonClick: login: {loginGameObject!.text}");

                var selectedServerIdx = serverSelectGameObject!.value;
                var serverUrl = serverSelectGameObject.options[selectedServerIdx].text;
                Debug.Log($"LoginPopupBehaviour.OnSendButtonClick: server: {serverUrl}");
                Networking.Instance.Init(serverUrl, canvasService!);

                using var sha512 = SHA512.Create();
                var passwordHash = GetStringFromHash(sha512.ComputeHash(Encoding.UTF8.GetBytes(passwordGameObject!.text)));
                var dto = new LoginDTO { login = loginGameObject.text, passwordSha512 = passwordHash };
                LoginResponseDTO? response = null;
                try
                {
                    response = await Networking.Instance.PostAsync<LoginDTO, LoginResponseDTO>(Constants.MethodNames.LOGIN, dto, destroyCancellationToken);
                }
                catch (Exception e)
                {
                    errorMessage = "Failed to log in. Could not connect the server";
                    Debug.LogException(e);
                    return;
                }

                if (response == null || !response.valid || response.token == null)
                {
                    Debug.Log("LoginPopupBehaviour.OnSendButtonClick: failed to log in");
                    errorMessage = "Failed to log in. Invalid login or password";
                    return;
                }

                GlobalStorage.Instance.Token = response.token;
                GlobalStorage.Instance.PlayerID = response.playerID;
                GlobalStorage.Instance.PlayerLogin = loginGameObject.text;
                GlobalStorage.Instance.CurrentServer = serverUrl;

                PlayerInformationDTO? currentPlayerInfo;
                try
                {
                    currentPlayerInfo = await Networking.Instance.GetAsync<PlayerInformationDTO>(Constants.MethodNames.GET_CURRENT_PLAYER_INFO, new Dictionary<string, string>(), destroyCancellationToken);
                }
                catch (Exception e)
                {
                    errorMessage = "Failed to log in. Failed to retrieve current user";
                    Debug.LogException(e);
                    return;
                }

                if (currentPlayerInfo == null)
                {
                    Debug.Log("LoginPopupBehaviour.OnSendButtonClick: failed to get current user info");
                    errorMessage = "Failed to log in. Failed to retrieve current user";
                    return;
                }

                if (currentPlayerInfo.state == (int)PlayerState.InMatch)
                {
                    canvasService!.ActiveCanvas = AppCanvas.Match;
                }
                else
                {
                    canvasService!.ActiveCanvas = AppCanvas.JoinMatch;
                }

                Debug.Log("LoginPopupBehaviour.OnSendButtonClick: logged in successfully");
            });
        }

        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }
    }
}