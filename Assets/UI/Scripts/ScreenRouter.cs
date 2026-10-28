using System;
using System.Collections;
using UnityEngine;

namespace Bak.UI
{
    public enum ScreenId { MainMenu, Call, PostStory, Stats }

    /// Показывает один экран за раз и делает затемнение экрана (финалы → главное меню).
    public class ScreenRouter : MonoBehaviour
    {
        public GameObject mainMenu;
        public GameObject call;
        public GameObject postStory;
        public GameObject stats;
        public SettingsView settings;
        [Tooltip("Чёрный слой поверх всего для затемнения")] public CanvasGroup fade;
        public float fadeDuration = 0.6f;

        public ScreenId Current { get; private set; }

        public void Show(ScreenId id)
        {
            Current = id;
            mainMenu.SetActive(id == ScreenId.MainMenu);
            call.SetActive(id == ScreenId.Call);
            postStory.SetActive(id == ScreenId.PostStory);
            stats.SetActive(id == ScreenId.Stats);
            if (settings != null) settings.gameObject.SetActive(false);
        }

        /// Экран темнеет (без надписей и кнопок), затем вызывается then и экран снова проявляется.
        public void FadeThrough(Action then, float hold = 0.8f) => StartCoroutine(FadeRoutine(then, hold));

        IEnumerator FadeRoutine(Action then, float hold)
        {
            fade.gameObject.SetActive(true);
            fade.blocksRaycasts = true;
            yield return Animate(0f, 1f);
            yield return new WaitForSecondsRealtime(hold);
            then?.Invoke();
            yield return Animate(1f, 0f);
            fade.blocksRaycasts = false;
            fade.gameObject.SetActive(false);
        }

        IEnumerator Animate(float from, float to)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                fade.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }
            fade.alpha = to;
        }
    }
}
