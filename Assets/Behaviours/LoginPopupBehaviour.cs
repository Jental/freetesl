#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Services;
using System;
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
        [SerializeField] private Button? sendButtonGameObject;
        [SerializeField] private Canvas? loginCanvas;
        [SerializeField] private Canvas? matchCanvas;

        public int inputFocused = 0;
        public int canvasActive = 0;

        protected void Start()
        {
            if (sendButtonGameObject == null) throw new InvalidOperationException($"{nameof(sendButtonGameObject)} game object is expected to be set");
            if (loginGameObject == null) throw new InvalidOperationException($"{nameof(loginGameObject)} game object is expected to be set");
            if (passwordGameObject == null) throw new InvalidOperationException($"{nameof(passwordGameObject)} game object is expected to be set");
            if (loginCanvas == null) throw new InvalidOperationException($"{nameof(loginCanvas)} game object is expected to be set");
            if (matchCanvas == null) throw new InvalidOperationException($"{nameof(matchCanvas)} game object is expected to be set");

            _ = destroyCancellationToken;
            sendButtonGameObject.onClick.AddListener(OnSendButtonClick);
            loginGameObject.ActivateInputField();
        }

        protected void Update()
        {
            // UpdateCanvasActivity();

            if (loginCanvas!.isActiveAndEnabled)
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
            }
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

        private void UpdateCanvasActivity()
        {
            switch (canvasActive)
            {
                case 0:
                    loginCanvas!.gameObject.SetActive(true);
                    matchCanvas!.gameObject.SetActive(false);
                    break;
                case 1:
                    loginCanvas!.gameObject.SetActive(false);
                    matchCanvas!.gameObject.SetActive(true);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid {nameof(canvasActive)} value: '{canvasActive}'");
            }
        }

        private void OnSendButtonClick()
        {
            _ = Task.Run(async () =>
            {
                if (loginGameObject == null) throw new InvalidOperationException($"{nameof(loginGameObject)} game object is expected to be set");
                if (passwordGameObject == null) throw new InvalidOperationException($"{nameof(passwordGameObject)} game object is expected to be set");

                Debug.Log("LoginPopupBehaviour.OnSendButtonClick");
                Debug.Log($"LoginPopupBehaviour.OnSendButtonClick: login: {loginGameObject.text}");

                using var sha512 = SHA512.Create();
                var passwordHash = GetStringFromHash(sha512.ComputeHash(Encoding.UTF8.GetBytes(passwordGameObject.text)));
                var dto = new LoginDTO { login = loginGameObject.text, passwordSha512 = passwordHash };
                var response = await Networking.Instance.PostAsync<LoginDTO, LoginResponseDTO>(Constants.MethodNames.LOGIN, dto, destroyCancellationToken);

                Debug.Log($"token: '{response?.token}'; valid: '{response?.valid}'");

                if (response != null && response.valid && response.token != null)
                {
                    GlobalStorage.Instance.Token = response.token;

                    canvasActive = 1;
                }
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