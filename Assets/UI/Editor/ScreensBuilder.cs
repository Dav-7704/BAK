using Bak.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static Bak.UI.EditorTools.CallScreenBuilder;

namespace Bak.UI.EditorTools
{
    /// Собирает все экраны БАК по сетке 640×360 и связывает их в путь игрока.
    /// Меню: BAK → Build All Screens. Пересборка заменяет экраны в сцене и префабы.
    public static class ScreensBuilder
    {
        const string PrefabDir = "Assets/UI/Prefabs/";

        [MenuItem("BAK/Build All Screens")]
        public static void BuildAll()
        {
            DemoRunsItself = false;
            try { CallScreenBuilder.Build(); }
            finally { DemoRunsItself = true; }
            LoadFonts();

            var uiRoot = GameObject.Find("UIRoot");
            if (uiRoot == null) { Debug.LogError("[БАК] В сцене нет UIRoot"); return; }
            foreach (var n in new[] { "MainMenu", "PostStory", "Stats", "Settings", "Fade" })
            {
                var old = uiRoot.transform.Find(n);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }
            foreach (var c in uiRoot.GetComponents<GameFlowDemo>()) Object.DestroyImmediate(c);
            foreach (var c in uiRoot.GetComponents<ScreenRouter>()) Object.DestroyImmediate(c);

            var call = uiRoot.transform.Find("CallScreen").gameObject;
            var menu = BuildMainMenu(uiRoot.transform);
            var news = BuildPostStory(uiRoot.transform);
            var stats = BuildStats(uiRoot.transform);
            var settings = BuildSettings(uiRoot.transform);
            var fade = BuildFade(uiRoot.transform);

            menu.transform.SetSiblingIndex(0);
            call.transform.SetSiblingIndex(1);
            news.transform.SetSiblingIndex(2);
            stats.transform.SetSiblingIndex(3);
            settings.transform.SetSiblingIndex(4);
            fade.transform.SetAsLastSibling();

            var router = uiRoot.AddComponent<ScreenRouter>();
            router.mainMenu = menu.gameObject;
            router.call = call;
            router.postStory = news.gameObject;
            router.stats = stats.gameObject;
            router.settings = settings;
            router.fade = fade;

            var flow = uiRoot.AddComponent<GameFlowDemo>();
            flow.router = router;
            flow.menu = menu;
            flow.settings = settings;
            flow.callDemo = call.GetComponent<CallScreenDemo>();
            flow.news = news;
            flow.stats = stats;

            SavePrefab(menu.gameObject, "MainMenu");
            SavePrefab(news.gameObject, "PostStory");
            SavePrefab(stats.gameObject, "Stats");
            SavePrefab(settings.gameObject, "Settings");

            // В редакторе виден только главный экран; в Play экраны переключает ScreenRouter.
            menu.gameObject.SetActive(true);
            call.SetActive(false);
            news.gameObject.SetActive(false);
            stats.gameObject.SetActive(false);
            settings.gameObject.SetActive(false);
            fade.gameObject.SetActive(false);

            var scene = uiRoot.scene;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = menu.gameObject;
            Debug.Log("[БАК] Все экраны собраны");
        }

        // ───────────── Общая строка состояния ─────────────
        static TMP_Text StatusBar(RectTransform screen, string right)
        {
            var bar = R("StatusBar", screen, 0, 0, 640, 32);
            T("Logo", bar, 12, 8, 32, 16, "БАК", 8, BakPalette.Main, TextAlignmentOptions.MidlineLeft, _pixel);
            T("Terminal", bar, 50, 8, 250, 16, "ТЕРМИНАЛ ПЕРВИЧНОГО КОНТАКТА · v1.3", 7, BakPalette.Secondary, TextAlignmentOptions.MidlineLeft, null, 8);
            var r = T("Right", bar, 340, 8, 288, 16, right, 7, BakPalette.Secondary, TextAlignmentOptions.MidlineRight, null, 8);
            Solid(R("Divider", bar, 12, 24, 616, 1), BakPalette.Line);
            return r;
        }

        static RectTransform ScreenRoot(string name, Transform uiRoot)
        {
            var rt = R(name, uiRoot, 0, 0, 640, 360);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            return rt;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        /// Заглушка под арт: заливка панели + подпись + пунктирная рамка (арт придёт позже).
        static Image ArtSlot(RectTransform parent, string name, float x, float y, float w, float h, string label, out GameObject placeholder)
        {
            var slot = R(name, parent, x, y, w, h);
            var art = Solid(R("Art", slot, 0, 0, w, h), BakPalette.Panel);
            placeholder = T("Placeholder", slot, 0, 0, w, h, label, 7, BakPalette.Secondary, TextAlignmentOptions.Center).gameObject;
            Img(R("Frame", slot, 0, 0, w, h), "frames/panel_frame_locked", Image.Type.Tiled).fillCenter = false;
            return art;
        }

        /// Текстовая кнопка с инверсией при наведении.
        static Button TextButton(RectTransform parent, string name, float x, float y, float w, float h, string label, bool primary, float size = 7)
        {
            var rt = R(name, parent, x, y, w, h);
            Button b = primary
                ? Btn(rt, "buttons/btn_primary_normal", "buttons/btn_primary_hover", "buttons/btn_primary_pressed", "buttons/btn_disabled", Image.Type.Tiled)
                : Btn(rt, "buttons/btn_normal", "buttons/btn_hover", "buttons/btn_pressed", "buttons/btn_disabled", Image.Type.Tiled);
            var t = T("Label", rt, 0, 0, w, h, label, size, primary ? BakPalette.Bg : BakPalette.Main, TextAlignmentOptions.Center, null, 10);
            if (primary) PBT(b, BakPalette.Bg, BakPalette.Bg, BakPalette.Bg, BakPalette.Frame, t);
            else PBT(b, BakPalette.Main, BakPalette.Bg, BakPalette.Bg, BakPalette.Frame, t);
            return b;
        }

        // ───────────── 1. Главное меню: фоны на колонках 1–3 и 10–12, центр — колонки 4–9 ─────────────
        static MainMenuView BuildMainMenu(Transform uiRoot)
        {
            var root = ScreenRoot("MainMenu", uiRoot);
            var v = root.gameObject.AddComponent<MainMenuView>();
            StatusBar(root, "ТИХОРЕЧЕНСК · 2002");

            v.backgroundLeft = ArtSlot(root, "BackgroundLeft", 12, 32, 148, 316, "ФОН СЛЕВА\nпромзона, пиксель-арт", out _);
            v.backgroundRight = ArtSlot(root, "BackgroundRight", 480, 32, 148, 316, "ФОН СПРАВА\nгород и река, пиксель-арт", out _);

            var center = R("Center", root, 168, 32, 304, 316);
            var title = T("Title", center, 0, 52, 304, 64, "БЮРО\nАНОМАЛЬНОГО\nКОНТРОЛЯ", 16, BakPalette.Main, TextAlignmentOptions.Center, _pixel);
            title.lineSpacing = 25;
            T("Subtitle", center, 0, 124, 304, 12, "ОТДЕЛ ПЕРВИЧНОГО КОНТАКТА", 7, BakPalette.Secondary, TextAlignmentOptions.Center, null, 12);

            // три кнопки ≈150×28 с промежутком 8, по центру
            v.newGameButton = TextButton(center, "BtnNewGame", 77, 160, 150, 28, "НОВАЯ ИГРА", false);
            v.continueButton = TextButton(center, "BtnContinue", 77, 196, 150, 28, "ПРОДОЛЖИТЬ", false);
            v.settingsButton = TextButton(center, "BtnSettings", 77, 232, 150, 28, "НАСТРОЙКИ", false);
            return v;
        }

        // ───────────── 1a. Настройки: окно на колонках 4–9, меню затемнено (чёрный 78%) ─────────────
        static SettingsView BuildSettings(Transform uiRoot)
        {
            var root = R("Settings", uiRoot, 0, 0, 640, 360);
            Stretch(root);
            var dim = Solid(R("Dim", root, 0, 0, 640, 360), BakPalette.Dim);
            Stretch(dim.rectTransform);
            dim.raycastTarget = true; // клики по меню под окном не проходят

            var area = ScreenRoot("Area", root);
            var w = R("Window", area, 168, 119, 304, 146);
            Img(w, "frames/popup_frame", Image.Type.Sliced, true);
            var v = root.gameObject.AddComponent<SettingsView>();

            T("Title", w, 10, 0, 200, 16, "НАСТРОЙКИ", 7, BakPalette.Main, TextAlignmentOptions.MidlineLeft, null, 10);
            v.closeButton = Btn(R("Close", w, 284, 1, 14, 14), "icons/btn_close_normal", "icons/btn_close_hover", "icons/btn_close_hover", "icons/btn_close_normal", Image.Type.Simple);
            Solid(R("HeaderLine", w, 2, 16, 298, 1), BakPalette.Frame);

            v.music = VolumeRow(w, "MusicRow", 35, "МУЗЫКА", SettingsView.DefaultMusic);
            v.sounds = VolumeRow(w, "SoundsRow", 69, "ЗВУКИ", SettingsView.DefaultSounds);

            v.backButton = TextButton(w, "BtnBack", 194, 105, 95, 24, "◂ НАЗАД", false);
            return v;
        }

        static VolumeRowView VolumeRow(RectTransform window, string name, float y, string label, int value)
        {
            var row = R(name, window, 0, y, 301, 22);
            var v = row.gameObject.AddComponent<VolumeRowView>();
            v.label = T("Label", row, 16, 0, 56, 22, label, 7, BakPalette.Main, TextAlignmentOptions.MidlineLeft, null, 10);
            v.minusButton = Btn(R("Minus", row, 72, 0, 22, 22), "icons/btn_minus_normal", "icons/btn_minus_hover", "icons/btn_minus_hover", "icons/btn_minus_disabled", Image.Type.Simple);
            var segs = R("Segments", row, 102, 3, 129, 16);
            v.segments = new Image[10];
            for (int i = 0; i < 10; i++)
                v.segments[i] = Img(R("Seg" + (i + 1), segs, i * 13, 0, 12, 16), "hud/seg_full", Image.Type.Simple); // сегмент 6×8, ровно ×2
            v.segFull = S("hud/seg_full");
            v.segEmpty = S("hud/seg_empty");
            v.plusButton = Btn(R("Plus", row, 238, 0, 22, 22), "icons/btn_plus_normal", "icons/btn_plus_hover", "icons/btn_plus_hover", "icons/btn_plus_disabled", Image.Type.Simple);
            v.percent = T("Percent", row, 262, 0, 30, 22, value + "%", 7, BakPalette.Main, TextAlignmentOptions.MidlineRight);
            v.SetValue(value, false);
            return v;
        }

        // ───────────── 3. Пост-история: газета на колонках 2–11, «ДАЛЕЕ ▸» на колонках 9–11 ─────────────
        static NewspaperView BuildPostStory(Transform uiRoot)
        {
            var root = ScreenRoot("PostStory", uiRoot);
            var v = root.gameObject.AddComponent<NewspaperView>();
            v.statusRight = StatusBar(root, "ИТОГ ВЫЗОВА · ЗВОНОК 2 / 5");

            var paper = R("Newspaper", root, 64, 32, 512, 283);
            Img(paper, "frames/news_frame", Image.Type.Sliced);
            T("Masthead", paper, 0, 6, 509, 22, "ТИХОРЕЧЕНСКИЙ ВЕСТНИК", 16, BakPalette.Main, TextAlignmentOptions.Center, _pixel);
            Solid(R("MastheadLine", paper, 14, 31, 481, 1), BakPalette.Main);
            v.issue = T("Issue", paper, 14, 35, 160, 12, "№ 41 (2847)", 7, BakPalette.Secondary, TextAlignmentOptions.MidlineLeft, null, 6);
            T("Kind", paper, 174, 35, 161, 12, "ГОРОДСКАЯ ГАЗЕТА", 7, BakPalette.Secondary, TextAlignmentOptions.Center, null, 8);
            v.date = T("Date", paper, 335, 35, 160, 12, "16 ОКТЯБРЯ 2002", 7, BakPalette.Secondary, TextAlignmentOptions.MidlineRight, null, 6);

            v.headline = Wrap(T("Headline", paper, 14, 52, 481, 40, "В ДОМЕ НА ЗАВОДСКОЙ\nПРОВЕЛИ САНОБРАБОТКУ", 15, BakPalette.Main, TextAlignmentOptions.TopLeft, null, 4));
            v.subheadline = T("Subheadline", paper, 14, 96, 481, 12, "ЖИЛЬЦОВ ОДНОГО ПОДЪЕЗДА ВРЕМЕННО ОТСЕЛИЛИ", 7, BakPalette.Secondary, TextAlignmentOptions.MidlineLeft, null, 8);
            v.body = Wrap(T("Body", paper, 14, 112, 227, 150,
                "Ночью специалисты Бюро аномального контроля обработали дом № 14 по Заводской улице от опасной плесени, обнаруженной в стояках. По словам представителя Бюро, угрозы для соседних домов нет.\n\nЖильцов квартиры № 37 увезли в больницу с отравлением. Сосед, вызвавший службу, остался недоволен шумом во время работ.",
                8, BakPalette.Main, TextAlignmentOptions.TopLeft));
            v.body.lineSpacing = 8;

            v.illustration = ArtSlot(paper, "Illustration", 261, 110, 235, 145, "ИЛЛЮСТРАЦИЯ\nодна на звонок,\nисходы различаются текстом", out v.illustrationPlaceholder);
            v.caption = T("Caption", paper, 261, 259, 235, 12, "Дом № 14 после обработки. Фото редакции.", 7, BakPalette.Secondary, TextAlignmentOptions.MidlineLeft);
            v.caption.fontStyle = FontStyles.Italic;

            v.nextButton = TextButton(root, "BtnNext", 428, 320, 148, 28, "ДАЛЕЕ ▸", true, 8);
            return v;
        }

        // ───────────── 4. Личное дело: блок на колонках 3–10 по центру ─────────────
        static StatsView BuildStats(Transform uiRoot)
        {
            var root = ScreenRoot("Stats", uiRoot);
            var v = root.gameObject.AddComponent<StatsView>();
            v.statusRight = StatusBar(root, "15.10.2002");

            var block = R("Block", root, 116, 76, 408, 220);
            T("Title", block, 0, 4, 408, 20, "ЛИЧНОЕ ДЕЛО", 16, BakPalette.Main, TextAlignmentOptions.Center, _pixel);
            v.subtitle = T("Subtitle", block, 0, 27, 408, 12, "ДИСПЕТЧЕР Д. КОВАЛЁВ · СМЕНА 1", 7, BakPalette.Secondary, TextAlignmentOptions.Center, null, 10);
            Solid(R("Line", block, 0, 45, 408, 1), BakPalette.Line);

            var calls = R("CellCalls", block, 0, 54, 200, 90);
            Img(calls, "frames/panel_frame", Image.Type.Tiled);
            T("Label", calls, 0, 24, 200, 12, "ЗВОНКОВ ОБРАБОТАНО", 7, BakPalette.Secondary, TextAlignmentOptions.Center, null, 10);
            v.callsValue = T("Value", calls, 0, 42, 200, 24, "2 / 5", 16, BakPalette.Main, TextAlignmentOptions.Center, _pixel);

            var rep = R("CellReprimands", block, 208, 54, 200, 90);
            Img(rep, "frames/panel_frame", Image.Type.Tiled);
            T("Label", rep, 0, 14, 200, 12, "ВЫГОВОРЫ", 7, BakPalette.Secondary, TextAlignmentOptions.Center, null, 10);
            v.reprimandsValue = T("Value", rep, 0, 30, 200, 24, "1 / 3", 16, BakPalette.Main, TextAlignmentOptions.Center, _pixel);
            var sq = R("Squares", rep, 78, 62, 44, 12);
            var counter = sq.gameObject.AddComponent<ReprimandCounter>();
            counter.empty = S("hud/reprimand_big_empty");
            counter.full = S("hud/reprimand_big_full");
            counter.squares = new Image[3];
            for (int i = 0; i < 3; i++)
                counter.squares[i] = Img(R("Square" + (i + 1), sq, i * 16, 0, 12, 12), "hud/reprimand_big_empty", Image.Type.Simple);
            counter.SetCount(1);
            v.reprimandSquares = counter;

            var last = R("LastCall", block, 0, 152, 408, 22);
            Img(last, "frames/panel_frame_locked", Image.Type.Tiled);
            T("Label", last, 12, 0, 96, 22, "ПОСЛЕДНИЙ ВЫЗОВ", 7, BakPalette.Secondary, TextAlignmentOptions.MidlineLeft, null, 8);
            v.lastCall = T("Text", last, 112, 0, 288, 22, "Без замечаний. Угроза устранена.", 7, BakPalette.Main, TextAlignmentOptions.MidlineLeft);

            v.menuButton = TextButton(block, "BtnMenu", 0, 190, 200, 28, "⌂ ГЛАВНОЕ МЕНЮ", false, 8);
            v.nextButton = TextButton(block, "BtnNext", 208, 190, 200, 28, "СЛЕДУЮЩИЙ ЗВОНОК ▸", true, 8);
            v.nextLabel = v.nextButton.GetComponentInChildren<TMP_Text>();
            return v;
        }

        // ───────────── Затемнение (финалы: без надписей и кнопок) ─────────────
        static CanvasGroup BuildFade(Transform uiRoot)
        {
            var rt = R("Fade", uiRoot, 0, 0, 640, 360);
            Stretch(rt);
            var img = Solid(rt, Color.black);
            img.raycastTarget = true;
            var cg = rt.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            return cg;
        }

        static void SavePrefab(GameObject go, string name)
        {
            EnsureFolder("Assets/UI/Prefabs");
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, PrefabDir + name + ".prefab", InteractionMode.AutomatedAction);
        }
    }
}
