using System;
using TMPro;
using UnityEngine;

namespace Assets.Behaviours
{
    public class TooltipBehaviour : MonoBehaviour
    {
        [SerializeField] private Camera uiCamera;

        private TextMeshProUGUI textGameObject;
        private RectTransform rectTransform;

        protected void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            textGameObject = transform.Find("TooltipText").GetComponent<TextMeshProUGUI>();
        }

        protected void Update()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out Vector2 localPosition);
            transform.localPosition = localPosition;
        }

        public void Show(string text)
        {
            gameObject.SetActive(true);

            textGameObject.text = text;

            var size = new Vector2(textGameObject.preferredWidth + 25.0f, Math.Max(50.0f, textGameObject.preferredHeight + 25.0f));
            rectTransform.sizeDelta = size;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
