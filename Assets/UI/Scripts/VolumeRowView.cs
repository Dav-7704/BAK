using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI
{
    /// Строка громкости: − [10 сегментов] + и процент справа. Шаг 10%.
    public class VolumeRowView : MonoBehaviour
    {
        public TMP_Text label;
        public Button minusButton;
        public Button plusButton;
        public Image[] segments;
        public TMP_Text percent;
        public Sprite segFull, segEmpty;
        public int step = 10;

        [SerializeField, Range(0, 100)] int value = 70;

        public int Value => value;
        public event Action<int> Changed;

        void Awake()
        {
            minusButton.onClick.AddListener(() => SetValue(value - step, true));
            plusButton.onClick.AddListener(() => SetValue(value + step, true));
        }

        void OnEnable() => Refresh();

        public void SetValue(int newValue, bool notify)
        {
            int v = Mathf.Clamp(newValue, 0, 100);
            bool changed = v != value;
            value = v;
            Refresh();
            if (notify && changed) Changed?.Invoke(value);
        }

        void Refresh()
        {
            int filled = Mathf.RoundToInt(value / 10f);
            if (segments != null)
                for (int i = 0; i < segments.Length; i++)
                    if (segments[i] != null) segments[i].sprite = i < filled ? segFull : segEmpty;
            if (percent != null) percent.text = value + "%";
            if (minusButton != null) minusButton.interactable = value > 0;
            if (plusButton != null) plusButton.interactable = value < 100;
        }
    }
}
