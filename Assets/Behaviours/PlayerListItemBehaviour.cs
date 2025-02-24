#nullable enable

using Assets.Enums;
using Assets.Models;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class PlayerListItemBehaviour : MonoBehaviour, IPointerClickHandler
    {
        public Player? Player = null;
        public Action? OnClick = null;
        public bool IsSelected = false;

        [SerializeField] private RawImage? AvatarImage = null;
        [SerializeField] private RawImage? StateImage = null;
        [SerializeField] private TextMeshProUGUI? NameText = null;

        protected void Start()
        {
            if (this.AvatarImage == null) throw new InvalidOperationException($"{nameof(AvatarImage)} game object is expected to be set");
            if (this.StateImage == null) throw new InvalidOperationException($"{nameof(StateImage)} game object is expected to be set");
            if (this.NameText == null) throw new InvalidOperationException($"{nameof(NameText)} game object is expected to be set");
            if (this.Player == null) throw new InvalidOperationException($"{nameof(Player)} parameter is expected to be set");
            if (this.OnClick == null) throw new InvalidOperationException($"{nameof(OnClick)} parameter is expected to be set");
        }

        protected void Update()
        {
            AvatarImage!.texture = Resources.Load<Texture>($"Player/{Player!.AvatarName}");
            
            string stateTexturePath = Player.State switch
            {
                PlayerState.Offline => "neutral-icon",
                PlayerState.Online => "JoinMatch/online-icon",
                PlayerState.LookingForOpponent => "JoinMatch/friends-icon",
                PlayerState.InMatch => "JoinMatch/swords-icon",
                _ => "neutral-icon",
            };
            StateImage!.texture = Resources.Load<Texture>(stateTexturePath);

            NameText!.text = IsSelected ? Player.Name.ToUpper() : Player.Name;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick!();
        }
    }
}
