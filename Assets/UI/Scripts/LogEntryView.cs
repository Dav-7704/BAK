using TMPro;
using UnityEngine;

namespace Bak.UI
{
    /// Одна строка журнала: имя говорящего + реплика.
    public class LogEntryView : MonoBehaviour
    {
        public TMP_Text speaker;
        public TMP_Text body;

        public void Set(string speakerName, string text, bool isDispatcher)
        {
            speaker.text = speakerName;
            speaker.color = BakPalette.Secondary;
            body.text = text;
            body.color = isDispatcher ? BakPalette.Secondary : BakPalette.Main;
        }
    }
}
