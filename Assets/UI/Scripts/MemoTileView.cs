using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bak.UI
{
    public enum MemoTileState { Sealed, Available }

    /// Плитка «Памятка» на рабочем столе: вся плитка — одна кнопка, открывает памятку.
    /// Опечатана в прологе (пунктир), становится активной после реплики босса.
    public class MemoTileView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Button button;
        public TMP_Text title;
        public TMP_Text status;
        public GameObject sealedContent;
        public GameObject availableContent;
        public Image footer;
        public TMP_Text footerText;
        public Sprite footerNormal, footerHover;

        public string statusSealed = "ЗАКРЫТО";
        public string statusAvailable = "ДСП";

        [SerializeField] MemoTileState state = MemoTileState.Sealed;

        public MemoTileState State => state;
        public event Action Clicked;

        void Awake()
        {
            button.onClick.AddListener(() => Clicked?.Invoke());
        }

        void OnEnable() => SetState(state);

        public void SetState(MemoTileState newState)
        {
            state = newState;
            bool available = state == MemoTileState.Available;
            button.interactable = available;
            sealedContent.SetActive(!available);
            availableContent.SetActive(available);
            footer.gameObject.SetActive(available);
            status.text = available ? statusAvailable : statusSealed;
            title.color = available ? BakPalette.Main : BakPalette.Secondary;
            status.color = BakPalette.Secondary;
            SetFooterHover(false);
        }

        public void OnPointerEnter(PointerEventData e) { if (button.interactable) SetFooterHover(true); }
        public void OnPointerExit(PointerEventData e) => SetFooterHover(false);

        void SetFooterHover(bool hover)
        {
            if (footer == null) return;
            footer.sprite = hover ? footerHover : footerNormal;
            if (footerText != null) footerText.color = BakPalette.Bg;
        }
    }
}
