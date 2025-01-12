using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

namespace Assets.Scripts
{
    public class DisplayCard: MonoBehaviour
    {
        public CardInstance? displayCard = null;
        public bool showFront = false;

        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public RawImage image;

        public Image frontEl;
        public RawImage backEl;

        protected void Update()
        {
            frontEl.gameObject.SetActive(showFront);
            backEl.gameObject.SetActive(!showFront);

            if (showFront)
            {
                if (displayCard != null)
                {
                    nameText.text = (string)displayCard.Card.cardName.Clone();
                    descriptionText.text = (string)displayCard.Card.description.Clone();
                    image.texture = displayCard.Card.image.texture;
                }
            }
        }

    }
}
