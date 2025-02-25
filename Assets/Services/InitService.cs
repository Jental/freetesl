#nullable enable

using UnityEngine;

namespace Assets.Services
{
    public class InitService : MonoBehaviour
    {
        void Start()
        {
            GlobalStorage.Instance.Init();
        }
    }
}
