using UnityEngine;

namespace Bak.UI
{
    /// Следит, чтобы была открыта только одна подсказка «?».
    public class HelpPopupGroup : MonoBehaviour
    {
        public HelpButton[] buttons;

        public void CloseAllExcept(HelpButton keep)
        {
            if (buttons == null) return;
            foreach (var b in buttons)
                if (b != null && b != keep && b.IsOpen) b.SetOpen(false);
        }

        public void CloseAll() => CloseAllExcept(null);
    }
}
