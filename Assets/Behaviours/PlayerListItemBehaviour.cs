#nullable enable

using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Assets.Models;
using System;
using Assets.Enums;

namespace Assets.Behaviours
{
    public class PlayerListItemBehaviour : AListItemBehaviour<Player>
    {
        [SerializeField] private RawImage? AvatarImage = null;
        [SerializeField] private RawImage? StateImage = null;
        [SerializeField] private TextMeshProUGUI? NameText = null;
        [SerializeField] private Image? HoverBackground = null;
        [SerializeField] private Image? SelectedBackground = null;

        public new void Start()
        {
            base.Start();

            if (this.AvatarImage == null) throw new InvalidOperationException($"{nameof(AvatarImage)} game object is expected to be set");
            if (this.StateImage == null) throw new InvalidOperationException($"{nameof(StateImage)} game object is expected to be set");
            if (this.NameText == null) throw new InvalidOperationException($"{nameof(NameText)} game object is expected to be set");
            if (this.HoverBackground == null) throw new InvalidOperationException($"{nameof(HoverBackground)} game object is expected to be set");
            if (this.SelectedBackground == null) throw new InvalidOperationException($"{nameof(SelectedBackground)} game object is expected to be set");
        }

        protected override void FirstRenderImpl()
        {
            AvatarImage!.texture = Resources.Load<Texture>($"Avatars/{Model!.AvatarName}");

            string stateTexturePath = Model.State switch
            {
                PlayerState.Offline => "neutral-icon",
                PlayerState.Online => "JoinMatch/online-icon",
                PlayerState.LookingForOpponent => "JoinMatch/friends-icon",
                PlayerState.InMatch => "JoinMatch/swords-icon",
                _ => "neutral-icon",
            };
            StateImage!.texture = Resources.Load<Texture>(stateTexturePath);

            NameText!.text = Model.Name;
        }

        protected override void UpdateImpl()
        {
            HoverBackground!.gameObject.SetActive(isHovered);
            SelectedBackground!.gameObject.SetActive(isSelected);
        }
    }
}
