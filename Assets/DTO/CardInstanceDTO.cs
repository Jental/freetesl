﻿#nullable enable

using System;

namespace Assets.DTO
{
    [Serializable]
    public record CardInstanceDTO
    {
        public int cardID;
        public string cardInstanceID;
        public int power;
        public int health;
        public int cost;
        public byte[] keywords;

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
