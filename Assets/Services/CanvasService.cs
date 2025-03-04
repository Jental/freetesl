#nullable enable

using Assets.Behaviours;
using Assets.Enums;
using System;
using UnityEngine;

namespace Assets.Services
{
    public class CanvasService: MonoBehaviour
    {
        [SerializeField] private Canvas? loginCanvas;
        [SerializeField] private Canvas? joinMatchCanvas;
        [SerializeField] private Canvas? matchCanvas;
        [SerializeField] private MatchEndCanvasBehaviour? matchEndCanvas;

        private Component[] canvases = Array.Empty<Component>();
        private AppCanvas activeCanvas = AppCanvas.Login;
        private bool changesArePresent = false;

        protected void Start()
        {
            if (loginCanvas == null) throw new InvalidOperationException($"{nameof(loginCanvas)} game object is expected to be set");
            if (joinMatchCanvas == null) throw new InvalidOperationException($"{nameof(joinMatchCanvas)} game object is expected to be set");
            if (matchCanvas == null) throw new InvalidOperationException($"{nameof(matchCanvas)} game object is expected to be set");
            if (matchEndCanvas == null) throw new InvalidOperationException($"{nameof(matchEndCanvas)} game object is expected to be set");

            canvases = new Component[] { loginCanvas, joinMatchCanvas, matchCanvas, matchEndCanvas };
        }

        protected void Update()
        {
            if (!changesArePresent)
            {
                return;
            }
            changesArePresent = false;

            for (int i = 0; i< canvases.Length; i++)
            {
                canvases[i].gameObject.SetActive(i == (int)activeCanvas);
            }
        }

        public AppCanvas ActiveCanvas
        {
            set
            {
                activeCanvas = value;
                changesArePresent = true;
            }
        }

        public bool MatchEndHasWon
        {
            set
            {
                matchEndCanvas!.HasWon = value;
            }
        }
    }
}
