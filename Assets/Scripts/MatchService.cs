#nullable enable

using Assets.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class MatchService : MonoBehaviour
    {
        public GameObject? HandGameObject = null;
        public DisplayCard? CardPrefab;

        private List<Action> unsubscribers = new List<Action>();
        private bool changesArePresent = false;

        private Dictionary<int, Card>? allCards;
        private List<CardInstance> cardsToShow = new List<CardInstance>();

        private Dictionary<int, Card> AllCardsNotNull => allCards ?? throw new InvalidOperationException("All cards collection is not initialized");


        void Start()
        {
            allCards = Resources.LoadAll<Card>("CardObjects").ToDictionary(c => c.id, c => c);

            var uss = Networking.Instance.Subscribe(Constants.MethodNames.MATCH_STATE_UPDATE, OnMatchStateUpdateAsync);
            unsubscribers.Add(uss);

            _ = destroyCancellationToken;

            var joinDTO = new JoinMatchDTO { playerID = 1 };
            _ = Task.Run(async () => await JoinMatchAsync());
        }

        void Update()
        {
            lock (this)
            {
                if (!changesArePresent)
                {
                    return;
                }

                if (HandGameObject == null)
                {
                    throw new InvalidOperationException("HandGameObject is not set");
                }
                if (CardPrefab == null)
                {
                    throw new InvalidOperationException("CardPrefab is not set");
                }

                var children = HandGameObject.transform.GetComponentsInChildren<DisplayCard>();
                foreach (var dc in children)
                {
                    Destroy(dc.gameObject);
                    Destroy(dc);
                }

                if (cardsToShow.Count > 0)
                {
                    var handRect = HandGameObject.GetComponent<RectTransform>();
                    float cardHeight = handRect.rect.height;
                    float cardWidth = cardHeight * Constants.CARD_ASPECT_RATIO;
                    float cardWidthShare = cardWidth / handRect.rect.width;
                    float totalCardWidthShare = cardWidthShare * (1 - Constants.HAND_CARD_OVERFLOW) * cardsToShow.Count + cardWidthShare * Constants.HAND_CARD_OVERFLOW;
                    float marginWidthShare = Math.Max(0.0f, (1 - totalCardWidthShare) / 2.0f);

                    for (int i = 0; i < cardsToShow.Count; i++)
                    {
                        var card = cardsToShow[i];

                        var dc = Instantiate(CardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        dc.transform.parent = HandGameObject.transform;
                        dc.displayCard = card;
                        dc.showFront = true;

                        var cardRect = dc.gameObject.GetComponent<RectTransform>();
                        cardRect.anchorMin = new Vector2(marginWidthShare + cardWidthShare * i * (1 - Constants.HAND_CARD_OVERFLOW), 0);
                        cardRect.anchorMax = new Vector2(marginWidthShare + cardWidthShare * (i * (1 - Constants.HAND_CARD_OVERFLOW) + 1), 1);
                        cardRect.offsetMin = new Vector2(0, 0);
                        cardRect.offsetMax = new Vector2(0, 0);
                        cardRect.localScale = new Vector3(1, 1, 1);
                    }
                }

                changesArePresent = false;
            }
        }

        private void OnDestroy()
        {
            foreach (var uss in unsubscribers)
            {
                uss.Invoke();
            }
        }

        private async Task JoinMatchAsync()
        {
            var dto = new JoinMatchDTO { playerID = 1 };
            await Networking.Instance.SendMessageAsync(Constants.MethodNames.MATCH_JOIN, dto, destroyCancellationToken);
        }

        private async Task OnMatchStateUpdateAsync(string message, CancellationToken cancellationToken)
        {
            ServerMessageDTO<PlayerMatchStateDTO> dto = JsonUtility.FromJson<ServerMessageDTO<PlayerMatchStateDTO>>(message);
            Debug.Log($"MatchService.OnMatchStateUpdateAsync: Counts: {dto.body.deck.Length}, {dto.body.hand.Length}");

            try
            {
                cardsToShow = dto.body.hand.Select(c => new CardInstance(
                    AllCardsNotNull[c.cardID],
                    c.CardInstanceGuid ?? throw new InvalidOperationException($"Card instance id is null or is not a guid: '{c.cardInstanceID}'")
                )).ToList();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            changesArePresent = true;
        }
    }
}
