#nullable enable

using Assets.DTO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class AvatarBehaviour : MonoBehaviour
    {
        public TextMeshProUGUI? healthText = null;
        public GameObject? healthSectorPrefab = null;
        public GameObject? healthBarGameObject = null;
        public GameObject? runesGameObject = null;

        private const float HEALTH_START_ANGLE = 152.25f;
        private const float HEALTH_STEP_ANGLE = 10.5f;

        private List<Action> unsubscribers = new List<Action>();
        private bool changesArePresent = false;
        private int health = 0;
        private byte runes = 0;

        void Start()
        {
            var uss = Networking.Instance.Subscribe(Constants.MethodNames.MATCH_STATE_UPDATE, OnMatchStateUpdateAsync);
            unsubscribers.Add(uss);

            _ = destroyCancellationToken;
        }

        void Update()
        {
            if (!changesArePresent) return;

            if (this.healthText == null) throw new InvalidOperationException($"{nameof(healthText)} gameObject is expected to be set");
            if (this.healthSectorPrefab == null) throw new InvalidOperationException($"{nameof(healthSectorPrefab)} prefab is expected to be set");
            if (this.healthBarGameObject == null) throw new InvalidOperationException($"{nameof(healthBarGameObject)} gameObject is expected to be set");
            if (this.runesGameObject == null) throw new InvalidOperationException($"{nameof(runesGameObject)} gameObject is expected to be set");

            this.healthText.text = health.ToString();

            var currentHealthSectors = healthBarGameObject.transform.GetComponentsInChildren<RawImage>();
            foreach (var dc in currentHealthSectors)
            {
                Destroy(dc.gameObject);
                Destroy(dc);
            }

            for (int i = 0; i < health; i++)
            {
                var angle = HEALTH_START_ANGLE - i * HEALTH_STEP_ANGLE;
                var sector = Instantiate(healthSectorPrefab, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 0, angle)));
                sector.transform.parent = healthBarGameObject.transform;

                var sectorRect = sector.GetComponent<RectTransform>();
                sectorRect.anchorMin = new Vector2(0, 0);
                sectorRect.anchorMax = new Vector2(1, 1);
                sectorRect.offsetMin = new Vector2(0, 0);
                sectorRect.offsetMax = new Vector2(0, 0);
                sectorRect.localScale = new Vector3(1, 1, 1);
            }

            var runeObjects = runesGameObject.transform.GetComponentsInChildren<RawImage>();
            for (byte i = 0; i < runeObjects.Length; i++)
            {
                runeObjects[i].gameObject.SetActive(i < runes);
            }

            changesArePresent = false;
        }

        private async Task OnMatchStateUpdateAsync(string message, CancellationToken cancellationToken)
        {
            ServerMessageDTO<PlayerMatchStateDTO> dto = JsonUtility.FromJson<ServerMessageDTO<PlayerMatchStateDTO>>(message);
            Debug.Log($"AvatarBehaviour.OnMatchStateUpdateAsync: Health and runes: {dto.body.health}, {dto.body.runes}");

            this.health = dto.body.health;
            this.runes = dto.body.runes;

            changesArePresent = true;
        }
    }
}
