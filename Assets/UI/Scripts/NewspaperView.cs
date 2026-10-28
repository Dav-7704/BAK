using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI
{
    /// Данные одного выпуска газеты (исход звонка).
    [Serializable]
    public class NewsIssue
    {
        public string issue = "№ 41 (2847)";
        public string date = "16 ОКТЯБРЯ 2002";
        [TextArea(1, 2)] public string headline;
        public string subheadline;
        [TextArea(3, 8)] public string body;
        public Sprite illustration;
        public string caption;
    }

    /// Пост-история: газета «Тихореченский вестник». Шаблон один, меняются только данные.
    /// Дальше — только кнопкой «ДАЛЕЕ ▸», не кликом по экрану.
    public class NewspaperView : MonoBehaviour
    {
        public TMP_Text statusRight;
        public TMP_Text issue;
        public TMP_Text date;
        public TMP_Text headline;
        public TMP_Text subheadline;
        public TMP_Text body;
        public Image illustration;
        public GameObject illustrationPlaceholder;
        public TMP_Text caption;
        public Button nextButton;

        public event Action Next;

        void Awake() => nextButton.onClick.AddListener(() => Next?.Invoke());

        public void Show(NewsIssue n, int callNumber, int totalCalls)
        {
            statusRight.text = "ИТОГ ВЫЗОВА · ЗВОНОК " + callNumber + " / " + totalCalls;
            issue.text = n.issue;
            date.text = n.date;
            headline.text = n.headline;
            subheadline.text = n.subheadline;
            body.text = n.body;
            caption.text = n.caption;
            illustration.sprite = n.illustration;
            illustration.color = n.illustration != null ? Color.white : BakPalette.Panel;
            illustrationPlaceholder.SetActive(n.illustration == null);
            gameObject.SetActive(true);
        }
    }
}
