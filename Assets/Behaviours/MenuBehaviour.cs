#nullable enable

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class MenuBehaviour : MonoBehaviour
    {
        [SerializeField] private Button[] buttons = Array.Empty<Button>();
        [SerializeField] private GameObject? buttonsBlock = null;
        [SerializeField] private Button? closeButton = null;
        
        private bool isRendered = false;

        protected void Start ()
        {
            if (buttonsBlock == null) throw new InvalidOperationException($"{nameof(buttonsBlock)} game object is expected to be set");
            if (closeButton == null) throw new InvalidOperationException($"{nameof(closeButton)} game object is expected to be set");

            closeButton.onClick.AddListener(OnCloseClick);
        }

        protected void OnDisable()
        {
            isRendered = false;
        }

        protected void Update()
        {
            if (!isRendered)
            {
                isRendered = true;

                foreach (var button in buttons)
                {
                    button.gameObject.transform.parent = buttonsBlock!.transform;
                    button.gameObject.SetActive(true);
                }
            }
        }

        private void OnCloseClick()
        {
            var popups = GameObject.FindGameObjectsWithTag("popup");
            foreach (var popup in popups)
            {
                popup.SetActive(false);
            }
        }
    }
}
