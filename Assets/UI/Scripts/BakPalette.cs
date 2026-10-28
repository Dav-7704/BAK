using UnityEngine;

namespace Bak.UI
{
    /// Палитра БАК из спецификации интерфейса.
    public static class BakPalette
    {
        public static readonly Color Bg        = Hex("#071209"); // фон экрана, текст на светлых кнопках
        public static readonly Color Panel     = Hex("#0E2414"); // заливка панелей и кнопок
        public static readonly Color Line      = Hex("#2C5A35"); // разделители, неактивные рамки
        public static readonly Color Main      = Hex("#9BE7A3"); // основной текст, рамки, наведение
        public static readonly Color Accent    = Hex("#D8FFDC"); // нажатие, критическое терпение
        public static readonly Color Secondary = Hex("#6FBF7A"); // подписи, реплики диспетчера
        public static readonly Color Frame     = Hex("#4E9A5A"); // рамки панелей
        public static readonly Color SegEmpty  = Hex("#1F3B26"); // пустой сегмент
        public static readonly Color Inactive  = Hex("#0A1A0E"); // фон неактивного
        public static readonly Color HoverBg   = Hex("#122E19"); // фон наведения
        public static readonly Color Dim       = new Color(0f, 0f, 0f, 0.78f); // затемнение под окнами

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
