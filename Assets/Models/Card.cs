using Assets.Enums;

namespace Assets.Models
{
    public record Card
    {
        public int ID { get; private set; }
        public int Power { get; private set; }
        public int Health { get; private set; }
        public int Cost { get; private set; }
        public CardType Type { get; private set; }
        public Keyword[] Keywords { get; private set; }

        public CardScriptableObject ScriptableObject { get; private set; }

        public Card(int id, CardScriptableObject scriptableObject, int power, int health, int cost, CardType type, Keyword[] keywords)
        {
            ID = id;
            Power = power;
            Health = health;
            Cost = cost;
            Type = type;
            Keywords = keywords;

            ScriptableObject = scriptableObject;
        }
    }
}
