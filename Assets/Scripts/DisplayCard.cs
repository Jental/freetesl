using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

namespace Assets.Scripts
{
    public class DisplayCard: MonoBehaviour
    {
        public Card? displayCard = null;
        public bool showFront = false;

        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public RawImage image;

        public Image frontEl;
        public RawImage backEl;

        private void Update()
        {
            frontEl.gameObject.SetActive(showFront);
            backEl.gameObject.SetActive(!showFront);

            if (showFront)
            {
                if (displayCard != null)
                {
                    nameText.text = (string)displayCard.cardName.Clone();
                    descriptionText.text = (string)displayCard.description.Clone();
                    image.texture = displayCard.image.texture;
                }
            }
        }

    }
}
