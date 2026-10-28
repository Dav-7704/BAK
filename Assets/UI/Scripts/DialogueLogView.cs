using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bak.UI
{
    /// Журнал вызова: реплики печатаются по буквам, новые — снизу.
    /// Клик по журналу: если текст печатается — допечатать сразу, иначе событие Clicked (следующая реплика).
    public class DialogueLogView : MonoBehaviour, IPointerClickHandler
    {
        public ScrollRect scroll;
        public RectTransform content;
        public LogEntryView entryTemplate;
        [Tooltip("Нижняя полоса «▼ ЛКМ — ДАЛЕЕ» — маркер ожидания клика")]
        public GameObject waitMarker;
        [Tooltip("Плашка «АБОНЕНТ ПОЛОЖИЛ ТРУБКУ»")]
        public GameObject hangupBadge;
        public float charsPerSecond = 45f;
        public string caret = "▌";

        public event Action Clicked;
        public event Action TypingFinished;

        public bool IsTyping => _typing != null;

        Coroutine _typing;
        LogEntryView _current;
        string _currentText;

        void Awake()
        {
            if (entryTemplate != null) entryTemplate.gameObject.SetActive(false);
        }

        public void Clear()
        {
            StopTyping(false);
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var child = content.GetChild(i);
                if (entryTemplate != null && child == entryTemplate.transform) continue;
                Destroy(child.gameObject);
            }
            SetHangup(false);
        }

        public void AddLine(string speakerName, string text, bool isDispatcher, bool typewriter = true)
        {
            StopTyping(true);
            var entry = Instantiate(entryTemplate, content);
            entry.gameObject.SetActive(true);
            entry.name = "Entry";
            entry.Set(speakerName, typewriter ? caret : text, isDispatcher);
            _current = entry;
            _currentText = text;
            if (typewriter && isActiveAndEnabled) _typing = StartCoroutine(TypeRoutine());
            else ScrollToBottom();
        }

        public void SkipTyping() => StopTyping(true);

        public void SetWaitMarker(bool on)
        {
            if (waitMarker != null) waitMarker.SetActive(on);
        }

        public void SetHangup(bool on)
        {
            if (hangupBadge != null) hangupBadge.SetActive(on);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (IsTyping) SkipTyping();
            else Clicked?.Invoke();
        }

        IEnumerator TypeRoutine()
        {
            int shown = 0;
            float acc = 0f;
            while (shown < _currentText.Length)
            {
                acc += Time.unscaledDeltaTime * charsPerSecond;
                int step = Mathf.FloorToInt(acc);
                if (step > 0)
                {
                    acc -= step;
                    shown = Mathf.Min(_currentText.Length, shown + step);
                    _current.body.text = _currentText.Substring(0, shown) + caret;
                    ScrollToBottom();
                }
                yield return null;
            }
            _typing = null;
            _current.body.text = _currentText;
            ScrollToBottom();
            TypingFinished?.Invoke();
        }

        void StopTyping(bool complete)
        {
            if (_typing == null) return;
            StopCoroutine(_typing);
            _typing = null;
            if (complete && _current != null)
            {
                _current.body.text = _currentText;
                ScrollToBottom();
                TypingFinished?.Invoke();
            }
        }

        void ScrollToBottom()
        {
            if (scroll == null) return;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            scroll.verticalNormalizedPosition = 0f;
        }
    }
}
