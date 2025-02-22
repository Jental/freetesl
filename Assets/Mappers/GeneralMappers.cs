#nullable enable

using Assets.DTO;
using Assets.Enums;
using Assets.Models;

namespace Assets.Mappers
{
    public static class GeneralMappers
    {
        public static Player MapFromPlayerInformationDTO(PlayerInformationDTO dto) =>
            new Player(
                dto.id,
                dto.name,
                dto.avatarName,
                (PlayerState)dto.state
            );
    }
}
