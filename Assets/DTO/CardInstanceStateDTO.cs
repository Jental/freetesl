#nullable enable

using System;

namespace Assets.DTO
{
    [Serializable]
    public record CardInstanceStateDTO
    {
        public string cardInstanceID;
        public bool isActive;

        private Guid? cardInstanceGuid = null;

        public Guid CardInstanceGuid
        {
            get
            {
                if (cardInstanceGuid == null)
                {
                    cardInstanceGuid = Guid.Parse(cardInstanceID);
                }
                else if (string.IsNullOrEmpty(cardInstanceID))
                {
                    throw new InvalidOperationException("Card instance id should be a not emnpy guid string");
                }

                return cardInstanceGuid.Value;
            }
        }
    }
}
