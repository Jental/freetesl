using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(fileName = "NewCard", menuName = "Card")]
    public class Card : ScriptableObject
    {
        public int id;
        public string cardName;
        public string description;
        public int power;
        public int health;
        public int cost;
        public Sprite image;
    }
}
