#nullable enable

using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System;
using Assets.DTO;

namespace Assets.Behaviours
{
    public class DeckListItemBehaviour : AListItemBehaviour<DeckDTO>
    {
        [SerializeField] private RawImage? AvatarImage;
        [SerializeField] private GameObject? AttributesGameObject;
        [SerializeField] private DeckListAttributeBehaviour? AttributePrefab;
        [SerializeField] private TextMeshProUGUI? NameText;
        [SerializeField] private Image? HoverBackground;
        [SerializeField] private Image? SelectedBackground;

        public new void Start()
        {
            base.Start();

            if (this.AvatarImage == null) throw new InvalidOperationException($"{nameof(AvatarImage)} game object is expected to be set");
            if (this.AttributesGameObject == null) throw new InvalidOperationException($"{nameof(AttributesGameObject)} game object is expected to be set");
            if (this.AttributePrefab == null) throw new InvalidOperationException($"{nameof(AttributePrefab)} prefab is expected to be set");
            if (this.NameText == null) throw new InvalidOperationException($"{nameof(NameText)} game object is expected to be set");
            if (this.HoverBackground == null) throw new InvalidOperationException($"{nameof(HoverBackground)} game object is expected to be set");
            if (this.SelectedBackground == null) throw new InvalidOperationException($"{nameof(SelectedBackground)} game object is expected to be set");
        }

        protected override void FirstRenderImpl()
        {
            NameText!.text = Model!.name;
            if (Model.avatarName != null)
            {
                AvatarImage!.texture = Resources.Load<Texture>($"Avatars/{Model.avatarName}");
                AvatarImage.gameObject.SetActive(true);
            }

            foreach(var attr in Model.attributes)
            {
                var dc =
                    Instantiate(AttributePrefab, new Vector3(0, 0, 0), Quaternion.identity)
                    ?? throw new InvalidOperationException("Failed to instantiate a deck list attribute prefab");
                dc.transform.parent = AttributesGameObject!.transform;
                dc.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                
                string texturePath = $"{attr}-icon";
                dc.Image = Resources.Load<Texture>(texturePath);
            }
        }

        protected override void UpdateImpl()
        {
            HoverBackground!.gameObject.SetActive(isHovered);
            SelectedBackground!.gameObject.SetActive(isSelected);
        }
    }
}
