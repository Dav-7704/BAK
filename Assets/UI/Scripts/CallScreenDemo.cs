using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bak.UI
{
    /// Демо-сценарий основного экрана (тексты из макетов). Не игровая логика:
    /// настоящий сценарий будет читать данные звонков. Сообщает об исходе через CallFinished.
    public class CallScreenDemo : MonoBehaviour
    {
        public CallScreenView view;
        [Tooltip("Запускать инструктаж сам при старте (выключено, если экраном управляет GameFlowDemo)")]
        public bool runOnPlay = true;
        [Tooltip("Сколько вопросов надо задать, чтобы открылась панель диспетчеризации")]
        public int requiredQuestions = 2;
        public int startPatience = 70;

        /// Исход звонка: Militia, Ambulance, Liquidators, Ignore или HungUp.
        public event Action<string> CallFinished;

        class Line
        {
            public string speaker, text;
            public bool dispatcher;
            public Action onShown;
            public Line(string s, string t, bool d = false, Action a = null) { speaker = s; text = t; dispatcher = d; onShown = a; }
        }

        class Option
        {
            public string question, answer;
            public int patienceDelta;
            public Option(string q, string a, int d) { question = q; answer = a; patienceDelta = d; }
        }

        readonly Queue<Line> _queue = new Queue<Line>();
        Action _onQueueDone;
        readonly List<Option> _options = new List<Option>();
        int _asked;
        bool _finished;
        bool _subscribed;

        const string Boss = "НАЧ. СМЕНЫ", Disp = "ДИСПЕТЧЕР", Caller = "АБОНЕНТ";

        void Start()
        {
            Subscribe();
            // если экранами управляет GameFlowDemo, демо само не стартует
            if (runOnPlay && FindAnyObjectByType<GameFlowDemo>() == null) StartBriefing();
        }

        void Subscribe()
        {
            if (_subscribed) return;
            if (view == null) view = GetComponent<CallScreenView>();
            view.log.Clicked += OnLogClicked;
            view.log.TypingFinished += OnTypingFinished;
            view.options.Picked += OnPicked;
            view.dispatch.Dispatched += OnDispatched;
            _subscribed = true;
        }

        [ContextMenu("Демо: инструктаж босса")]
        public void StartBriefing() => StartBriefing(null);

        public void StartBriefing(Action onDone)
        {
            Subscribe();
            ResetScreen();
            view.SetStatus("ИНСТРУКТАЖ", "14.10.2002");
            view.SetDispatcher("ДИСПЕТЧЕР", "Д. Ковалёв");
            view.SetCaller("НАЧ. СМЕНЫ", "Б. А. Сытин");
            view.memoTile.SetState(MemoTileState.Sealed);
            view.SetPhase(CallPhase.Boss);
            Enqueue(
                new Line(Boss, "Так, новенький. Наушники надел? Слушай внимательно, повторять не буду."),
                new Line(Boss, "Выезд ликвидаторов — сорок тысяч. Ошибка стоит дороже. А ты, стажёр, стоишь дешевле всего. Ха!"),
                new Line(Boss, "Держи памятку. Сверяйся с ней на каждом звонке.", false,
                    () => view.memoTile.SetState(MemoTileState.Available)),
                new Line(Boss, "Три выговора — и до свидания. Надеюсь, продержишься подольше, чем прошлый."));
            _onQueueDone = onDone ?? (() => StartCall(2, 5));
            ShowNext();
        }

        [ContextMenu("Демо: звонок (сбор данных)")]
        public void StartCallDefault() => StartCall(2, 5);

        public void StartCall(int callNumber, int totalCalls, string date = "15.10.2002")
        {
            Subscribe();
            ResetScreen();
            view.SetStatus("ЗВОНОК " + callNumber + " / " + totalCalls, date);
            view.SetCaller("АБОНЕНТ", "Заводской р-н");
            view.memoTile.SetState(MemoTileState.Available);
            view.patience.SetValue(startPatience);
            view.SetPhase(CallPhase.Gathering);
            Enqueue(
                new Line(Disp, "Бюро аномального контроля, слушаю вас.", true),
                new Line(Caller, "Слышь, у соседей третий день грохот! Мебель ломают, полы вскрывают!"),
                new Line(Disp, "Назовите адрес, пожалуйста.", true),
                new Line(Caller, "Заводская, четырнадцать, тридцать шестая. Шли своих ликвидаторов, они быстрее милиции!"),
                new Line(Disp, "Что ещё вы слышите, кроме грохота?", true),
                new Line(Caller, "Рычит кто-то. И хлюпает, мокро так… Долго ещё допрашивать будешь?"));
            _onQueueDone = StartAnalysis;
            ShowNext();
        }

        /// Финал в режиме босса: после последней реплики — onDone (обычно затемнение и главное меню).
        public void StartFinal(bool fired, Action onDone)
        {
            Subscribe();
            ResetScreen();
            view.SetStatus(fired ? "ПРИКАЗ" : "КОНЕЦ СМЕНЫ", "15.10.2002");
            view.SetCaller("НАЧ. СМЕНЫ", "Б. А. Сытин");
            view.memoTile.SetState(MemoTileState.Available);
            view.SetPhase(CallPhase.Boss);
            if (fired)
                Enqueue(
                    new Line(Boss, "Ковалёв. Контракт расторгнут в связи с профнепригодностью."),
                    new Line(Boss, "За тобой уже выехала служба безопасности. Не переживай: в Бюро ничего не пропадает. Даже ты."));
            else
                Enqueue(
                    new Line(Boss, "Смена закрыта. Остаёшься в штате, стажёр."),
                    new Line(Boss, "Знаешь, почему прошлый не продержался? Кормовая база нынче дорогая. Шучу. Наверное."));
            _onQueueDone = onDone;
            ShowNext();
        }

        [ContextMenu("Демо: анализ (варианты ответа)")]
        public void StartAnalysis()
        {
            _onQueueDone = null;
            _queue.Clear();
            _asked = 0;
            _options.Clear();
            _options.Add(new Option("Чем пахнет в подъезде или из-под их двери?",
                "Сыростью, гнилью какой-то… Какая разница, чем воняет из-под их двери?!", -10));
            _options.Add(new Option("Когда вы последний раз видели соседей? Как они себя вели?",
                "Неделю назад. Бледные, не здороваются. Теперь только рычат за стеной.", +10));
            _options.Add(new Option("Может, они просто делают ремонт?",
                "Ремонт?! Ночью? С рычанием?! Вы там издеваетесь?", -30));
            view.SetPhase(CallPhase.Analysis);
            ShowOptions();
        }

        void ResetScreen()
        {
            StopAllCoroutines();
            _finished = false;
            _queue.Clear();
            _onQueueDone = null;
            view.log.Clear();
            view.dispatch.ResetPanel();
            view.helpGroup.CloseAll();
            if (view.memoWindow != null) view.memoWindow.gameObject.SetActive(false);
        }

        void Enqueue(params Line[] lines)
        {
            foreach (var l in lines) _queue.Enqueue(l);
        }

        void ShowNext()
        {
            if (_queue.Count == 0)
            {
                var done = _onQueueDone;
                _onQueueDone = null;
                done?.Invoke();
                return;
            }
            var line = _queue.Dequeue();
            view.log.AddLine(line.speaker, line.text, line.dispatcher);
            view.SetSpeaker(!line.dispatcher);
            line.onShown?.Invoke();
        }

        void OnLogClicked()
        {
            if (_finished) return;
            if (view.Phase == CallPhase.Boss || view.Phase == CallPhase.Gathering) ShowNext();
        }

        void ShowOptions()
        {
            var texts = new List<string>();
            foreach (var o in _options) texts.Add(o.question);
            if (texts.Count > 0) view.options.ShowOptions(texts);
            else view.options.ShowEmpty("Вопросов больше нет — примите решение на панели.");
        }

        void OnPicked(int index)
        {
            if (view.Phase != CallPhase.Analysis || index >= _options.Count) return;
            var o = _options[index];
            _options.RemoveAt(index);
            _asked++;
            view.options.SetInteractable(false);
            view.log.AddLine(Disp, o.question, true, false);
            view.log.AddLine(Caller, o.answer, false);
            view.SetSpeaker(true);
            view.patience.Add(o.patienceDelta);
        }

        void OnTypingFinished()
        {
            if (view.Phase != CallPhase.Analysis || _finished) return;
            if (view.patience.Value <= 0)
            {
                view.SetPhase(CallPhase.HungUp);
                _finished = true;
                StartCoroutine(Outro("HungUp"));
                return;
            }
            if (_asked >= requiredQuestions) view.dispatch.SetLocked(false);
            ShowOptions();
        }

        void OnDispatched(DispatchService service)
        {
            _finished = true;
            view.options.ShowEmpty("Звонок завершён.");
            StartCoroutine(Outro(service.ToString()));
        }

        IEnumerator Outro(string outcome)
        {
            yield return new WaitForSecondsRealtime(1.5f);
            Debug.Log("[БАК демо] исход звонка: " + outcome);
            CallFinished?.Invoke(outcome);
        }
    }
}
