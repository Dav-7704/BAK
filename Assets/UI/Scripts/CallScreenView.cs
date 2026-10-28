using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI
{
    public enum CallPhase
    {
        Boss,       // инструктаж / финалы: портрет босса, шкалы нет, панель заблокирована
        Gathering,  // сбор данных: текст листается кликом, шкала серая, панель заблокирована
        Analysis,   // анализ: варианты ответа, шкала активна, панель открывается после обязательных узлов
        HungUp      // терпение 0%: плашка «АБОНЕНТ ПОЛОЖИЛ ТРУБКУ»
    }

    /// Основной экран звонка. Собирает виджеты и переключает их по фазам звонка.
    /// Логика диалога (данные звонка) живёт отдельно и только вызывает методы этого класса.
    public class CallScreenView : MonoBehaviour
    {
        [Header("Строка состояния")]
        public TMP_Text contextLabel;
        public TMP_Text dateLabel;
        public ReprimandCounter reprimands;
        public HelpButton reprimandHelp;

        [Header("Портреты")]
        public Image dispatcherPortrait;
        public Image callerPortrait;
        public GameObject dispatcherPlaceholder;
        public GameObject callerPlaceholder;
        public Image dispatcherFrame;
        public Image callerFrame;
        public TMP_Text dispatcherRole, dispatcherName;
        public TMP_Text callerRole, callerName;
        public Sprite frameIdle, frameSpeaking;

        [Header("Памятка")]
        public MemoTileView memoTile;
        public MemoWindowView memoWindow;

        [Header("Журнал и терпение")]
        public TMP_Text logTitle;
        public GameObject patienceGroup;
        public TMP_Text patienceHiddenLabel;
        public PatienceBar patience;
        public HelpButton patienceHelp;
        public DialogueLogView log;

        [Header("Ответы и панель")]
        public ResponseOptionsView options;
        public DispatchPanelView dispatch;
        public HelpPopupGroup helpGroup;

        [Header("Тексты")]
        public string bossLogTitle = "ВНУТРЕННЯЯ ЛИНИЯ · НАЧАЛЬНИК СМЕНЫ";
        public string callLogTitle = "ЖУРНАЛ ВЫЗОВА";
        public string noOptionsMessage = "В фазе сбора данных и в речи босса вариантов нет: текст листается кликом по журналу.";

        [SerializeField] CallPhase phase = CallPhase.Gathering;
        public CallPhase Phase => phase;

        void Awake()
        {
            if (memoTile != null && memoWindow != null) memoTile.Clicked += memoWindow.Open;
        }

        void Start()
        {
            if (memoWindow != null) memoWindow.gameObject.SetActive(false);
            SetPhase(phase);
        }

        public void SetStatus(string context, string date)
        {
            contextLabel.text = context;
            dateLabel.text = date;
        }

        public void SetCaller(string role, string nameOrDistrict, Sprite portrait = null)
        {
            callerRole.text = role;
            callerName.text = nameOrDistrict;
            SetPortrait(callerPortrait, callerPlaceholder, portrait);
        }

        public void SetDispatcher(string role, string name, Sprite portrait = null)
        {
            dispatcherRole.text = role;
            dispatcherName.text = name;
            SetPortrait(dispatcherPortrait, dispatcherPlaceholder, portrait);
        }

        static void SetPortrait(Image img, GameObject placeholder, Sprite sprite)
        {
            img.sprite = sprite;
            img.color = sprite != null ? Color.white : BakPalette.Panel;
            if (placeholder != null) placeholder.SetActive(sprite == null);
        }

        /// Подсвечивает рамку портрета того, кто сейчас говорит.
        public void SetSpeaker(bool callerSpeaking)
        {
            callerFrame.sprite = callerSpeaking ? frameSpeaking : frameIdle;
            dispatcherFrame.sprite = callerSpeaking ? frameIdle : frameSpeaking;
        }

        public void SetPhase(CallPhase newPhase)
        {
            phase = newPhase;
            bool boss = phase == CallPhase.Boss;

            logTitle.text = boss ? bossLogTitle : callLogTitle;
            patienceGroup.SetActive(!boss);
            patienceHiddenLabel.gameObject.SetActive(boss);
            patienceHelp.SetInteractable(!boss);
            reprimandHelp.SetInteractable(!boss);

            switch (phase)
            {
                case CallPhase.Boss:
                case CallPhase.Gathering:
                    if (!boss) patience.SetFrozen(true);
                    log.SetWaitMarker(true);
                    log.SetHangup(false);
                    options.ShowEmpty(noOptionsMessage);
                    dispatch.SetLocked(true);
                    break;
                case CallPhase.Analysis:
                    if (patience.IsFrozen) patience.SetValue(patience.Value);
                    log.SetWaitMarker(false);
                    log.SetHangup(false);
                    break;
                case CallPhase.HungUp:
                    log.SetWaitMarker(false);
                    log.SetHangup(true);
                    patience.SetValue(0);
                    options.ShowEmpty("Связь прервана.");
                    dispatch.SetLocked(true);
                    dispatch.SetStatus("звонок прерван абонентом");
                    break;
            }
        }

        [ContextMenu("Фаза: Босс")] void PreviewBoss() => SetPhase(CallPhase.Boss);
        [ContextMenu("Фаза: Сбор данных")] void PreviewGathering() => SetPhase(CallPhase.Gathering);
        [ContextMenu("Фаза: Анализ")] void PreviewAnalysis() => SetPhase(CallPhase.Analysis);
        [ContextMenu("Фаза: Трубка брошена")] void PreviewHungUp() => SetPhase(CallPhase.HungUp);
    }
}
