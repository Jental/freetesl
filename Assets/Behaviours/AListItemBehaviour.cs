#nullable enable

using Assets.Models;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Behaviours
{
    public abstract class AListItemBehaviour<T> : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public T? Model = default;
        public Action? OnClick = null;

        protected bool isHovered = false;
        protected bool isSelected = false;
        protected bool isRendered = false;
        protected bool changesArePresent = true;

        protected void Start()
        {
            if (this.OnClick == null) throw new InvalidOperationException($"{nameof(OnClick)} parameter is expected to be set");
        }

        protected void Update()
        {
            if (!isRendered) {
                isRendered = true;
                FirstRenderImpl();
            }

            if (changesArePresent)
            {
                changesArePresent = false;
                UpdateImpl();
            }
        }

        protected abstract void FirstRenderImpl();
        protected abstract void UpdateImpl();

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
