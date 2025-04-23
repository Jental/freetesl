#nullable enable

using Assets.DTO;
using Assets.Enums;
using Assets.Models;
using Assets.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Mappers
{
    public static class MatchMappers
    {
        private static Keyword[] MapToKeywords(byte[] byteValues) =>
            byteValues.Select(b => (Keyword)b).ToArray();

        public static EffectInstance MapFromEffectInstanceDTO(EffectInstanceDTO dto)
        {
            var cardInstance =
                dto.SourceCardInstanceGuid == null
                ? null
                : GlobalStorage.Instance.AllCardInstances[dto.SourceCardInstanceGuid.Value];
            return new EffectInstance((EffectType)dto.id, dto.description, cardInstance);
        }

        public static KeywordInstance MapFromKeywordInstanceDTO(KeywordInstanceDTO dto)
        {
            var cardInstance =
                dto.SourceCardInstanceGuid == null
                ? null
                : GlobalStorage.Instance.AllCardInstances[dto.SourceCardInstanceGuid.Value];
            return new KeywordInstance((Keyword)dto.id, cardInstance);
        }


        public static void MapAndFillKeywordsAndEffects(CardInstanceDTO[] dtos, Dictionary<Guid, CardInstance> allCardInstances)
        {
            foreach (var dto in dtos)
            {
                var effects = dto.effects.Select(MapFromEffectInstanceDTO).ToArray();
                var keywords = dto.keywords.Select(MapFromKeywordInstanceDTO).ToArray();
                var cardInstance = allCardInstances[dto.CardInstanceGuid];
                cardInstance.Effects = effects;
                cardInstance.Keywords = keywords;
            }
        }

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
                // keywords and effects should be handled separately as to map them we need GlobalStorage.Instance.AllCardInstances filled, and that methos uses MapFromCardInstanceDTO for fill
                previous?.IsActive ?? false
            );
    }
}
