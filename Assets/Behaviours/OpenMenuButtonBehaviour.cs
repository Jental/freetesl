#nullable enable

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Behaviours
{
    public class OpenMenuButtonBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject? menuDialog = null;

        protected void Start()
        {
            if (menuDialog == null) throw new InvalidOperationException($"{nameof(menuDialog)} game object is expected to be set");

            gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            Debug.Log("OpenMenuButtonBehaviour.OnClick");
            menuDialog!.SetActive(true);
        }
    }
}
