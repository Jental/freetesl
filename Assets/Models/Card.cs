using UnityEngine;

namespace Assets.Models
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
