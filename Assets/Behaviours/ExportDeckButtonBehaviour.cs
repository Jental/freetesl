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
    public class ExportDeckButtonBehaviour: MonoBehaviour
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
            Debug.Log($"{nameof(ExportDeckButtonBehaviour)}: {nameof(OnClick)}");
            
            int? selectedDeckID = deckListBehaviour!.SelectedModel?.id ?? throw new InvalidOperationException($"Deck should be selected");
            string selectedDeckName = deckListBehaviour.SelectedModel.name;

            Debug.Log($"{nameof(ExportDeckButtonBehaviour)}: {nameof(OnClick)}: deck id: {selectedDeckID}");

            _ = Task.Run(async () =>
            {
                var bytes = await Networking.Instance.GetBytesAsync(
                    Constants.MethodNames.EXPORT_DECK,
                    new Dictionary<string, string>()
                    {
                        { "deckID", selectedDeckID.ToString() },
                    },
                    destroyCancellationToken
                );

                var path = StandaloneFileBrowser.SaveFilePanel("Save deck", "", $"{selectedDeckName}.xlsx", "xlsx");
                if (!string.IsNullOrEmpty(path))
                {
                    File.WriteAllBytes(path, bytes);
                    Debug.Log($"{nameof(ExportDeckButtonBehaviour)}: {nameof(OnClick)}: Saved");
                    canvasService!.ShowNotification("Deck was successfully exported");
                }
                else
                {
                    Debug.Log($"{nameof(ExportDeckButtonBehaviour)}: {nameof(OnClick)}: File name has not been selected");
                }
            });
        }
    }
}
