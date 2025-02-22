using Assets.Enums;

namespace Assets.Models
{
    public record Player
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public string AvatarName { get; private set; }
        public PlayerState State { get; private set; }

        public Player(int id, string name, string avatarName, PlayerState state)
        {
            ID = id;
            Name = name;
            AvatarName = avatarName;
            State = state;
        }
    }
}
