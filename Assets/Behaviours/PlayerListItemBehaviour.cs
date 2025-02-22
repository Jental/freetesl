#nullable enable

using Assets.Enums;
using Assets.Models;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class PlayerListItemBehaviour : MonoBehaviour
    {
        public Player? player = null;

        [SerializeField] private RawImage? AvatarImage = null;
        [SerializeField] private RawImage? StateImage = null;
        [SerializeField] private TextMeshProUGUI? NameText = null;

        protected void Start()
        {
            if (this.AvatarImage == null) throw new InvalidOperationException($"{nameof(AvatarImage)} game object is expected to be set");
            if (this.StateImage == null) throw new InvalidOperationException($"{nameof(StateImage)} game object is expected to be set");
            if (this.NameText == null) throw new InvalidOperationException($"{nameof(NameText)} game object is expected to be set");
            if (this.player == null) throw new InvalidOperationException($"{nameof(player)} parameter is expected to be set");
        }

        protected void Update()
        {
            AvatarImage!.texture = Resources.Load<Texture>($"Player/{player!.AvatarName}");
            
            string stateTexturePath = player.State switch
            {
                PlayerState.Offline => "neutral-icon",
                PlayerState.Online => "JoinMatch/online-icon",
                PlayerState.LookingForOpponent => "JoinMatch/friends-icon",
                PlayerState.InMatch => "JoinMatch/swords-icon",
                _ => "neutral-icon",
            };
            StateImage!.texture = Resources.Load<Texture>(stateTexturePath);

            NameText!.text = player.Name;
        }
    }
}
