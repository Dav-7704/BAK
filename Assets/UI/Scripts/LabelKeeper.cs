using UnityEngine;

namespace Bak.UI
{
    /// Хранит исходную подпись кнопки, чтобы вернуть её после «ВЫЗОВ ПЕРЕДАН».
    public class LabelKeeper : MonoBehaviour
    {
        public string original;
        public Sprite sprite;
        public UnityEngine.UI.Image.Type imageType = UnityEngine.UI.Image.Type.Tiled;
    }
}
