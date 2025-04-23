#nullable enable

using Assets.Enums;

namespace Assets.Models
{
    public record EffectInstance
    {
        public EffectType EffectType { get; private set; }

        public string Description { get; private set; }

        public CardInstance? SourceCardInstance { get; private set; }

        public EffectInstance(EffectType effectType, string description, CardInstance? sourceCardInstance)
        {
            this.EffectType = effectType;
            this.Description = description;
            this.SourceCardInstance = sourceCardInstance;
        }

    }
}
