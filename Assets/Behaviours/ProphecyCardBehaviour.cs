#nullable enable

using Assets.DTO;
using Assets.Enums;
using Assets.Services;
using System.Threading;
using System;
using UnityEngine;
using System.Threading.Tasks;
using Assets.Models;

namespace Assets.Behaviours
{
    public class ProphecyCardBehaviour : AWithMatchStateSubscribtionBehaviour
    {
        [SerializeField] private CardBehaviour? cardPrefab;

        private CardInstance? cardToShow = null;

        protected override void UpdateImpl()
        {
            var children = gameObject.transform.GetComponentsInChildren<CardBehaviour>();
            foreach (var dc in children)
            {
                Destroy(dc.gameObject);
                Destroy(dc);
            }

            if (cardToShow != null)
            {
                var dc =
                    Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.identity)
                    ?? throw new InvalidOperationException("Failed to instantiate a card prefab");
                dc.transform.parent = gameObject.transform;
                dc.UpdateDisplaySettings(
                    cardToShow,
                    playerType == PlayerType.Self ? CardDisplayMode.Full : CardDisplayMode.Cover,
                    isFloating: false
                );

                var cardRect = dc.gameObject.GetComponent<RectTransform>();
                cardRect.anchorMin = new Vector2(0, 0);
                cardRect.anchorMax = new Vector2(1, 1);
                cardRect.offsetMin = new Vector2(0, 0);
                cardRect.offsetMax = new Vector2(0, 0);
                cardRect.localScale = new Vector3(1, 1, 1);
            }
        }

        protected override void VerifyFields()
        {
            if (cardPrefab == null) throw new InvalidOperationException($"ProphecyCardBehaviour: {nameof(cardPrefab)} prefab is not set");
        }

        protected override Task OnMatchStateUpdateAsync(PlayerMatchStateDTO dto, MatchStateDTO _, bool isPlayersTurn, CancellationToken cancellationToken)
        {
            Debug.Log("ProphecyCardBehaviour.OnMatchStateUpdateAsync");

            var cardInstanceStr = dto.cardInstanceWaitingForAction;
            if (string.IsNullOrEmpty(cardInstanceStr))
            {
                cardToShow = null;
                return Task.CompletedTask;
            }

            var cardInstanceID = Guid.Parse(cardInstanceStr);
            cardToShow = GlobalStorage.Instance.AllCardInstances[cardInstanceID];
            cardToShow.IsActive = true;

            return Task.CompletedTask;
        }
    }
}
