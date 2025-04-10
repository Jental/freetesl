#nullable enable

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class DeckListAttributeBehaviour: MonoBehaviour
    {
        [SerializeField] private RawImage? imageGameObject;
        public Texture? Image;

        private bool isRendered = false;

        protected void Start()
        {
            if (this.Image == null) throw new InvalidOperationException($"{nameof(Image)} texture is expected to be set");
            if (this.imageGameObject == null) throw new InvalidOperationException($"{nameof(imageGameObject)} game object is expected to be set");
        }

        protected void Update()
        {
            if (isRendered) return;
            isRendered = true;

            imageGameObject!.texture = Image!;
        }
    }
}
