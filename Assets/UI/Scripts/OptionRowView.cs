using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI
{
    /// Строка варианта ответа: номер + текст, вся строка — кнопка.
    public class OptionRowView : MonoBehaviour
    {
        public Button button;
        public TMP_Text number;
        public TMP_Text label;

        Action _onClick;

        void Awake()
        {
            button.onClick.AddListener(() => _onClick?.Invoke());
        }

        public void Set(int index, string text, Action onClick)
        {
            number.text = (index + 1).ToString();
            label.text = text;
            _onClick = onClick;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _onClick = null;
            gameObject.SetActive(false);
        }
    }
}
