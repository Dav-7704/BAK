using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI
{
    /// Шкала терпения: 10 сегментов по 10%. Состояния: заморожена (сбор данных), обычная, критическая (≤20%).
    public class PatienceBar : MonoBehaviour
    {
        public Image[] segments;
        public TMP_Text percentLabel;
        public Sprite full, empty, frozen, critical;
        [Range(0, 100)] public int criticalThreshold = 20;
        public bool blinkWhenCritical = true;
        public float blinkPeriod = 0.5f;

        [SerializeField, Range(0, 100)] int value = 100;
        [SerializeField] bool isFrozen = true;

        public int Value => value;
        public bool IsFrozen => isFrozen;
        public bool IsCritical => !isFrozen && value > 0 && value <= criticalThreshold;

        float _blinkTimer;
        bool _blinkOn = true;

        void OnEnable() => Refresh();

        public void SetFrozen(bool frozenState)
        {
            isFrozen = frozenState;
            Refresh();
        }

        public void SetValue(int percent)
        {
            value = Mathf.Clamp(percent, 0, 100);
            isFrozen = false;
            Refresh();
        }

        public void Add(int delta) => SetValue(value + delta);

        void Update()
        {
            if (!blinkWhenCritical || !IsCritical) return;
            _blinkTimer += Time.unscaledDeltaTime;
            if (_blinkTimer < blinkPeriod) return;
            _blinkTimer = 0f;
            _blinkOn = !_blinkOn;
            ApplySegments();
        }

        public void Refresh()
        {
            _blinkOn = true;
            _blinkTimer = 0f;
            ApplySegments();
            if (percentLabel == null) return;
            percentLabel.text = isFrozen ? "— —" : value + "%";
            percentLabel.color = IsCritical ? BakPalette.Accent : (isFrozen ? BakPalette.Frame : BakPalette.Main);
        }

        void ApplySegments()
        {
            if (segments == null) return;
            int filled = Mathf.RoundToInt(value / 10f);
            for (int i = 0; i < segments.Length; i++)
            {
                var s = segments[i];
                if (s == null) continue;
                if (isFrozen) s.sprite = frozen;
                else if (i < filled) s.sprite = IsCritical ? (_blinkOn ? critical : empty) : full;
                else s.sprite = empty;
            }
        }
    }
}
