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
    public class PlayerListItemBehaviour : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Player? Player = null;
        public Action? OnClick = null;

        [SerializeField] private RawImage? AvatarImage = null;
        [SerializeField] private RawImage? StateImage = null;
        [SerializeField] private TextMeshProUGUI? NameText = null;
        [SerializeField] private Image? HoverBackground = null;
        [SerializeField] private Image? SelectedBackground = null;

        private bool isHovered = false;
        public bool isSelected = false;
        private bool isRendered = false;
        private bool changesArePresent = true;

        protected void Start()
        {
            if (this.AvatarImage == null) throw new InvalidOperationException($"{nameof(AvatarImage)} game object is expected to be set");
            if (this.StateImage == null) throw new InvalidOperationException($"{nameof(StateImage)} game object is expected to be set");
            if (this.NameText == null) throw new InvalidOperationException($"{nameof(NameText)} game object is expected to be set");
            if (this.HoverBackground == null) throw new InvalidOperationException($"{nameof(HoverBackground)} game object is expected to be set");
            if (this.SelectedBackground == null) throw new InvalidOperationException($"{nameof(SelectedBackground)} game object is expected to be set");
            if (this.Player == null) throw new InvalidOperationException($"{nameof(Player)} parameter is expected to be set");
            if (this.OnClick == null) throw new InvalidOperationException($"{nameof(OnClick)} parameter is expected to be set");
        }

        protected void Update()
        {
            if (!isRendered) {
                isRendered = true;

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

                NameText!.text = Player.Name;
            }

            if (changesArePresent)
            {
                changesArePresent = false;
                HoverBackground!.gameObject.SetActive(isHovered);
                SelectedBackground!.gameObject.SetActive(isSelected);
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                changesArePresent = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick!();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("PlayerListItem.OnPointerEnter");
            isHovered = true;
            changesArePresent = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("PlayerListItem.OnPointerExit");
            isHovered = false;
            changesArePresent = true;
        }
    }
}
