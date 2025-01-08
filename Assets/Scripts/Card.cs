using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "NewCard", menuName = "Card")]
    public class Card : ScriptableObject
    {
        public int id;
        public string cardName;
        public string description;
        public Sprite image;
    }
}
