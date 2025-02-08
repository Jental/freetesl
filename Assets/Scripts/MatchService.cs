#nullable enable

using Assets.DTO;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
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
            var dto = new JoinMatchDTO { playerID = 1 };
            await Networking.Instance.SendMessageAsync(Constants.MethodNames.MATCH_JOIN, dto, destroyCancellationToken);
        }
    }
}
