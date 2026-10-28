using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI
{
    /// Кнопка «?»: клик открывает карточку-подсказку, повторный клик закрывает.
    /// Одновременно открыта только одна карточка (через HelpPopupGroup).
    public class HelpButton : MonoBehaviour
    {
        public Button button;
        public GameObject card;
        public Sprite activeSprite;
        public HelpPopupGroup group;

        Sprite _normalSprite;
        bool _open;

        public bool IsOpen => _open;

        void Awake()
        {
            Init();
            button.onClick.AddListener(Toggle);
            if (card != null) card.SetActive(false);
        }

        void Init()
        {
            if (button == null) button = GetComponent<Button>();
            if (_normalSprite == null && !_open && button != null) _normalSprite = button.image.sprite;
        }

        public void Toggle() => SetOpen(!_open);

        public void SetOpen(bool open)
        {
            Init();
            if (open && group != null) group.CloseAllExcept(this);
            _open = open;
            if (card != null) card.SetActive(open);
            if (button == null) return;
            button.transition = open ? Selectable.Transition.None : Selectable.Transition.SpriteSwap;
            button.image.sprite = open ? activeSprite : _normalSprite;
        }

        public void SetInteractable(bool interactable)
        {
            if (!interactable) SetOpen(false);
            if (button != null) button.interactable = interactable;
        }
    }
}
