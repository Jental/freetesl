#nullable enable

using UnityEngine;

namespace Assets.Services
{
    public class MatchService : MonoBehaviour
    {
        void Start()
        {
            _ = destroyCancellationToken;
            GlobalStorage.Instance.Init();
            Networking.Instance.ConnectAndListen(destroyCancellationToken);
        }
    }
}
