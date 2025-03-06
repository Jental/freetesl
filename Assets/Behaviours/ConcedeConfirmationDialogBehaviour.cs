#nullable enable

using System;
using UnityEngine;

namespace Assets.Behaviours
{
    public class ConcedeConfirmationDialogBehaviour : MonoBehaviour
    {
        protected void Start()
        {
            _ = destroyCancellationToken;

            var confirmationDialog = gameObject.GetComponent<ConfirmationDialogBehaviour>();
            if (confirmationDialog == null ) throw new InvalidOperationException($"{nameof(ConfirmationDialogBehaviour)} component is expected to be added");
            confirmationDialog.OnOK = OnOk;
        }

        private void OnOk()
        {
            Debug.Log("ConcedeConfirmationDialogBehaviour.OnOk");
        }
    }
}
