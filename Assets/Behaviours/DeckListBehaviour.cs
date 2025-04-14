#nullable enable

using Assets.Common;
using Assets.DTO;
using Assets.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Behaviours
{
    public class DeckListBehaviour : AListBehaviour<DeckDTO, DeckListItemBehaviour>
    {
        protected new void Start()
        {
            base.Start();
            Refresh();
        }

        protected override async Task RefreshImplAsync(CancellationToken cancellationToken)
        {
            await LoadDecksAsync(cancellationToken);
        }

        private async Task LoadDecksAsync(CancellationToken cancellationToken)
        {
            ListDTO<DeckDTO>? dtos;
            try
            {
                dtos = await Networking.Instance.GetAsync<ListDTO<DeckDTO>>(
                    Constants.MethodNames.GET_DECKS,
                    new Dictionary<string, string>(),
                    cancellationToken
                );
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            if (dtos?.items == null)
            {
                Debug.LogError("Deck list is null");
                return;
            }

            ModelsToShow = dtos.items;
        }
    }
}