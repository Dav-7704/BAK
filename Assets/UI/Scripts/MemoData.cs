using System;
using UnityEngine;

namespace Bak.UI
{
    /// Содержимое «Памятки сотрудника». Страницы хранятся данными, а не картинками:
    /// заголовок, подзаголовок, 1–2 блока с пунктами и решение внизу.
    [CreateAssetMenu(menuName = "BAK/Memo Data", fileName = "MemoData")]
    public class MemoData : ScriptableObject
    {
        [Serializable]
        public class Block
        {
            public string heading;
            [TextArea(1, 3)] public string[] items;
        }

        [Serializable]
        public class Page
        {
            [Tooltip("Подпись вкладки")] public string tab;
            public string title;
            public string subtitle;
            public Block[] blocks;
            [Tooltip("Рамка с решением внизу страницы")] public string decision;
        }

        public Page[] pages;
    }
}
