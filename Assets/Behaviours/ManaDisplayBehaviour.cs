#nullable enable

using Assets.DTO;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;

namespace Assets.Behaviours
{
    public class ManaDisplayBehaviour : AWithMatchStateSubscribtionBehaviour
    {
        public TextMeshProUGUI? manaTextGameObject;

        private int mana = 0;
        private int maxMana = 0;

        protected new void OnDisable()
        {
            base.OnDisable();
            mana = 0;
            maxMana = 0;
            changesArePresent = true;
        }

        protected override Task OnMatchStateUpdateAsync(PlayerMatchStateDTO dto, bool isPlayersTurn, CancellationToken cancellationToken)
        {
            this.maxMana = dto.maxMana;
            this.mana = dto.mana;

            return Task.CompletedTask;
        }

        protected override void UpdateImpl()
        {
            this.manaTextGameObject!.text = $"{mana}/{maxMana}";
        }

        protected override void VerifyFields()
        {
            if (this.manaTextGameObject == null) throw new InvalidOperationException($"{nameof(manaTextGameObject)} gameObject is expected to be set");
        }
    }
}
