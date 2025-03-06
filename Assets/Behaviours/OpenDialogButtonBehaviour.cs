#nullable enable

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class OpenDialogButtonBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject? dialogGameObject = null;

        protected void Start()
        {
            if (dialogGameObject == null) throw new InvalidOperationException($"{nameof(dialogGameObject)} game object is expected to be set");

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

            dialogGameObject!.SetActive(true);
        }
    }
}