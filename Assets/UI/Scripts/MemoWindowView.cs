using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI
{
    /// Окно «Памятка сотрудника» поверх левой зоны. Игра под ним не останавливается.
    /// Вкладки (5 в ряд), стрелки ◂ ▸ по кругу, «стр. N / 5», крестик. Помнит последнюю страницу.
    public class MemoWindowView : MonoBehaviour
    {
        public MemoData data;
        public Button closeButton;
        public Button prevButton, nextButton;
        public TMP_Text pageLabel;
        public Button[] tabs;
        public TMP_Text pageTitle;
        public TMP_Text pageSubtitle;
        public TMP_Text[] blockHeadings;
        public TMP_Text[] blockItems;
        public TMP_Text decision;
        public GameObject decisionFrame;

        public event Action Closed;

        int _page;

        public int Page => _page;

        void Awake()
        {
            closeButton.onClick.AddListener(Close);
            prevButton.onClick.AddListener(() => ShowPage(_page - 1));
            nextButton.onClick.AddListener(() => ShowPage(_page + 1));
            for (int i = 0; i < tabs.Length; i++)
            {
                int index = i;
                tabs[i].onClick.AddListener(() => ShowPage(index));
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            ShowPage(_page);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            Closed?.Invoke();
        }

        public void ShowPage(int index)
        {
            if (data == null || data.pages == null || data.pages.Length == 0) return;
            int count = data.pages.Length;
            _page = ((index % count) + count) % count; // листание по кругу
            var p = data.pages[_page];

            for (int i = 0; i < tabs.Length; i++)
            {
                bool exists = i < count;
                tabs[i].gameObject.SetActive(exists);
                if (!exists) continue;
                var label = tabs[i].GetComponentInChildren<TMP_Text>(true);
                if (label != null) label.text = data.pages[i].tab;
                var pbt = tabs[i].GetComponent<PixelButtonText>();
                if (pbt != null) pbt.SetActive(i == _page);
            }

            pageTitle.text = p.title;
            pageSubtitle.text = p.subtitle;
            pageSubtitle.gameObject.SetActive(!string.IsNullOrEmpty(p.subtitle));

            for (int i = 0; i < blockHeadings.Length; i++)
            {
                bool has = p.blocks != null && i < p.blocks.Length;
                blockHeadings[i].transform.parent.gameObject.SetActive(has);
                if (!has) continue;
                blockHeadings[i].text = p.blocks[i].heading;
                blockItems[i].text = FormatItems(p.blocks[i].items);
            }

            bool hasDecision = !string.IsNullOrEmpty(p.decision);
            decisionFrame.SetActive(hasDecision);
            decision.text = p.decision;
            pageLabel.text = "стр. " + (_page + 1) + " / " + count;

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)pageTitle.transform.parent);
        }

        static string FormatItems(string[] items)
        {
            if (items == null) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < items.Length; i++)
            {
                if (i > 0) sb.Append('\n');
                // висячий отступ: перенесённые строки выравниваются по тексту, а не по тире
                sb.Append("—<indent=8>").Append(items[i]).Append("</indent>");
            }
            return sb.ToString();
        }
    }
}
