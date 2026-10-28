using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI
{
    public enum DispatchService { Militia, Ambulance, Liquidators, Ignore }

    /// Панель диспетчеризации: 4 кнопки. Нажатие сразу завершает звонок, подтверждения нет.
    public class DispatchPanelView : MonoBehaviour
    {
        public Button militia, ambulance, liquidators, ignore;
        public TMP_Text title;
        public TMP_Text status;
        public Sprite pressedSprite;
        public string lockedText = "ЗАБЛОКИРОВАНА — СНАЧАЛА СОБЕРИТЕ ДАННЫЕ";
        public string activeText = "нажатие сразу завершает звонок";
        public string sentText = "ВЫЗОВ ПЕРЕДАН";

        public event Action<DispatchService> Dispatched;

        bool _sent;

        void Awake()
        {
            militia.onClick.AddListener(() => Send(militia, DispatchService.Militia));
            ambulance.onClick.AddListener(() => Send(ambulance, DispatchService.Ambulance));
            liquidators.onClick.AddListener(() => Send(liquidators, DispatchService.Liquidators));
            ignore.onClick.AddListener(() => Send(ignore, DispatchService.Ignore));
        }

        Button[] All => new[] { militia, ambulance, liquidators, ignore };

        public void SetStatus(string text) => status.text = text;

        public void SetLocked(bool locked)
        {
            if (_sent) return;
            foreach (var b in All)
            {
                b.interactable = !locked;
                var pbt = b.GetComponent<PixelButtonText>();
                if (pbt != null) pbt.Apply();
            }
            status.text = locked ? lockedText : activeText;
            title.color = locked ? BakPalette.Secondary : BakPalette.Main;
        }

        /// Вернуть панель в исходное состояние перед новым звонком.
        public void ResetPanel()
        {
            _sent = false;
            foreach (var b in All)
            {
                b.transition = Selectable.Transition.SpriteSwap;
                var keeper = b.GetComponent<LabelKeeper>();
                if (keeper != null)
                {
                    var label = b.GetComponentInChildren<TMP_Text>(true);
                    if (label != null && !string.IsNullOrEmpty(keeper.original)) label.text = keeper.original;
                    if (keeper.sprite != null) b.image.sprite = keeper.sprite;
                    b.image.type = keeper.imageType;
                }
                var pbt = b.GetComponent<PixelButtonText>();
                if (pbt != null) { pbt.enabled = true; pbt.Apply(); }
            }
            SetLocked(true);
        }

        void Send(Button pressedButton, DispatchService service)
        {
            if (_sent) return;
            _sent = true;
            foreach (var b in All)
            {
                if (b == pressedButton) continue;
                b.interactable = false;
                var other = b.GetComponent<PixelButtonText>();
                if (other != null) other.Apply();
            }

            // Нажатая кнопка остаётся залитой акцентом с подписью «ВЫЗОВ ПЕРЕДАН».
            var keeper = pressedButton.GetComponent<LabelKeeper>();
            if (keeper == null) keeper = pressedButton.gameObject.AddComponent<LabelKeeper>();
            var text = pressedButton.GetComponentInChildren<TMP_Text>(true);
            if (string.IsNullOrEmpty(keeper.original) && text != null) keeper.original = text.text;
            if (keeper.sprite == null) { keeper.sprite = pressedButton.image.sprite; keeper.imageType = pressedButton.image.type; }

            pressedButton.transition = Selectable.Transition.None;
            pressedButton.image.sprite = pressedSprite;
            pressedButton.image.type = Image.Type.Sliced;
            if (text != null)
            {
                text.text = sentText;
                text.color = BakPalette.Bg;
                text.fontStyle |= FontStyles.Bold;
            }
            var pbt = pressedButton.GetComponent<PixelButtonText>();
            if (pbt != null) pbt.enabled = false;
            status.text = "звонок завершён";
            Dispatched?.Invoke(service);
        }
    }
}
