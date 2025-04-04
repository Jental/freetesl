using System;

namespace Assets.DTO
{
    [Serializable]
    public record ShowCardActionSelectDTO
    {
        public string cardInstanceID;

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
