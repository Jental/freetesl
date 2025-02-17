using UnityEngine;

namespace Assets.Models
{
    [CreateAssetMenu(fileName = "NewCard", menuName = "Card")]
    public class CardScriptableObject : ScriptableObject
    {
        public int id;
        public string cardName;
        public string description;
        public Sprite image;
    }
}
