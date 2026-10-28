using Bak.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Bak.UI.EditorTools
{
    /// Собирает основной экран звонка по сетке спецификации (640×360, 12 колонок по 44 px, межколонник 8, поля 12).
    /// Каждый элемент — отдельный объект-слой. Меню: BAK → Build Call Screen. Пересборка заменяет экран в сцене и префаб.
    public static class CallScreenBuilder
    {
        const string ArtRoot = "Assets/Art/UI/";
        const string PrefabPath = "Assets/UI/Prefabs/CallScreen.prefab";
        const string MemoDataPath = "Assets/UI/Data/MemoData.asset";

        internal static TMP_FontAsset _mono, _pixel;

        /// Запускать демо-сценарий звонка самостоятельно (false — экраном управляет GameFlowDemo).
        internal static bool DemoRunsItself = true;

        internal static void LoadFonts()
        {
            _mono = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PTMono/PTMono SDF.asset");
            _pixel = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/PressStart2P/PressStart2P Raster.asset");
        }

        [MenuItem("BAK/Build Call Screen")]
        public static void Build()
        {
            LoadFonts();

            var uiRoot = GameObject.Find("UIRoot");
            if (uiRoot == null) { Debug.LogError("[БАК] В сцене нет UIRoot"); return; }
            var old = uiRoot.transform.Find("CallScreen");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var screenRt = R("CallScreen", uiRoot.transform, 0, 0, 640, 360);
            screenRt.anchorMin = screenRt.anchorMax = screenRt.pivot = new Vector2(0.5f, 0.5f);
            screenRt.anchoredPosition = Vector2.zero;
            var view = screenRt.gameObject.AddComponent<CallScreenView>();
            var helpGroup = screenRt.gameObject.AddComponent<HelpPopupGroup>();
            view.helpGroup = helpGroup;

            BuildStatusBar(screenRt, view);
            BuildLeftZone(screenRt, view);
            BuildRightZone(screenRt, view);
            BuildMemoWindow(screenRt, view);
            BuildHelpCards(screenRt, view);
            helpGroup.buttons = new[] { view.patienceHelp, view.reprimandHelp };

            var demo = screenRt.gameObject.AddComponent<CallScreenDemo>();
            demo.view = view;
            demo.runOnPlay = DemoRunsItself;

            view.memoWindow.gameObject.SetActive(false);

            // Стартовое состояние для редактора: звонок, сбор данных, памятка доступна.
            view.memoTile.SetState(MemoTileState.Available);
            view.SetPhase(CallPhase.Gathering);
            view.SetSpeaker(true);

            EnsureFolder("Assets/UI/Prefabs");
            PrefabUtility.SaveAsPrefabAssetAndConnect(screenRt.gameObject, PrefabPath, InteractionMode.AutomatedAction);
            var scene = screenRt.gameObject.scene;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = screenRt.gameObject;
            Debug.Log("[БАК] Основной экран собран: " + PrefabPath);
        }

        // ───────────── Строка состояния (колонки 1–12, y 8..24, линия снизу) ─────────────
        static void BuildStatusBar(RectTransform screen, CallScreenView view)
        {
            var bar = R("StatusBar", screen, 0, 0, 640, 32);
            T("Logo", bar, 12, 8, 32, 16, "БАК", 8, BakPalette.Main, TextAlignmentOptions.MidlineLeft, _pixel);
            T("Terminal", bar, 50, 8, 250, 16, "ТЕРМИНАЛ ПЕРВИЧНОГО КОНТАКТА · v1.3", 7, BakPalette.Secondary, TextAlignmentOptions.MidlineLeft, null, 8);
            view.contextLabel = T("Context", bar, 300, 8, 140, 16, "ЗВОНОК 2 / 5", 7, BakPalette.Secondary, TextAlignmentOptions.MidlineRight, null, 8);
            view.dateLabel = T("Date", bar, 446, 8, 70, 16, "15.10.2002", 7, BakPalette.Secondary, TextAlignmentOptions.MidlineRight, null, 8);
            T("ReprimandsLabel", bar, 516, 8, 68, 16, "ВЫГОВОРЫ", 7, BakPalette.Main, TextAlignmentOptions.MidlineRight, null, 8);

            var rep = R("Reprimands", bar, 588, 12, 25, 7);
            var counter = rep.gameObject.AddComponent<ReprimandCounter>();
            counter.empty = S("hud/reprimand_empty");
            counter.full = S("hud/reprimand_full");
            counter.squares = new Image[3];
            for (int i = 0; i < 3; i++)
                counter.squares[i] = Img(R("Square" + (i + 1), rep, i * 9, 0, 7, 7), "hud/reprimand_empty", Image.Type.Simple);
            view.reprimands = counter;

            view.reprimandHelp = HelpBtn("ReprimandHelp", bar, 617, 10);
            Solid(R("Divider", bar, 12, 24, 616, 1), BakPalette.Line);
        }

        // ───────────── Левая зона: колонки 1–4 (x 12, w 200) ─────────────
        static void BuildLeftZone(RectTransform screen, CallScreenView view)
        {
            var left = R("LeftZone", screen, 12, 32, 200, 316);

            Portrait(left, "Dispatcher", 0, "ДИСПЕТЧЕР", "Д. Ковалёв", "ПОРТРЕТ\nдиспетчер",
                out view.dispatcherPortrait, out view.dispatcherPlaceholder, out view.dispatcherFrame,
                out view.dispatcherRole, out view.dispatcherName);
            Portrait(left, "Caller", 104, "НАЧ. СМЕНЫ", "Б. А. Сытин", "ПОРТРЕТ\nсобеседник",
                out view.callerPortrait, out view.callerPlaceholder, out view.callerFrame,
                out view.callerRole, out view.callerName);
            view.frameIdle = S("frames/panel_frame");
            view.frameSpeaking = S("frames/memo_tile_hover");
            view.callerFrame.sprite = view.frameSpeaking;

            // Плитка «Памятка»: вся плитка — одна кнопка
            var tile = R("MemoTile", left, 0, 126, 200, 190);
            var tileBtn = Btn(tile, "frames/memo_tile_normal", "frames/memo_tile_hover", "frames/memo_tile_hover", "frames/memo_tile_sealed", Image.Type.Tiled);
            var tv = tile.gameObject.AddComponent<MemoTileView>();
            tv.button = tileBtn;
            tv.title = T("Title", tile, 8, 0, 140, 16, "ПАМЯТКА СОТРУДНИКА", 7, BakPalette.Main, TextAlignmentOptions.MidlineLeft, null, 8);
            tv.status = T("Status", tile, 130, 0, 62, 16, "ЗАКРЫТО", 7, BakPalette.Secondary, TextAlignmentOptions.MidlineRight, null, 8);
            Solid(R("HeaderLine", tile, 2, 16, 196, 1), BakPalette.Line);

            var sealedRt = R("SealedContent", tile, 0, 17, 200, 171);
            var sealedText = T("Text", sealedRt, 10, 0, 180, 171,
                "Папка опечатана.\nНачальник выдаёт её\nв конце инструктажа —\nплитка станет активной.",
                7, BakPalette.Secondary, TextAlignmentOptions.Center);
            sealedText.lineSpacing = 20;
            tv.sealedContent = sealedRt.gameObject;

            var avail = R("AvailableContent", tile, 0, 17, 200, 155);
            var folder = R("FolderArt", avail, 52, 22, 96, 72);
            Solid(folder, BakPalette.Panel);
            Img(R("Frame", folder, 0, 0, 96, 72), "frames/panel_frame_locked", Image.Type.Tiled).fillCenter = false;
            T("Placeholder", folder, 0, 0, 96, 72, "ПАПКА\n(арт позже)", 7, BakPalette.Secondary, TextAlignmentOptions.Center);
            var hint = T("Hint", avail, 10, 104, 180, 30, "5 разделов · сверяйте слова\nабонента с регламентом", 7, BakPalette.Secondary, TextAlignmentOptions.Center);
            hint.textWrappingMode = TextWrappingModes.Normal;
            tv.availableContent = avail.gameObject;

            var footer = R("Footer", tile, 2, 172, 196, 16);
            tv.footer = Img(footer, "frames/memo_tile_footer_normal", Image.Type.Sliced);
            tv.footerText = T("Label", footer, 0, 0, 196, 16, "▸ ОТКРЫТЬ ПАМЯТКУ", 7, BakPalette.Bg, TextAlignmentOptions.Center, null, 8);
            tv.footerText.fontStyle = FontStyles.Bold;
            tv.footerNormal = S("frames/memo_tile_footer_normal");
            tv.footerHover = S("frames/memo_tile_footer_hover");
            view.memoTile = tv;
        }

        static void Portrait(RectTransform left, string id, float x, string role, string name, string placeholder,
            out Image art, out GameObject placeholderGo, out Image frame, out TMP_Text roleText, out TMP_Text nameText)
        {
            var p = R("Portrait" + id, left, x, 0, 96, 96);
            art = Solid(R("Art", p, 0, 0, 96, 96), BakPalette.Panel);
            var ph = T("Placeholder", p, 0, 0, 96, 96, placeholder, 7, BakPalette.Secondary, TextAlignmentOptions.Center);
            placeholderGo = ph.gameObject;
            frame = Img(R("Frame", p, 0, 0, 96, 96), "frames/panel_frame", Image.Type.Tiled);
            frame.fillCenter = false; // рамка поверх арта: середина прозрачная
            var cap = R("Caption" + id, left, x, 100, 96, 22);
            roleText = T("Role", cap, 0, 0, 96, 11, role, 7, BakPalette.Main, TextAlignmentOptions.MidlineLeft, null, 6);
            nameText = T("Name", cap, 0, 11, 96, 11, name, 7, BakPalette.Secondary, TextAlignmentOptions.MidlineLeft);
        }

        // ───────────── Правая зона: колонки 5–12 (x 220, w 408) ─────────────
        static void BuildRightZone(RectTransform screen, CallScreenView view)
        {
            var right = R("RightZone", screen, 220, 32, 408, 316);
            BuildLog(right, view);
            BuildOptions(right, view);
            BuildDispatch(right, view);
        }

        static void BuildLog(RectTransform right, CallScreenView view)
        {
            var logRt = R("Log", right, 0, 0, 408, 174);
            Img(logRt, "frames/panel_frame", Image.Type.Tiled, true);
            var log = logRt.gameObject.AddComponent<DialogueLogView>();

            view.logTitle = T("Title", logRt, 10, 0, 220, 16, "ЖУРНАЛ ВЫЗОВА", 7, BakPalette.Main, TextAlignmentOptions.MidlineLeft, null, 8);

            var pg = R("PatienceGroup", logRt, 228, 0, 172, 16);
            T("Label", pg, 0, 0, 60, 16, "ТЕРПЕНИЕ", 7, BakPalette.Main, TextAlignmentOptions.MidlineRight, null, 8);
            var barRt = R("Bar", pg, 66, 4, 69, 8);
            var bar = barRt.gameObject.AddComponent<PatienceBar>();
            bar.full = S("hud/seg_full");
            bar.empty = S("hud/seg_empty");
            bar.frozen = S("hud/seg_frozen");
            bar.critical = S("hud/seg_critical");
            bar.segments = new Image[10];
            for (int i = 0; i < 10; i++)
                bar.segments[i] = Img(R("Seg" + (i + 1), barRt, i * 7, 0, 6, 8), "hud/seg_frozen", Image.Type.Simple);
            bar.percentLabel = T("Percent", pg, 136, 0, 22, 16, "— —", 7, BakPalette.Main, TextAlignmentOptions.MidlineRight);
            view.patienceHelp = HelpBtn("PatienceHelp", pg, 161, 2);
            view.patience = bar;
            view.patienceGroup = pg.gameObject;
            view.patienceHiddenLabel = T("PatienceHidden", logRt, 228, 0, 170, 16, "шкала терпения скрыта", 7, BakPalette.Secondary, TextAlignmentOptions.MidlineRight, null, 6);

            Solid(R("HeaderLine", logRt, 1, 16, 406, 1), BakPalette.Frame);

            // Прокручиваемая область реплик
            var viewport = R("Viewport", logRt, 10, 21, 388, 130);
            var vpImg = viewport.gameObject.AddComponent<Image>();
            vpImg.color = new Color(0, 0, 0, 0);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = R("Content", viewport, 0, 0, 388, 0);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.sizeDelta = new Vector2(0, 0);
            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 3;
            vlg.childControlWidth = vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = false;
            scroll.scrollSensitivity = 12;

            var entry = R("EntryTemplate", content, 0, 0, 388, 12);
            var hlg = entry.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childControlWidth = hlg.childControlHeight = true;
            hlg.childForceExpandWidth = hlg.childForceExpandHeight = false;
            var ev = entry.gameObject.AddComponent<LogEntryView>();
            ev.speaker = T("Speaker", entry, 0, 0, 60, 12, "АБОНЕНТ", 8, BakPalette.Secondary, TextAlignmentOptions.TopLeft);
            var le = ev.speaker.gameObject.AddComponent<LayoutElement>();
            le.minWidth = le.preferredWidth = 60;
            le.flexibleWidth = 0;
            ev.body = T("Body", entry, 68, 0, 320, 12, "Реплика", 8, BakPalette.Main, TextAlignmentOptions.TopLeft);
            ev.body.textWrappingMode = TextWrappingModes.Normal;
            ev.body.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            entry.gameObject.SetActive(false);

            // Нижняя полоса «▼ ЛКМ — ДАЛЕЕ»
            var wait = R("WaitMarker", logRt, 1, 154, 406, 19);
            Solid(R("Line", wait, 0, 0, 406, 1), BakPalette.Frame);
            var row = R("Row", wait, 196, 1, 200, 18);
            var rh = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rh.childAlignment = TextAnchor.MiddleRight;
            rh.spacing = 4;
            rh.childControlWidth = true;
            rh.childControlHeight = false;
            rh.childForceExpandWidth = rh.childForceExpandHeight = false;
            var arrow = Img(R("Arrow", row, 0, 0, 7, 4), "icons/icon_arrow_down", Image.Type.Simple);
            var ale = arrow.gameObject.AddComponent<LayoutElement>();
            ale.minWidth = ale.preferredWidth = 7;
            T("Label", row, 0, 0, 80, 18, "ЛКМ — ДАЛЕЕ", 7, BakPalette.Main, TextAlignmentOptions.MidlineRight, null, 8);

            // Плашка «АБОНЕНТ ПОЛОЖИЛ ТРУБКУ»
            var badge = R("HangupBadge", logRt, 10, 156, 124, 14);
            Img(badge, "hud/badge_hangup", Image.Type.Sliced);
            T("Label", badge, 0, 0, 124, 14, "АБОНЕНТ ПОЛОЖИЛ ТРУБКУ", 7, BakPalette.Bg, TextAlignmentOptions.Center, null, 6).fontStyle = FontStyles.Bold;
            badge.gameObject.SetActive(false);

            log.scroll = scroll;
            log.content = content;
            log.entryTemplate = ev;
            log.waitMarker = wait.gameObject;
            log.hangupBadge = badge.gameObject;
            view.log = log;
        }

        static void BuildOptions(RectTransform right, CallScreenView view)
        {
            var opt = R("Options", right, 0, 182, 408, 78);
            var ov = opt.gameObject.AddComponent<ResponseOptionsView>();
            ov.frame = Img(opt, "frames/panel_frame_locked", Image.Type.Tiled);
            ov.activeFrame = S("frames/panel_frame");
            ov.lockedFrame = S("frames/panel_frame_locked");
            ov.title = T("Title", opt, 10, 0, 200, 16, "ВАРИАНТЫ ОТВЕТА", 7, BakPalette.Main, TextAlignmentOptions.MidlineLeft, null, 8);
            ov.hint = T("Hint", opt, 200, 0, 198, 16, "нет", 7, BakPalette.Secondary, TextAlignmentOptions.MidlineRight, null, 6);
            Solid(R("HeaderLine", opt, 1, 16, 406, 1), BakPalette.Line);

            ov.rows = new OptionRowView[3];
            for (int i = 0; i < 3; i++)
            {
                var row = R("Option" + (i + 1), opt, 2, 19 + i * 18, 404, 18);
                var b = Btn(row, "buttons/option_normal", "buttons/option_hover", "buttons/option_hover", "buttons/option_normal", Image.Type.Sliced);
                var rv = row.gameObject.AddComponent<OptionRowView>();
                rv.button = b;
                rv.number = T("Number", row, 8, 0, 12, 18, (i + 1).ToString(), 8, BakPalette.Main, TextAlignmentOptions.MidlineLeft);
                rv.label = T("Text", row, 24, 0, 372, 18, "Вариант ответа", 8, BakPalette.Main, TextAlignmentOptions.MidlineLeft);
                PBT(b, BakPalette.Main, BakPalette.Bg, BakPalette.Bg, BakPalette.Secondary, rv.number, rv.label);
                ov.rows[i] = rv;
                row.gameObject.SetActive(false);
            }

            ov.emptyMessage = T("EmptyMessage", opt, 10, 17, 388, 59, view.noOptionsMessage, 7, BakPalette.Secondary, TextAlignmentOptions.Center);
            ov.emptyMessage.textWrappingMode = TextWrappingModes.Normal;
            view.options = ov;
        }

        static void BuildDispatch(RectTransform right, CallScreenView view)
        {
            var d = R("Dispatch", right, 0, 268, 408, 48);
            var dv = d.gameObject.AddComponent<DispatchPanelView>();
            dv.title = T("Title", d, 0, 0, 200, 12, "ПАНЕЛЬ ДИСПЕТЧЕРИЗАЦИИ", 7, BakPalette.Secondary, TextAlignmentOptions.MidlineLeft, null, 8);
            dv.status = T("Status", d, 200, 0, 208, 12, dv.lockedText, 7, BakPalette.Secondary, TextAlignmentOptions.MidlineRight, null, 6);
            Solid(R("Line", d, 0, 13, 408, 1), BakPalette.Line);
            dv.pressedSprite = S("buttons/btn_pressed");
            dv.militia = DispatchBtn(d, "Militia", 0, "МИЛИЦИЯ", "buttons/btn_normal");
            dv.ambulance = DispatchBtn(d, "Ambulance", 1, "СКОРАЯ", "buttons/btn_normal");
            dv.liquidators = DispatchBtn(d, "Liquidators", 2, "ЛИКВИДАТОРЫ", "buttons/btn_normal");
            dv.ignore = DispatchBtn(d, "Ignore", 3, "ИГНОРИРОВАТЬ", "buttons/btn_ignore_normal");
            view.dispatch = dv;
        }

        static Button DispatchBtn(RectTransform parent, string id, int index, string label, string normalSprite)
        {
            var rt = R("Btn" + id, parent, index * 104, 16, 96, 32);
            var b = Btn(rt, normalSprite, "buttons/btn_hover", "buttons/btn_pressed", "buttons/btn_disabled", Image.Type.Tiled);
            var t = T("Label", rt, 0, 0, 96, 32, label, 8, BakPalette.Main, TextAlignmentOptions.Center, null, 8);
            PBT(b, BakPalette.Main, BakPalette.Bg, BakPalette.Bg, BakPalette.Frame, t);
            b.interactable = false;
            return b;
        }

        // ───────────── Окно «Памятка» поверх левой зоны ─────────────
        static void BuildMemoWindow(RectTransform screen, CallScreenView view)
        {
            var w = R("MemoWindow", screen, 12, 32, 200, 316);
            // Подложка цвета фона: закрывает левую зону целиком (в т.ч. углы под пиксельной тенью) и ловит клики
            var backdrop = Solid(R("Backdrop", w, 0, 0, 200, 316), BakPalette.Bg);
            backdrop.raycastTarget = true;
            Img(R("Frame", w, 0, 0, 200, 316), "frames/popup_frame", Image.Type.Sliced);
            var mv = w.gameObject.AddComponent<MemoWindowView>();
            mv.data = EnsureMemoData();

            T("Title", w, 8, 0, 160, 16, "ПАМЯТКА СОТРУДНИКА · ДСП", 7, BakPalette.Main, TextAlignmentOptions.MidlineLeft, null, 8);
            var close = R("Close", w, 180, 1, 14, 14);
            mv.closeButton = Btn(close, "icons/btn_close_normal", "icons/btn_close_hover", "icons/btn_close_hover", "icons/btn_close_normal", Image.Type.Simple);
            Solid(R("HeaderLine", w, 2, 16, 193, 1), BakPalette.Frame);

            var tabs = R("Tabs", w, 2, 17, 193, 16);
            var th = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
            th.childControlWidth = th.childControlHeight = true;
            th.childForceExpandWidth = th.childForceExpandHeight = true;
            mv.tabs = new Button[5];
            string[] names = { "ПОРЯДОК", "ЛОЖНЫЕ", "РАБОЧИЙ", "СТОРОЖ", "ДУБЛЁР" };
            for (int i = 0; i < 5; i++)
            {
                var tab = R("Tab" + (i + 1), tabs, 0, 0, 38, 16);
                var b = Btn(tab, "buttons/tab_normal", "buttons/tab_hover", "buttons/tab_hover", "buttons/tab_normal", Image.Type.Sliced);
                var lbl = T("Label", tab, 0, 0, 38, 16, names[i], 6, BakPalette.Secondary, TextAlignmentOptions.Center);
                var lrt = lbl.rectTransform;
                lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = lrt.offsetMax = Vector2.zero;
                var pbt = PBT(b, BakPalette.Secondary, BakPalette.Main, BakPalette.Main, BakPalette.Line, lbl);
                pbt.active = BakPalette.Bg;
                pbt.activeSprite = S("buttons/tab_active");
                mv.tabs[i] = b;
            }
            Solid(R("TabsLine", w, 2, 33, 193, 1), BakPalette.Frame);

            // Страница: заголовок, подзаголовок, 2 блока (данные из MemoData)
            var page = R("Page", w, 10, 38, 177, 218);
            var pv = page.gameObject.AddComponent<VerticalLayoutGroup>();
            pv.spacing = 3;
            pv.childControlWidth = pv.childControlHeight = true;
            pv.childForceExpandWidth = true;
            pv.childForceExpandHeight = false;
            mv.pageTitle = Wrap(T("PageTitle", page, 0, 0, 177, 12, "ОБРАЗЕЦ", 9, BakPalette.Main, TextAlignmentOptions.TopLeft));
            mv.pageSubtitle = Wrap(T("PageSubtitle", page, 0, 0, 177, 10, "разг.:", 7, BakPalette.Secondary, TextAlignmentOptions.TopLeft));
            mv.blockHeadings = new TMP_Text[2];
            mv.blockItems = new TMP_Text[2];
            for (int i = 0; i < 2; i++)
            {
                var block = R("Block" + (i + 1), page, 0, 0, 177, 40);
                var bv = block.gameObject.AddComponent<VerticalLayoutGroup>();
                bv.spacing = 2;
                bv.padding = new RectOffset(0, 0, 4, 0);
                bv.childControlWidth = bv.childControlHeight = true;
                bv.childForceExpandWidth = true;
                bv.childForceExpandHeight = false;
                mv.blockHeadings[i] = T("Heading", block, 0, 0, 177, 10, "ПРИЗНАКИ", 7, BakPalette.Main, TextAlignmentOptions.TopLeft, null, 8);
                var line = Solid(R("Line", block, 0, 0, 177, 1), BakPalette.Line);
                var lle = line.gameObject.AddComponent<LayoutElement>();
                lle.minHeight = lle.preferredHeight = 1;
                mv.blockItems[i] = Wrap(T("Items", block, 0, 0, 177, 10, "— пункт", 7, BakPalette.Main, TextAlignmentOptions.TopLeft));
                mv.blockItems[i].lineSpacing = 10;
            }

            var dec = R("DecisionFrame", w, 10, 262, 177, 20);
            Img(dec, "buttons/btn_normal", Image.Type.Tiled);
            mv.decision = T("Decision", dec, 4, 0, 169, 20, "РЕШЕНИЕ", 7, BakPalette.Main, TextAlignmentOptions.Center, null, 6);
            mv.decisionFrame = dec.gameObject;

            Solid(R("FooterLine", w, 2, 288, 193, 1), BakPalette.Frame);
            mv.prevButton = Btn(R("Prev", w, 8, 293, 22, 16), "icons/btn_arrow_left_normal", "icons/btn_arrow_left_hover", "icons/btn_arrow_left_pressed", "icons/btn_arrow_left_normal", Image.Type.Simple);
            mv.pageLabel = T("PageLabel", w, 40, 289, 117, 24, "стр. 1 / 5", 7, BakPalette.Secondary, TextAlignmentOptions.Center, null, 6);
            mv.nextButton = Btn(R("Next", w, 167, 293, 22, 16), "icons/btn_arrow_right_normal", "icons/btn_arrow_right_hover", "icons/btn_arrow_right_pressed", "icons/btn_arrow_right_normal", Image.Type.Simple);

            view.memoWindow = mv;
        }

        // ───────────── Подсказки «?» (поверх всего) ─────────────
        static void BuildHelpCards(RectTransform screen, CallScreenView view)
        {
            var overlays = R("Overlays", screen, 0, 0, 640, 360);
            view.patienceHelp.card = Card(overlays, "PatienceCard", 436, 48, 64, "ТЕРПЕНИЕ АБОНЕНТА",
                "Точные и вежливые вопросы удерживают шкалу. Лишние, грубые и затянутые — снижают. На 0% абонент бросает трубку.");
            view.reprimandHelp.card = Card(overlays, "ReprimandCard", 436, 24, 70, "ВЫГОВОРЫ · 3 ИЗ 3 = УВОЛЬНЕНИЕ",
                "+1 — не та служба или ложный выезд ликвидаторов\n+2 — реальная угроза проигнорирована\n+3 — милиция или скорая отправлены на аномалию");
        }

        static GameObject Card(RectTransform parent, string name, float x, float y, float h, string title, string body)
        {
            var c = R(name, parent, x, y, 192, h);
            Img(c, "frames/popup_frame", Image.Type.Sliced, true);
            T("Title", c, 8, 5, 176, 10, title, 7, BakPalette.Main, TextAlignmentOptions.MidlineLeft, null, 6);
            var b = T("Body", c, 8, 18, 176, h - 24, body, 7, BakPalette.Main, TextAlignmentOptions.TopLeft);
            b.textWrappingMode = TextWrappingModes.Normal;
            b.lineSpacing = 8;
            c.gameObject.SetActive(false);
            return c.gameObject;
        }

        static HelpButton HelpBtn(string name, RectTransform parent, float x, float y)
        {
            var rt = R(name, parent, x, y, 11, 11);
            var b = Btn(rt, "icons/btn_help_normal", "icons/btn_help_hover", "icons/btn_help_active", "icons/btn_help_disabled", Image.Type.Simple);
            var h = rt.gameObject.AddComponent<HelpButton>();
            h.button = b;
            h.activeSprite = S("icons/btn_help_active");
            return h;
        }

        // ───────────── Данные памятки (черновик; правится в инспекторе) ─────────────
        static MemoData EnsureMemoData()
        {
            var data = AssetDatabase.LoadAssetAtPath<MemoData>(MemoDataPath);
            if (data != null) return data;
            EnsureFolder("Assets/UI/Data");
            data = ScriptableObject.CreateInstance<MemoData>();
            data.pages = new[]
            {
                Page("ПОРЯДОК", "ПОРЯДОК ПРИЁМА ВЫЗОВА", "общие положения",
                    Block("ЭТАПЫ", "Выяснить: что случилось, где, кто рядом.", "Сверить слова абонента с признаками образцов.", "Уточнить. Вежливо и по делу.", "Направить одну службу. Решение окончательное."),
                    Block("ПОМНИТЬ", "Выезд ликвидаторов — 40 000 руб.", "Терпение абонента не бесконечно."),
                    "СОМНЕНИЯ — В ПОЛЬЗУ РЕГЛАМЕНТА"),
                Page("ЛОЖНЫЕ", "ЛОЖНЫЕ ВЫЗОВЫ", "розыгрыши, паника, бытовое",
                    Block("ПРИЗНАКИ", "Путается в показаниях, меняет детали.", "«Пришельцы», «монстр под кроватью».", "Смех и голоса на фоне, ответы невпопад."),
                    Block("УТОЧНИТЬ", "Повторите адрес полностью.", "Кто ещё это видел?"),
                    "ИГНОРИРОВАТЬ · ПРИ ПСИХОЗЕ — СКОРАЯ"),
                Page("РАБОЧИЙ", "ОБРАЗЕЦ «РАБОЧИЙ»", "разг.: «мутация»",
                    Block("ПРИЗНАКИ", "Шум разборки мебели и полов без перерыва.", "Рычание, «влажные» звуки.", "Сырость, ржавый налёт («плесень») у труб и стояков.", "Жильцы не выходят и не отзываются по имени."),
                    Block("УТОЧНИТЬ", "Чем пахнет из-под двери?", "Есть ли рыжий налёт в ванной, на трубах?", "Когда соседей видели последний раз?"),
                    "ЛИКВИДАТОРЫ · МИЛИЦИЮ НЕ НАПРАВЛЯТЬ"),
                Page("СТОРОЖ", "ОБРАЗЕЦ «СТОРОЖ»", "разг.: «имитатор»",
                    Block("ПРИЗНАКИ", "Домашнее животное ведёт себя «не так».", "Рычание сменяется тишиной и скулежом.", "Подражает голосу или словам человека."),
                    Block("УТОЧНИТЬ", "Где вы относительно животного?", "Произносит ли оно слова?"),
                    "ЛИКВИДАТОРЫ · НЕМЕДЛЕННО"),
                Page("ДУБЛЁР", "ОБРАЗЕЦ «ДУБЛЁР»", "разг.: «доппельгангер»",
                    Block("ПРИЗНАКИ", "Близкий ведёт себя безупречно спокойно.", "Готовые объяснения любым несостыковкам.", "Человек «находится» в двух местах сразу."),
                    Block("УТОЧНИТЬ", "Где его телефон? Кто прислал сообщение?", "Можете запереться и не открывать?"),
                    "ЛИКВИДАТОРЫ · АБОНЕНТУ НЕ ВЫХОДИТЬ"),
            };
            AssetDatabase.CreateAsset(data, MemoDataPath);
            AssetDatabase.SaveAssets();
            return data;
        }

        static MemoData.Page Page(string tab, string title, string subtitle, MemoData.Block b1, MemoData.Block b2, string decision)
            => new MemoData.Page { tab = tab, title = title, subtitle = subtitle, blocks = new[] { b1, b2 }, decision = decision };

        static MemoData.Block Block(string heading, params string[] items)
            => new MemoData.Block { heading = heading, items = items };

        // ───────────── Помощники ─────────────
        /// Прямоугольник в координатах родителя: x, y от левого верхнего угла (y вниз), как в макете.
        internal static RectTransform R(string name, Transform parent, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        internal static Sprite S(string name)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + name + ".png");
            if (s == null) Debug.LogWarning("[БАК] Нет спрайта " + name);
            return s;
        }

        internal static Image Img(RectTransform rt, string sprite, Image.Type type, bool raycast = false)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = S(sprite);
            img.type = type;
            img.pixelsPerUnitMultiplier = 1;
            img.raycastTarget = raycast;
            return img;
        }

        internal static Image Solid(RectTransform rt, Color c)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.color = c;
            img.raycastTarget = false;
            return img;
        }

        internal static TextMeshProUGUI T(string name, Transform parent, float x, float y, float w, float h, string text, float size,
            Color color, TextAlignmentOptions align, TMP_FontAsset font = null, float spacing = 0)
        {
            var rt = R(name, parent, x, y, w, h);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font != null ? font : _mono;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.characterSpacing = spacing;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Overflow;
            t.raycastTarget = false;
            t.richText = true;
            t.text = text;
            return t;
        }

        internal static TextMeshProUGUI Wrap(TextMeshProUGUI t)
        {
            t.textWrappingMode = TextWrappingModes.Normal;
            return t;
        }

        internal static Button Btn(RectTransform rt, string normal, string hover, string pressed, string disabled, Image.Type type)
        {
            var img = Img(rt, normal, type, true);
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            b.transition = Selectable.Transition.SpriteSwap;
            b.spriteState = new SpriteState
            {
                highlightedSprite = S(hover),
                pressedSprite = S(pressed),
                selectedSprite = null,
                disabledSprite = S(disabled)
            };
            b.navigation = new Navigation { mode = Navigation.Mode.None };
            return b;
        }

        internal static PixelButtonText PBT(Button b, Color normal, Color hover, Color pressed, Color disabled, params Graphic[] targets)
        {
            var p = b.gameObject.AddComponent<PixelButtonText>();
            p.normal = normal;
            p.hover = hover;
            p.pressed = pressed;
            p.disabled = disabled;
            p.targets = targets;
            p.Apply();
            return p;
        }

        internal static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}
