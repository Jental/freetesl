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
            GlobalStorage.Instance.Init();
            _ = Task.Run(async () => await JoinMatchAsync());
        }

        private async Task JoinMatchAsync()
        {
            Networking.Instance.ConnectAndListen(destroyCancellationToken);
            await Networking.Instance.SendMessageAsync(Constants.MethodNames.MATCH_JOIN, destroyCancellationToken);
        }
    }
}
