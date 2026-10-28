using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI
{
    /// Окно настроек поверх меню (меню затемнено, чёрный 78%). Музыка 70%, звуки 80% по умолчанию; значения сохраняются.
    /// Закрытие — крестик или «◂ НАЗАД». При изменении громкости звуков проигрывается щелчок.
    public class SettingsView : MonoBehaviour
    {
        public VolumeRowView music;
        public VolumeRowView sounds;
        public Button closeButton;
        public Button backButton;
        [Tooltip("Источник фоновой музыки (необязательно)")] public AudioSource musicSource;
        [Tooltip("Щелчок для проверки громкости звуков (необязательно)")] public AudioSource clickSource;

        public const string MusicKey = "bak.music";
        public const string SoundsKey = "bak.sounds";
        public const int DefaultMusic = 70;
        public const int DefaultSounds = 80;

        public static float MusicVolume => PlayerPrefs.GetInt(MusicKey, DefaultMusic) / 100f;
        public static float SoundsVolume => PlayerPrefs.GetInt(SoundsKey, DefaultSounds) / 100f;

        public event Action Closed;

        void Awake()
        {
            closeButton.onClick.AddListener(Close);
            backButton.onClick.AddListener(Close);
            music.Changed += v =>
            {
                PlayerPrefs.SetInt(MusicKey, v);
                if (musicSource != null) musicSource.volume = v / 100f;
            };
            sounds.Changed += v =>
            {
                PlayerPrefs.SetInt(SoundsKey, v);
                if (clickSource != null) clickSource.PlayOneShot(clickSource.clip, v / 100f);
            };
        }

        public void Open()
        {
            music.SetValue(PlayerPrefs.GetInt(MusicKey, DefaultMusic), false);
            sounds.SetValue(PlayerPrefs.GetInt(SoundsKey, DefaultSounds), false);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void Close()
        {
            PlayerPrefs.Save();
            gameObject.SetActive(false);
            Closed?.Invoke();
        }
    }
}
