#nullable enable

using Assets.DTO;
using Assets.Enums;
using Assets.Models;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Mappers
{
    public static class MatchMappers
    {
        private static Keyword[] MapToKeywords(byte[] byteValues) =>
            byteValues.Select(b => (Keyword)b).ToArray();

        public static Card MapFromCardDTO(CardDTO dto, Dictionary<int, CardScriptableObject> allCardScriptableObjects) =>
            new Card(
                dto.id,
                allCardScriptableObjects[dto.id],
                dto.power,
                dto.health,
                dto.cost,
                (CardType)dto.type,
                MapToKeywords(dto.keywords)
            );

        public static CardInstance MapFromCardInstanceDTO(CardInstanceDTO dto, CardInstance? previous, Dictionary<int, Card> allCards) =>
            new CardInstance(
                dto.CardInstanceGuid,
                allCards[dto.cardID],
                dto.power,
                dto.health,
                dto.cost,
                MapToKeywords(dto.keywords),
                previous?.IsActive ?? false
            );
    }
}
