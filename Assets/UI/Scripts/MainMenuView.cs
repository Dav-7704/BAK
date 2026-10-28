using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI
{
    /// Главное меню: НОВАЯ ИГРА, ПРОДОЛЖИТЬ (без сохранения — неактивна, пунктир), НАСТРОЙКИ.
    public class MainMenuView : MonoBehaviour
    {
        public Button newGameButton;
        public Button continueButton;
        public Button settingsButton;
        public Image backgroundLeft;
        public Image backgroundRight;

        public event Action NewGame;
        public event Action Continue;
        public event Action Settings;

        void Awake()
        {
            newGameButton.onClick.AddListener(() => NewGame?.Invoke());
            continueButton.onClick.AddListener(() => Continue?.Invoke());
            settingsButton.onClick.AddListener(() => Settings?.Invoke());
        }

        public void SetCanContinue(bool canContinue)
        {
            continueButton.interactable = canContinue;
            var pbt = continueButton.GetComponent<PixelButtonText>();
            if (pbt != null) pbt.Apply();
        }
    }
}
