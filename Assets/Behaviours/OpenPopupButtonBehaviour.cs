#nullable enable

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class OpenPopupButtonBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject? popupGameObject = null;

        protected void Start()
        {
            if (popupGameObject == null) throw new InvalidOperationException($"{nameof(popupGameObject)} game object is expected to be set");

            var buttonGameObject = GetComponent<Button>();
            if (buttonGameObject == null) throw new InvalidOperationException("Button component is expected to be present");
            buttonGameObject.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            Debug.Log("OpenPopupButtonBehaviour.OnClick");

            var popups = GameObject.FindGameObjectsWithTag("popup");
            foreach (var popup in popups)
            {
                popup.SetActive(false);
            }

            popupGameObject!.SetActive(true);
        }
    }
}