using System;

namespace Assets.DTO
{
    [Serializable]
    public record CardInstanceDTO
    {
        public int cardID;
        public string cardInstanceID;

        private Guid? cardInstanceGuid = null;

        public Guid? CardInstanceGuid
        {
            get
            {
                if (cardInstanceGuid == null && !string.IsNullOrEmpty(cardInstanceID))
                {
                    cardInstanceGuid = Guid.Parse(cardInstanceID);
                }
                return cardInstanceGuid;
            }
        }
    }
}
