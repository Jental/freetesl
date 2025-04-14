#nullable enable

using Assets.Common;
using Assets.Services;
using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class DeleteDeckButtonBehaviour: MonoBehaviour
    {
        [SerializeField] private DeckListBehaviour? deckListBehaviour;
        [SerializeField] private CanvasService? canvasService;

        private Button? buttonComponent;

        protected void Start()
        {
            if (deckListBehaviour == null) throw new InvalidOperationException($"{nameof(deckListBehaviour)} game object is expected to be set");
            if (canvasService == null) throw new InvalidOperationException($"{nameof(canvasService)} game object is expected to be set");

            _ = destroyCancellationToken;

            buttonComponent = this.GetComponent<Button>() ?? throw new InvalidOperationException($"{nameof(Button)} comnponent is expected to be present");
            buttonComponent.onClick.AddListener(OnClick);
        }

        protected void Update()
        {
            buttonComponent!.interactable = deckListBehaviour!.SelectedModel != null;
        }

        private void OnClick()
        {
            Debug.Log($"{nameof(DeleteDeckButtonBehaviour)}: {nameof(OnClick)}");
            
            int? selectedDeckID = deckListBehaviour!.SelectedModel?.id ?? throw new InvalidOperationException($"Deck should be selected");

            Debug.Log($"{nameof(DeleteDeckButtonBehaviour)}: {nameof(OnClick)}: deck id: {selectedDeckID}");

            _ = Task.Run(async () =>
            {
                await Networking.Instance.DeleteAsync(
                    Constants.MethodNames.DELETE_DECK,
                    new Dictionary<string, string>()
                    {
                        { "deckID", selectedDeckID.ToString() },
                    },
                    destroyCancellationToken
                );

                deckListBehaviour!.Deselect();
                deckListBehaviour.Refresh();
            });
        }
    }
}
