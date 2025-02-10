#nullable enable

using Assets.Common;
using Assets.DTO;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Services
{
    public class MatchService : MonoBehaviour
    {
        void Start()
        {
            _ = destroyCancellationToken;
            _ = Task.Run(async () => await JoinMatchAsync());
        }

        private async Task JoinMatchAsync()
        {
            var dto = new JoinMatchDTO { playerID = Constants.TEST_PLAYER_ID };
            await Networking.Instance.SendMessageAsync(Constants.MethodNames.MATCH_JOIN, dto, destroyCancellationToken);
        }
    }
}
