#nullable enable

using Assets.Services;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Behaviours
{
    public class MatchEndCanvasBehaviour : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CanvasService? canvasService;
        [SerializeField] private GameObject? victoryGameObject;
        [SerializeField] private GameObject? defeatGameObject;

        private bool? hasWon = null;
        private bool changesArePresent = false;

        public bool? HasWon
        {
            get { return hasWon; }
            set
            {
                if (hasWon != value)
                {
                    hasWon = value;
                    changesArePresent = true;
                }
            }
        }

        protected void Start()
        {
            if (canvasService == null) throw new InvalidOperationException($"{nameof(canvasService)} game object is expected to be set");
            if (victoryGameObject == null) throw new InvalidOperationException($"{nameof(victoryGameObject)} game object is expected to be set");
            if (defeatGameObject == null) throw new InvalidOperationException($"{nameof(defeatGameObject)} game object is expected to be set");
        }

        protected void Update()
        {
            if (!changesArePresent)
            {
                return;
            }
            changesArePresent = false;

            switch (hasWon)
            {
                case null:
                    victoryGameObject!.SetActive(false);
                    defeatGameObject!.SetActive(false);
                    break;
                case true:
                    victoryGameObject!.SetActive(true);
                    defeatGameObject!.SetActive(false);
                    break;
                case false:
                    victoryGameObject!.SetActive(false);
                    defeatGameObject!.SetActive(true);
                    break;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            hasWon = null; // resetting state
            canvasService!.ActiveCanvas = Enums.AppCanvas.JoinMatch;
        }
    }
}
