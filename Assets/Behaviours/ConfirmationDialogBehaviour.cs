#nullable enable

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class ConfirmationDialogBehaviour : MonoBehaviour
    {
        [SerializeField] private string headerText = "Confirm";
        [SerializeField] private string? text = null;
        [SerializeField] private string okButtonText = "Confirm";
        [SerializeField] private Texture2D? image = null;
        [SerializeField] private TextMeshProUGUI? headerTextGameObject = null;
        [SerializeField] private TextMeshProUGUI? textGameObject = null;
        [SerializeField] private RawImage? imageGameObject = null;
        [SerializeField] private TextMeshProUGUI? okButtonTextGameObject = null;
        [SerializeField] private Button? okButtonGameObject = null;
        [SerializeField] private Button? cancelButtonGameObject = null;

        private bool isRendered = false;
        private Action? onOk;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected void Start()
        {
            if (text == null) throw new InvalidOperationException($"{nameof(text)} parameter is expected to be set");
            if (headerTextGameObject == null) throw new InvalidOperationException($"{nameof(headerTextGameObject)} game object is expected to be set");
            if (textGameObject == null) throw new InvalidOperationException($"{nameof(textGameObject)} game object is expected to be set");
            if (imageGameObject == null) throw new InvalidOperationException($"{nameof(imageGameObject)} game object is expected to be set");
            if (okButtonTextGameObject == null) throw new InvalidOperationException($"{nameof(okButtonTextGameObject)} game object is expected to be set");
            if (okButtonGameObject == null) throw new InvalidOperationException($"{nameof(okButtonGameObject)} game object is expected to be set");
            if (cancelButtonGameObject == null) throw new InvalidOperationException($"{nameof(cancelButtonGameObject)} game object is expected to be set");

            cancelButtonGameObject.onClick.AddListener(OnCancel);
            okButtonGameObject.onClick.AddListener(OnOkAgg);
        }

        protected void Update()
        {
            if (isRendered) return;
            isRendered = true;

            headerTextGameObject!.text = headerText;
            textGameObject!.text = text;
            okButtonTextGameObject!.text = okButtonText;
            if (image != null) {
                imageGameObject!.texture = image;
            }
        }

        private void OnCancel()
        {
            this.gameObject.SetActive(false);
        }

        public Action OnOK
        {
            set { this.onOk = value; }
        }

        private void OnOkAgg()
        {
            this.gameObject.SetActive(false);

            onOk?.Invoke();
        }
    }
}