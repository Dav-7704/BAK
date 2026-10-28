using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI
{
    /// Личное дело: звонков обработано, выговоры, последний вызов.
    /// Правая кнопка: «СЛЕДУЮЩИЙ ЗВОНОК ▸», а при увольнении или после 5-го звонка — «ДАЛЕЕ ▸».
    public class StatsView : MonoBehaviour
    {
        public TMP_Text statusRight;
        public TMP_Text subtitle;
        public TMP_Text callsValue;
        public TMP_Text reprimandsValue;
        public ReprimandCounter reprimandSquares;
        public TMP_Text lastCall;
        public Button menuButton;
        public Button nextButton;
        public TMP_Text nextLabel;

        public string nextCallText = "СЛЕДУЮЩИЙ ЗВОНОК ▸";
        public string finalText = "ДАЛЕЕ ▸";

        public event Action MainMenu;
        public event Action Next;

        void Awake()
        {
            menuButton.onClick.AddListener(() => MainMenu?.Invoke());
            nextButton.onClick.AddListener(() => Next?.Invoke());
        }

        public void Show(string date, string dispatcherLine, int callsDone, int totalCalls, int reprimands, int maxReprimands,
            string lastCallPhrase, bool goesToFinal)
        {
            statusRight.text = date;
            subtitle.text = dispatcherLine;
            callsValue.text = callsDone + " / " + totalCalls;
            reprimandsValue.text = reprimands + " / " + maxReprimands;
            reprimandSquares.SetCount(reprimands);
            lastCall.text = lastCallPhrase;
            nextLabel.text = goesToFinal ? finalText : nextCallText;
            gameObject.SetActive(true);
        }
    }
}
