using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI
{
    /// Счётчик выговоров: пустой квадрат — рамка, полученный — заливка. Меняется только между звонками.
    public class ReprimandCounter : MonoBehaviour
    {
        public Image[] squares;
        public Sprite empty, full;
        [SerializeField, Range(0, 3)] int count;

        public int Count => count;

        void OnEnable() => Refresh();

        public void SetCount(int value)
        {
            count = Mathf.Clamp(value, 0, squares != null ? squares.Length : 3);
            Refresh();
        }

        void Refresh()
        {
            if (squares == null) return;
            for (int i = 0; i < squares.Length; i++)
                if (squares[i] != null) squares[i].sprite = i < count ? full : empty;
        }
    }
}
