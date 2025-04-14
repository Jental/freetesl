#nullable enable

using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Assets.Behaviours
{
    public abstract class AListBehaviour<T, B> : MonoBehaviour where B : AListItemBehaviour<T>
    {
        [SerializeField] protected GameObject? ContentGameObject = null;
        [SerializeField] protected B? ListItemPrefab = null;

        private T[] modelsToShow = Array.Empty<T>();
        protected int? selectedItemIdx;

        protected List<B> itemGameObjects = new List<B>();
        protected bool modelsChangesArePresent = false;
        protected bool dynamicChangesArePresent = false;

        protected T[] ModelsToShow
        {
            get { return modelsToShow; }
            set
            {
                modelsChangesArePresent = !modelsToShow.SequenceEqual(value) || modelsToShow.Length != itemGameObjects.Count; // for case of unsync happened
                modelsToShow = value;
            }
        }

        protected void Start()
        {
            if (this.ContentGameObject == null) throw new InvalidOperationException($"{nameof(ContentGameObject)} game object is expected to be set");
            if (this.ListItemPrefab == null) throw new InvalidOperationException($"{nameof(ListItemPrefab)} prefab is expected to be set");

            _ = destroyCancellationToken;
        }

        protected void OnDisable()
        {
            Debug.Log($"{nameof(AListBehaviour<T, B>)}.OnDisable");
            selectedItemIdx = null;
        }

        protected void Update()
        {
            if (modelsChangesArePresent)
            {
                modelsChangesArePresent = false;

                var children = ContentGameObject!.transform.GetComponentsInChildren<B>();
                foreach (var dc in children)
                {
                    Destroy(dc.gameObject);
                    Destroy(dc);
                }
                itemGameObjects.Clear();

                for (int i = 0; i < modelsToShow.Length; i++)
                {
                    var player = modelsToShow[i];
                    var dc =
                        Instantiate(ListItemPrefab, new Vector3(0, 0, 0), Quaternion.identity)
                        ?? throw new InvalidOperationException("Failed to instantiate a player list item prefab");
                    dc.transform.parent = ContentGameObject.transform;
                    dc.Model = player;
                    var savedIdx = i;
                    dc.OnClick = () => OnItemClick(savedIdx);
                    dc.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                    itemGameObjects.Add(dc);
                }
            }

            if (dynamicChangesArePresent)
            {
                for (int i = 0; i < itemGameObjects.Count; i++)
                {
                    var igo = itemGameObjects[i];
                    igo.IsSelected = i == selectedItemIdx;
                }
            }
        }

        protected abstract Task RefreshImplAsync(CancellationToken cancellationToken);

        public void Refresh()
        {
            _ = Task.Run(async () => {
                await RefreshImplAsync(destroyCancellationToken);
                modelsChangesArePresent = true;
            });
        }

        public T? SelectedModel =>
            selectedItemIdx == null || selectedItemIdx >= modelsToShow.Length
            ? default
            : modelsToShow[selectedItemIdx.Value];

        public void DeselectPlayer()
        {
            selectedItemIdx = null;
        }

        protected void OnItemClick(int idx)
        {
            Debug.Log($"{nameof(AListBehaviour<T, B>)}: OnItemClick: {idx}");
            selectedItemIdx =
                idx == selectedItemIdx
                ? null
                : idx;
            dynamicChangesArePresent = true;
        }
    }
}
