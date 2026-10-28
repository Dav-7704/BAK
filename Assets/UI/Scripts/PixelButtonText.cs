using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bak.UI
{
    /// Меняет цвет текста/иконок кнопки по состояниям (обычная, наведение, нажата, неактивна, активна).
    /// Стандартный Button перекрашивает только фон, поэтому текст инвертируем этим компонентом.
    /// Состояние «активна» (выбранная вкладка, открытая подсказка) фиксирует отдельный спрайт.
    [RequireComponent(typeof(Selectable))]
    public class PixelButtonText : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Graphic[] targets;
        public Color normal = BakPalette.Main;
        public Color hover = BakPalette.Bg;
        public Color pressed = BakPalette.Bg;
        public Color disabled = BakPalette.Frame;
        public Color active = BakPalette.Bg;

        [Tooltip("Тёмный текст на светлой заливке делать полужирным")]
        public bool boldWhenInverted = true;

        [Tooltip("Спрайт для состояния «активна». Пусто — состояние не используется.")]
        public Sprite activeSprite;

        [SerializeField] bool isActive;

        Selectable _sel;
        Sprite _normalSprite;
        bool _over, _down, _lastInteractable;

        public bool IsActive => isActive;

        void Awake() => Init();

        void Init()
        {
            if (_sel != null) return;
            _sel = GetComponent<Selectable>();
            if (_sel.image != null) _normalSprite = _sel.image.sprite;
        }

        void OnEnable()
        {
            _over = _down = false;
            Apply();
        }

        void Update()
        {
            if (_sel != null && _sel.interactable != _lastInteractable) Apply();
        }

        public void SetActive(bool value)
        {
            isActive = value;
            Apply();
        }

        public void OnPointerEnter(PointerEventData e) { _over = true; Apply(); }
        public void OnPointerExit(PointerEventData e) { _over = false; _down = false; Apply(); }
        public void OnPointerDown(PointerEventData e) { _down = true; Apply(); }
        public void OnPointerUp(PointerEventData e) { _down = false; Apply(); }

        public void Apply()
        {
            Init();
            _lastInteractable = _sel.interactable;

            if (activeSprite != null && _sel.image != null)
            {
                bool showActive = isActive && _sel.interactable;
                _sel.transition = showActive ? Selectable.Transition.None : Selectable.Transition.SpriteSwap;
                _sel.image.sprite = showActive ? activeSprite : _normalSprite;
            }

            Color c;
            if (!_sel.interactable) c = disabled;
            else if (isActive) c = active;
            else if (_down) c = pressed;
            else if (_over) c = hover;
            else c = normal;

            if (targets == null) return;
            // тёмный текст на светлой заливке делаем полужирным, иначе тонкий шрифт «тает» и выглядит серым
            bool inverted = boldWhenInverted && c == BakPalette.Bg;
            foreach (var g in targets)
            {
                if (g == null) continue;
                g.color = c;
                if (g is TMPro.TMP_Text t)
                    t.fontStyle = inverted ? (t.fontStyle | TMPro.FontStyles.Bold) : (t.fontStyle & ~TMPro.FontStyles.Bold);
            }
        }
    }
}
