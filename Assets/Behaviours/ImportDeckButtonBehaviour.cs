#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Services;
using SFB;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class ImportDeckButtonBehaviour: MonoBehaviour
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

        private void OnClick()
        {
            Debug.Log($"{nameof(ImportDeckButtonBehaviour)}: {nameof(OnClick)}");

            _ = Task.Run(async () =>
            {
                string[] filePaths = StandaloneFileBrowser.OpenFilePanel("Open deck file", "", "xlsx", false);
                if (filePaths == null || filePaths.Length == 0)
                {
                    Debug.Log($"{nameof(ImportDeckButtonBehaviour)}: {nameof(OnClick)}: No file name has been selected");
                    return;
                }

                byte[] bytes = await File.ReadAllBytesAsync(filePaths[0], destroyCancellationToken);
                var dto = new ImportDeckDTO
                {
                    fileBase64 = Convert.ToBase64String(bytes),
                };

                await Networking.Instance.PostAsync(
                    Constants.MethodNames.IMPORT_DECK,
                    dto,
                    destroyCancellationToken
                );

                deckListBehaviour!.Refresh();
            });
        }
    }
}
