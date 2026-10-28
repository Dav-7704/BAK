using UnityEngine;

namespace Bak.UI
{
    /// Сквозной путь игрока для проверки интерфейса:
    /// меню → пролог с боссом → звонок → газета → личное дело → следующий звонок / финал → меню.
    /// Сохраняет номер следующего звонка и число выговоров. Тексты исходов — демонстрационные.
    public class GameFlowDemo : MonoBehaviour
    {
        public ScreenRouter router;
        public MainMenuView menu;
        public SettingsView settings;
        public CallScreenDemo callDemo;
        public NewspaperView news;
        public StatsView stats;
        public int totalCalls = 5;
        public int maxReprimands = 3;
        public string dispatcherLine = "ДИСПЕТЧЕР Д. КОВАЛЁВ · СМЕНА 1";
        public string shiftDate = "15.10.2002";

        const string NextCallKey = "bak.nextCall";
        const string ReprimandsKey = "bak.reprimands";

        int _call = 1;
        int _reprimands;
        string _lastPhrase;

        void Start()
        {
            menu.NewGame += NewGame;
            menu.Continue += ContinueGame;
            menu.Settings += () => settings.Open();
            callDemo.CallFinished += OnCallFinished;
            news.Next += ShowStats;
            stats.MainMenu += GoToMenu;
            stats.Next += AfterStats;
            GoToMenu();
        }

        void GoToMenu()
        {
            menu.SetCanContinue(PlayerPrefs.HasKey(NextCallKey));
            router.Show(ScreenId.MainMenu);
        }

        void NewGame()
        {
            PlayerPrefs.DeleteKey(NextCallKey);
            PlayerPrefs.DeleteKey(ReprimandsKey);
            _call = 1;
            _reprimands = 0;
            router.Show(ScreenId.Call);
            callDemo.StartBriefing(StartCall);
            callDemo.view.reprimands.SetCount(0);
        }

        void ContinueGame()
        {
            _call = PlayerPrefs.GetInt(NextCallKey, 1);
            _reprimands = PlayerPrefs.GetInt(ReprimandsKey, 0);
            StartCall();
        }

        void StartCall()
        {
            router.Show(ScreenId.Call);
            callDemo.StartCall(_call, totalCalls, shiftDate);
            callDemo.view.reprimands.SetCount(_reprimands);
        }

        void OnCallFinished(string outcome)
        {
            var result = Outcome(outcome);
            _reprimands = Mathf.Min(maxReprimands, _reprimands + result.penalty);
            _lastPhrase = result.phrase;
            // сохранение после каждого звонка: номер следующего звонка и выговоры
            PlayerPrefs.SetInt(NextCallKey, _call + 1);
            PlayerPrefs.SetInt(ReprimandsKey, _reprimands);
            PlayerPrefs.Save();
            router.Show(ScreenId.PostStory);
            news.Show(result.issue, _call, totalCalls);
        }

        void ShowStats()
        {
            router.Show(ScreenId.Stats);
            stats.Show(shiftDate, dispatcherLine, _call, totalCalls, _reprimands, maxReprimands, _lastPhrase, IsFinal);
        }

        bool IsFinal => _reprimands >= maxReprimands || _call >= totalCalls;

        void AfterStats()
        {
            if (_reprimands >= maxReprimands) Final(true);
            else if (_call >= totalCalls) Final(false);
            else
            {
                _call++;
                StartCall();
            }
        }

        void Final(bool fired)
        {
            PlayerPrefs.DeleteKey(NextCallKey);
            PlayerPrefs.DeleteKey(ReprimandsKey);
            router.Show(ScreenId.Call);
            callDemo.StartFinal(fired, () => router.FadeThrough(GoToMenu));
            callDemo.view.reprimands.SetCount(_reprimands);
        }

        struct Result
        {
            public int penalty;
            public string phrase;
            public NewsIssue issue;
        }

        static Result Outcome(string outcome)
        {
            var n = new NewsIssue { issue = "№ 41 (2847)", date = "16 ОКТЯБРЯ 2002", caption = "Дом № 14 по Заводской. Фото редакции." };
            var r = new Result { issue = n };
            switch (outcome)
            {
                case "Liquidators":
                    n.headline = "В ДОМЕ НА ЗАВОДСКОЙ\nПРОВЕЛИ САНОБРАБОТКУ";
                    n.subheadline = "ЖИЛЬЦОВ ОДНОГО ПОДЪЕЗДА ВРЕМЕННО ОТСЕЛИЛИ";
                    n.body = "Ночью специалисты Бюро аномального контроля обработали дом № 14 по Заводской улице от опасной плесени, обнаруженной в стояках. По словам представителя Бюро, угрозы для соседних домов нет.\n\nЖильцов квартиры № 37 увезли в больницу с отравлением. Сосед, вызвавший службу, остался недоволен шумом во время работ.";
                    n.caption = "Дом № 14 после обработки. Фото редакции.";
                    r.penalty = 0;
                    r.phrase = "Без замечаний. Угроза устранена.";
                    break;
                case "Militia":
                    n.headline = "НАРЯД МИЛИЦИИ\nНЕ ВЕРНУЛСЯ С ВЫЗОВА";
                    n.subheadline = "ДОМ НА ЗАВОДСКОЙ ОЦЕПЛЕН";
                    n.body = "Двое сотрудников милиции, прибывших по жалобе на шум в доме № 14 по Заводской улице, перестали выходить на связь через несколько минут после входа в подъезд.\n\nК утру дом оцепили. Представители Бюро аномального контроля от комментариев отказались.";
                    r.penalty = 3;
                    r.phrase = "+3: милиция отправлена на аномалию.";
                    break;
                case "Ambulance":
                    n.headline = "БРИГАДА СКОРОЙ\nПОСТРАДАЛА НА ВЫЗОВЕ";
                    n.subheadline = "ВРАЧИ НАЗЫВАЮТ НАПАДЕНИЕ «НЕОБЪЯСНИМЫМ»";
                    n.body = "Бригада скорой помощи, направленная в дом № 14 по Заводской улице, подверглась нападению в квартире № 37. Фельдшер госпитализирован.\n\nЖильцы подъезда жалуются на сырость и странный запах из-за стен.";
                    r.penalty = 3;
                    r.phrase = "+3: скорая отправлена на аномалию.";
                    break;
                case "Ignore":
                    n.headline = "ЖИЛЬЦЫ ПОДЪЕЗДА\nПРОПАЛИ БЕЗ ВЕСТИ";
                    n.subheadline = "СОСЕДИ ТРИ ДНЯ ЖАЛОВАЛИСЬ НА ШУМ";
                    n.body = "В доме № 14 по Заводской улице не могут найти жильцов двух квартир. Накануне соседи сообщали о грохоте и рычании за стеной.\n\nДверь квартиры № 37 обнаружена вскрытой изнутри.";
                    r.penalty = 2;
                    r.phrase = "+2: реальная угроза проигнорирована.";
                    break;
                default: // HungUp
                    n.headline = "НОЧНОЙ ВЫЗОВ\nОБОРВАЛСЯ НА ПОЛУСЛОВЕ";
                    n.subheadline = "ЖИТЕЛЬ ЗАВОДСКОЙ НЕ ДОЖДАЛСЯ ПОМОЩИ";
                    n.body = "Житель дома № 14 по Заводской улице, звонивший в Бюро с жалобой на соседей, бросил трубку, не дождавшись решения диспетчера.\n\nУтром его квартиру нашли пустой. В подъезде — следы сырости и ржавый налёт на стенах.";
                    r.penalty = 2;
                    r.phrase = "+2: абонент бросил трубку, угроза не устранена.";
                    break;
            }
            return r;
        }
    }
}
