#nullable enable

using System;

namespace Assets.DTO
{
    [Serializable]
    public record EffectInstanceDTO
    {
        public byte id;
        public string description = string.Empty;
        public string? sourceCardInstanceID;

        private Guid? sourceCardInstanceGuid = null;

        public Guid? SourceCardInstanceGuid
        {
            get
            {
                if (string.IsNullOrEmpty(sourceCardInstanceID)) {
                    return null;
                }
                
                if (sourceCardInstanceGuid == null)
                {
                    sourceCardInstanceGuid = Guid.Parse(sourceCardInstanceID);
                }

                return sourceCardInstanceGuid.Value;
            }
        }
    }
}
