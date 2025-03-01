#nullable enable

using Assets.Common;
using Assets.Services;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Behaviours
{
    public class ExitConfirmationDialogBehaviour : MonoBehaviour
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
            _ = Task.Run(async () =>
            {
                try
                {
                    await Networking.Instance.PostAsync(Constants.MethodNames.LOGOUT, destroyCancellationToken);
                }
                catch { }

                Application.Quit();
            });
        }
    }
}
