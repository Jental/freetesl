#nullable enable

using Assets.Behaviours;
using Assets.Common;
using Assets.DTO;
using Assets.Enums;
using Assets.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Services
{
    public class CardDragAndDropService : MonoBehaviour
    {
        [SerializeField] private LaneCardsBehaviour? ownLeftLaneCards;
        [SerializeField] private LaneCardsBehaviour? ownRightLaneCards;
        [SerializeField] private HandBehaviour? ownHand;

        protected void Start()
        {
            if (ownLeftLaneCards == null) throw new InvalidOperationException($"{ownLeftLaneCards} gameObject is expected to be set");
            if (ownRightLaneCards == null) throw new InvalidOperationException($"{ownRightLaneCards} gameObject is expected to be set");
            if (ownHand == null) throw new InvalidOperationException($"{ownHand} gameObject is expected to be set");
        }

        public static (CardDragSource source, bool isOwn) GetCardDragSource(GameObject draggedCard)
        {
            var parentHandComponent = draggedCard.GetComponentInParent<HandBehaviour>();
            var parentLaneCardsComponent = draggedCard.GetComponentInParent<LaneCardsBehaviour>();
            var parentProphecyComponent = draggedCard.GetComponentInParent<ProphecyCardBehaviour>();

            if (parentHandComponent != null)
            {
                return (CardDragSource.Hand, parentHandComponent.playerType == PlayerType.Self);
            }
            else if (parentLaneCardsComponent != null)
            {
                return parentLaneCardsComponent.LaneID switch
                {
                    LaneType.Left => (CardDragSource.LeftLane, parentLaneCardsComponent.playerType == PlayerType.Self),
                    LaneType.Right => (CardDragSource.RightLane, parentLaneCardsComponent.playerType == PlayerType.Self),
                    _ => throw new InvalidOperationException($"Invalid lane id: {parentLaneCardsComponent.LaneID}"),
                };
            }
            else if (parentProphecyComponent != null)
            {
                return (CardDragSource.Prophecy, parentProphecyComponent.playerType == PlayerType.Self);
            }
            else
            {
                throw new InvalidOperationException($"{nameof(CardDragAndDropService)}.{nameof(GetCardDragSource)}: unknown position");
            }
        }

        public void CardDrop(
            CardInstance droppedCardInstance,
            CardDragSource droppedCardSource,
            CardBehaviour droppedCardBehaviour,
            CardDragSource targetCardSource,
            bool targetIsOwn,
            CardInstance? targetCardInstance,
            CancellationToken cancellationToken
        )
        {
            switch (droppedCardInstance.Card.Type, droppedCardSource, targetCardSource, targetIsOwn)
            {
                case (CardType.Creature, CardDragSource.Hand, CardDragSource.LeftLane, true):
                case (CardType.Creature, CardDragSource.Hand, CardDragSource.RightLane, true):
                    CreatureCardDropFromOwnHandToOwnLane(
                        droppedCardInstance,
                        droppedCardBehaviour,
                        targetCardSource == CardDragSource.LeftLane ? LaneType.Left : LaneType.Right,
                        cancellationToken
                    );
                    break;
                case (CardType.Creature, CardDragSource.Prophecy, CardDragSource.LeftLane, true):
                case (CardType.Creature, CardDragSource.Prophecy, CardDragSource.RightLane, true):
                    CreatureCardDropFromOwnProphecyToOwnLane(
                        droppedCardInstance,
                        droppedCardBehaviour,
                        targetCardSource == CardDragSource.LeftLane ? LaneType.Left : LaneType.Right,
                        cancellationToken
                    );
                    break;
                case (CardType.Creature, CardDragSource.Prophecy, CardDragSource.Hand, true):
                    CardDropFromOwnProphecyToOwnHand(
                        droppedCardInstance,
                        droppedCardBehaviour,
                        cancellationToken
                    );
                    break;
                case (CardType.Creature, CardDragSource.LeftLane, CardDragSource.LeftLane, false):
                case (CardType.Creature, CardDragSource.RightLane, CardDragSource.RightLane, false):
                    CreatureCardDropFromOwnLaneToOpponentLaneCard(
                        droppedCardInstance,
                        droppedCardSource == CardDragSource.LeftLane ? LaneType.Left : LaneType.Right,
                        targetCardInstance ?? throw new InvalidOperationException($"{nameof(CardDrop)}: {nameof(targetCardInstance)} should be supplied"),
                        targetCardSource == CardDragSource.LeftLane ? LaneType.Left : LaneType.Right,
                        cancellationToken
                    );
                    break;
                case (CardType.Creature, CardDragSource.LeftLane, CardDragSource.Face, false):
                case (CardType.Creature, CardDragSource.RightLane, CardDragSource.Face, false):
                    CreatureCardDropFromOwnLaneToOpponentFace(
                        droppedCardInstance,
                        cancellationToken
                    );
                    break;
                case (CardType.Action, CardDragSource.Hand, CardDragSource.LeftLane, _):
                case (CardType.Action, CardDragSource.Hand, CardDragSource.RightLane, _):
                    ActionCardDropFromOwnHandToLaneCard(
                        droppedCardInstance,
                        targetCardInstance ?? throw new InvalidOperationException($"{nameof(CardDrop)}: {nameof(targetCardInstance)} should be supplied"),
                        cancellationToken
                    );
                    break;
                case (CardType.Action, CardDragSource.Hand, CardDragSource.Face, false):
                    ActionCardDropFromOwnHandToOpponentFace(
                        droppedCardInstance,
                        droppedCardBehaviour,
                        cancellationToken
                    );
                    break;
                case (CardType.Action, CardDragSource.Prophecy, CardDragSource.LeftLane, _):
                case (CardType.Action, CardDragSource.Prophecy, CardDragSource.RightLane, _):
                    ActionCardDropFromProphecyToLaneCard(
                        droppedCardInstance,
                        droppedCardBehaviour,
                        targetCardInstance ?? throw new InvalidOperationException($"{nameof(CardDrop)}: {nameof(targetCardInstance)} should be supplied"),
                        cancellationToken
                    );
                    break;
                case (CardType.Action, CardDragSource.Prophecy, CardDragSource.Face, false):
                    ActionCardDropFromProphecyToOpponentFace(
                        droppedCardInstance,
                        droppedCardBehaviour,
                        cancellationToken
                    );
                    break;
                case (CardType.Action, CardDragSource.Prophecy, CardDragSource.Hand, true):
                    CardDropFromOwnProphecyToOwnHand(
                        droppedCardInstance,
                        droppedCardBehaviour,
                        cancellationToken
                    );
                    break;
                case (CardType.Item, CardDragSource.Hand, CardDragSource.LeftLane, _):
                case (CardType.Item, CardDragSource.Hand, CardDragSource.RightLane, _):
                    ItemCardDropFromOwnHandToLaneCard(
                        droppedCardInstance,
                        targetCardInstance ?? throw new InvalidOperationException($"{nameof(CardDrop)}: {nameof(targetCardInstance)} should be supplied"),
                        cancellationToken
                    );
                    break;
                case (CardType.Item, CardDragSource.Prophecy, CardDragSource.Hand, true):
                    CardDropFromOwnProphecyToOwnHand(
                        droppedCardInstance,
                        droppedCardBehaviour,
                        cancellationToken
                    );
                    break;
                case (CardType.Support, CardDragSource.Prophecy, CardDragSource.Hand, true):
                    CardDropFromOwnProphecyToOwnHand(
                        droppedCardInstance,
                        droppedCardBehaviour,
                        cancellationToken
                    );
                    break;
                default:
                    _ = Task.Run(() =>
                    {
                        Debug.Log($"{nameof(CardDragAndDropService)}.{nameof(CardDrop)}: drop not accepted: {droppedCardInstance.Card.Type}, {droppedCardSource} -> {targetCardSource}, own:{targetIsOwn}");
                        droppedCardBehaviour.ReturnBack();
                    });
                    break;
            };
        }

        private void CreatureCardDropFromOwnHandToOwnLane(
            CardInstance droppedCardInstance,
            CardBehaviour droppedCardBehaviour,
            LaneType targetLaneType,
            CancellationToken cancellationToken
        )
        {
            Debug.Log($"{nameof(CardDragAndDropService)}.{nameof(CreatureCardDropFromOwnHandToOwnLane)}: {droppedCardInstance.ID} [{droppedCardInstance.Card.ScriptableObject.cardName}] -> {targetLaneType}");

            droppedCardInstance.IsActive = false;

            var laneCardsGameObject = targetLaneType switch
            {
                LaneType.Left => ownLeftLaneCards,
                LaneType.Right => ownRightLaneCards,
                _ => throw new ArgumentException($"{nameof(CardDragAndDropService)}.{nameof(CreatureCardDropFromOwnHandToOwnLane)}: invalid lane type: {targetLaneType}")
            };
            laneCardsGameObject!.AddCard(droppedCardInstance);

            Destroy(droppedCardBehaviour.gameObject);
            Destroy(droppedCardBehaviour);

            _ = Task.Run(async () =>
            {
                var dto = new MoveCardToLaneDTO
                {
                    cardInstanceID = droppedCardInstance.ID.ToString(),
                    laneID = (byte)targetLaneType,
                };
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.MOVE_CARD_TO_LANE, dto, cancellationToken);
            });
        }

        private void CreatureCardDropFromOwnProphecyToOwnLane(
            CardInstance droppedCardInstance,
            CardBehaviour droppedCardBehaviour,
            LaneType targetLaneType,
            CancellationToken cancellationToken
        )
        {
            Debug.Log($"{nameof(CardDragAndDropService)}.{nameof(CreatureCardDropFromOwnProphecyToOwnLane)}: {droppedCardInstance.ID} [{droppedCardInstance.Card.ScriptableObject.cardName}] -> {targetLaneType}");

            droppedCardInstance.IsActive = false;

            var laneCardsGameObject = targetLaneType switch
            {
                LaneType.Left => ownLeftLaneCards,
                LaneType.Right => ownRightLaneCards,
                _ => throw new ArgumentException($"{nameof(CardDragAndDropService)}.{nameof(CreatureCardDropFromOwnHandToOwnLane)}: invalid lane type: {targetLaneType}")
            };
            laneCardsGameObject!.AddCard(droppedCardInstance);

            Destroy(droppedCardBehaviour.gameObject);
            Destroy(droppedCardBehaviour);

            _ = Task.Run(async () =>
            {
                var dto = new DrawCardToLaneDTO
                {
                    laneID = (byte)targetLaneType,
                };
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.DRAW_CARD_TO_LANE, dto, cancellationToken);
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.WAITED_USER_ACTIONS_COMPLETED, cancellationToken);
            });
        }

        private void CardDropFromOwnProphecyToOwnHand( // card type does not matter
            CardInstance droppedCardInstance,
            CardBehaviour droppedCardBehaviour,
            CancellationToken cancellationToken
        )
        {
            Debug.Log($"{nameof(CardDragAndDropService)}.{nameof(CardDropFromOwnProphecyToOwnHand)}: {droppedCardInstance.ID} [{droppedCardInstance.Card.ScriptableObject.cardName}]");

            droppedCardInstance.IsActive = false;

            ownHand!.AddCard(droppedCardInstance);

            Destroy(droppedCardBehaviour.gameObject);
            Destroy(droppedCardBehaviour);

            _ = Task.Run(async () =>
            {
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.DRAW_CARD, cancellationToken);
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.WAITED_USER_ACTIONS_COMPLETED, cancellationToken);
            });
        }

        private void CreatureCardDropFromOwnLaneToOpponentLaneCard(
            CardInstance droppedCardInstance,
            LaneType droppedCardLaneType,
            CardInstance targetCardInstance,
            LaneType targetCardLaneType,
            CancellationToken cancellationToken
        )
        {
            Debug.Log($"{nameof(CardDragAndDropService)}.{nameof(CreatureCardDropFromOwnLaneToOpponentLaneCard)}: {droppedCardInstance.ID} [{droppedCardInstance.Card.ScriptableObject.cardName}] -> {targetCardInstance.ID} [{targetCardInstance.Card.ScriptableObject.cardName}]");

            if (droppedCardLaneType != targetCardLaneType) throw new InvalidOperationException($"{nameof(CardDragAndDropService)}.{nameof(CreatureCardDropFromOwnLaneToOpponentLaneCard)}: drop not accepted - other lane");

            // targetCardInstance.Health = targetCardInstance.Health - droppedCardInstance.Power;
            // droppedCardInstance.Health = droppedCardInstance.Health - targetCardInstance.Power;
            droppedCardInstance.IsActive = false;

            _ = Task.Run(async () =>
            {
                var dto = new HitCardDTO
                {
                    cardInstanceID = droppedCardInstance.ID.ToString(),
                    opponentCardInstanceID = targetCardInstance.ID.ToString(),
                };
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.HIT_CARD, dto, cancellationToken);
            });
        }

        private void CreatureCardDropFromOwnLaneToOpponentFace(
            CardInstance droppedCardInstance,
            CancellationToken cancellationToken
        )
        {
            Debug.Log($"{nameof(CardDragAndDropService)}.{nameof(CreatureCardDropFromOwnLaneToOpponentFace)}: {droppedCardInstance.ID} [{droppedCardInstance.Card.ScriptableObject.cardName}]");

            droppedCardInstance.IsActive = false;

            _ = Task.Run(async () =>
            {
                var dto = new HitFaceDTO
                {
                    cardInstanceID = droppedCardInstance.ID.ToString(),
                };
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.HIT_FACE, dto, cancellationToken);
            });
        }

        private void ActionCardDropFromOwnHandToLaneCard( // it does not matter own or opponent
            CardInstance droppedCardInstance,
            CardInstance targetCardInstance,
            CancellationToken cancellationToken
        )
        {
            Debug.Log($"{nameof(CardDragAndDropService)}.{nameof(ActionCardDropFromOwnHandToLaneCard)}: {droppedCardInstance.ID} [{droppedCardInstance.Card.ScriptableObject.cardName}] -> {targetCardInstance.ID} [{targetCardInstance.Card.ScriptableObject.cardName}]");

            droppedCardInstance.IsActive = false;

            _ = Task.Run(async () =>
            {
                var dto = new ApplyCardToCardDTO
                {
                    cardInstanceID = droppedCardInstance.ID.ToString(),
                    opponentCardInstanceID = targetCardInstance.ID.ToString(),
                };
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.APPLY_CARD_TO_CARD, dto, cancellationToken);
            });
        }

        private void ActionCardDropFromOwnHandToOpponentFace(
            CardInstance droppedCardInstance,
            CardBehaviour droppedCardBehaviour,
            CancellationToken cancellationToken
        )
        {
            Debug.Log($"{nameof(CardDragAndDropService)}.{nameof(ActionCardDropFromOwnHandToOpponentFace)}: {droppedCardInstance.ID} [{droppedCardInstance.Card.ScriptableObject.cardName}]");

            // droppedCardInstance.IsActive = false;

            _ = Task.Run(() =>
            {
                droppedCardBehaviour.ReturnBack();
                throw new NotImplementedException();
                // await Networking.Instance.SendMessageAsync(Constants.MethodNames.APPLY_ACTION_TO_FACE, dto, cancellationToken);
            });
        }

        private void ActionCardDropFromProphecyToLaneCard( // it does not matter own or opponent
            CardInstance droppedCardInstance,
            CardBehaviour droppedCardBehaviour,
            CardInstance targetCardInstance,
            CancellationToken cancellationToken
        )
        {
            Debug.Log($"{nameof(CardDragAndDropService)}.{nameof(ActionCardDropFromProphecyToLaneCard)}: {droppedCardInstance.ID} [{droppedCardInstance.Card.ScriptableObject.cardName}] -> {targetCardInstance.ID} [{targetCardInstance.Card.ScriptableObject.cardName}]");

            // droppedCardInstance.IsActive = false;

            _ = Task.Run(() =>
            {
                droppedCardBehaviour.ReturnBack();
                throw new NotImplementedException();
                //await Networking.Instance.SendMessageAsync(Constants.MethodNames.DRAW_AND_APPLY_ACTION_TO_CARD, dto, cancellationToken);
                //await Networking.Instance.SendMessageAsync(Constants.MethodNames.WAITED_USER_ACTIONS_COMPLETED, cancellationToken);
            });
        }

        private void ActionCardDropFromProphecyToOpponentFace(
            CardInstance droppedCardInstance,
            CardBehaviour droppedCardBehaviour,
            CancellationToken cancellationToken
        )
        {
            Debug.Log($"{nameof(CardDragAndDropService)}.{nameof(ActionCardDropFromProphecyToOpponentFace)}: {droppedCardInstance.ID} [{droppedCardInstance.Card.ScriptableObject.cardName}]");

            // droppedCardInstance.IsActive = false;

            _ = Task.Run(() =>
            {
                droppedCardBehaviour.ReturnBack();
                throw new NotImplementedException();
                // await Networking.Instance.SendMessageAsync(Constants.MethodNames.DRAW_AND_APPLY_ACTION_TO_FACE, dto, cancellationToken);
            });
        }

        private void ItemCardDropFromOwnHandToLaneCard( // it does not matter own or opponent
            CardInstance droppedCardInstance,
            CardInstance targetCardInstance,
            CancellationToken cancellationToken
        )
        {
            Debug.Log($"{nameof(CardDragAndDropService)}.{nameof(ItemCardDropFromOwnHandToLaneCard)}: {droppedCardInstance.ID} [{droppedCardInstance.Card.ScriptableObject.cardName}] -> {targetCardInstance.ID} [{targetCardInstance.Card.ScriptableObject.cardName}]");

            droppedCardInstance.IsActive = false;

            _ = Task.Run(async () =>
            {
                var dto = new ApplyCardToCardDTO
                {
                    cardInstanceID = droppedCardInstance.ID.ToString(),
                    opponentCardInstanceID = targetCardInstance.ID.ToString(),
                };
                await Networking.Instance.SendMessageAsync(Constants.MethodNames.APPLY_CARD_TO_CARD, dto, cancellationToken);
            });
        }
    }
}
