#nullable enable

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class ExitButtonBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject? exitConfirmationDialog = null;

        protected void Start()
        {
            if (exitConfirmationDialog == null) throw new InvalidOperationException($"{nameof(exitConfirmationDialog)} game object is expected to be set");

            var buttonGameObject = GetComponent<Button>();
            if (buttonGameObject == null) throw new InvalidOperationException("Button component is expected to be present");
            buttonGameObject.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            var popups = GameObject.FindGameObjectsWithTag("popup");
            foreach (var popup in popups)
            {
                popup.SetActive(false);
            }

            exitConfirmationDialog!.SetActive(true);
        }
    }
}