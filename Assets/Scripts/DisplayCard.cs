using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

namespace Assets.Scripts
{
    public class DisplayCard: MonoBehaviour
    {
        public Card? displayCard = null;

        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public RawImage image;

        private void Update()
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
