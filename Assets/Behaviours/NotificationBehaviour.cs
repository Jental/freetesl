#nullable enable

using System;
using TMPro;
using UnityEngine;

namespace Assets.Behaviours
{
    public class NotificationBehaviour: MonoBehaviour
    {
        private const double SECONDS_TO_SHOW = 1;

        [SerializeField] private TextMeshProUGUI? textGameObject;

        private bool changesArePresent = false;
        private string? text = null;
        private DateTime? startedToShow = null;

        public string Text
        {
            set
            {
                this.text = value;
                changesArePresent = true;
            }
        }

        protected void Start()
        {
            if (textGameObject == null) throw new InvalidOperationException($"{nameof(textGameObject)} game object is expected to be set");
        }

        protected void Update()
        {
            if (changesArePresent)
            {
                changesArePresent = false;
                if (text != null)
                {
                    textGameObject!.text = text;
                    startedToShow = DateTime.Now;
                }
            }
            else if (startedToShow != null && (DateTime.Now - startedToShow.Value).TotalSeconds >= SECONDS_TO_SHOW)
            {
                this.gameObject.SetActive(false);
                startedToShow = null;
            }
        }

    }
}
