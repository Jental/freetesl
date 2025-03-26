using Assets.Common;
using Assets.DTO;
using Assets.Enums;
using Assets.Services;
using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

namespace Assets.Behaviours
{
    public class DiscardPileButtonBehaviour: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private PlayerType playerType = PlayerType.Self;
        [SerializeField] private TooltipBehaviour tooltipGameObject;

        private List<Action> unsubscribers = new List<Action>();
        private bool changesArePresent = false;
        private bool isEnabled = false;
        private int cardCount = 0;

        protected void Start()
        {
            if (tooltipGameObject == null) throw new InvalidOperationException("DiscardPileButtonBehaviour: Tooltip game object is not set");

            var uss = Networking.Instance.Subscribe(Constants.MethodNames.DISCARD_PILE_STATE_UPDATE, OnDiscardPileStateUpdateAsync);
            unsubscribers.Add(uss);

            _ = destroyCancellationToken;
        }

        protected void OnDestroy()
        {
            foreach (var uss in unsubscribers)
            {
                uss();
            }
        }

        protected void OnEnable()
        {
            isEnabled = true;
        }

        protected void OnDisable()
        {
            isEnabled = false;
        }

        protected void Update()
        {
            if (!changesArePresent) return;
            changesArePresent = false;
        }

        public void OnPointerEnter(PointerEventData _)
        {
            tooltipGameObject.Show($"{cardCount} cards");
        }

        public void OnPointerExit(PointerEventData _)
        {
            tooltipGameObject.Hide();
        }

        private Task OnDiscardPileStateUpdateAsync(string methodName, string message, CancellationToken cancellationToken)
        {
            if (!isEnabled) return Task.CompletedTask;

            Debug.Log("OnDiscardPileStateUpdateAsync");
            try
            {
                ServerMessageDTO<DiscardPileStateUpdateDTO> dto = JsonUtility.FromJson<ServerMessageDTO<DiscardPileStateUpdateDTO>>(message);

                var deckState = playerType switch
                {
                    PlayerType.Self => dto.body.player,
                    PlayerType.Opponent => dto.body.opponent,
                    _ => throw new InvalidOperationException($"Unsupported {nameof(playerType)}: '{playerType}'")
                };

                Debug.Log($"OnDiscardPileStateUpdateAsync: [{playerType}]: counts: {deckState.Length}");

                cardCount = deckState.Length;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return Task.FromException(e);
            }

            changesArePresent = true;

            return Task.CompletedTask;
        }

    }
}
