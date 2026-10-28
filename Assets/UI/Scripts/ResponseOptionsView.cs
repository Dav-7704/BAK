using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI
{
    /// Блок «ВАРИАНТЫ ОТВЕТА»: до 3 строк. Без вариантов — пунктирная рамка и пояснение.
    public class ResponseOptionsView : MonoBehaviour
    {
        public Image frame;
        public Sprite activeFrame, lockedFrame;
        public TMP_Text title;
        public TMP_Text hint;
        public TMP_Text emptyMessage;
        public OptionRowView[] rows;

        public string hintActive = "ЛКМ — выбрать";
        public string hintEmpty = "нет";

        public event Action<int> Picked;

        public void ShowOptions(IList<string> options)
        {
            frame.sprite = activeFrame;
            title.color = BakPalette.Main;
            hint.text = hintActive;
            emptyMessage.gameObject.SetActive(false);
            for (int i = 0; i < rows.Length; i++)
            {
                if (i < options.Count)
                {
                    int index = i;
                    rows[i].Set(i, options[i], () => Picked?.Invoke(index));
                }
                else rows[i].Hide();
            }
            SetInteractable(true);
        }

        public void ShowEmpty(string message)
        {
            frame.sprite = lockedFrame;
            title.color = BakPalette.Secondary;
            hint.text = hintEmpty;
            emptyMessage.text = message;
            emptyMessage.gameObject.SetActive(true);
            foreach (var r in rows) r.Hide();
        }

        public void SetInteractable(bool interactable)
        {
            foreach (var r in rows) r.button.interactable = interactable;
        }
    }
}
