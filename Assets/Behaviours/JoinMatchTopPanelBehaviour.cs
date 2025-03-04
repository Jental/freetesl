#nullable enable

using Assets.Services;
using System;
using TMPro;
using UnityEngine;

namespace Assets.Behaviours
{
    public class JoinMatchTopPanelBehaviour : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI? playerNameGameObject;
        [SerializeField] private TextMeshProUGUI? serverGameObject;

        protected void Start()
        {
            if (playerNameGameObject == null) throw new InvalidOperationException($"{nameof(playerNameGameObject)} game object is expected to be set");
            if (serverGameObject == null) throw new InvalidOperationException($"{nameof(serverGameObject)} game object is expected to be set");
        }

        protected void Update()
        {
            playerNameGameObject!.text = $"user: {GlobalStorage.Instance.PlayerLogin}";
            serverGameObject!.text = $"server: {GlobalStorage.Instance.CurrentServer}";
        }
    }

}