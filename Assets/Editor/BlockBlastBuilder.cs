using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class BlockBlastBuilder
{
    const string SpriteDir = "Assets/Sprites/";
    const string ScenePath = "Assets/Scenes/BlockBlast.unity";

    static readonly string[] SpriteNames =
    {
        "background", "board-bg", "board", "cell-highlight", "cell-normal",
        "button-replay-active", "button-replay-normal", "icon-best-score", "score-background"
    };

    [MenuItem("Tools/Build Block Blast")]
    public static void Build()
    {
        if (!EnsureTMP()) return;

        var missing = SpriteNames.Where(n => !File.Exists(SpriteDir + n + ".png")).ToArray();
        if (missing.Length > 0)
        {
            EditorUtility.DisplayDialog("Block Blast", "Thiếu sprite trong Assets/Sprites: " + string.Join(", ", missing), "OK");
            return;
        }
        foreach (var n in SpriteNames) ImportAsSprite(SpriteDir + n + ".png");
        AssetDatabase.Refresh();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGo.tag = "MainCamera";
        var cam = camGo.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;

        var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        var canvasRt = (RectTransform)canvasGo.transform;

        var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        ObjectFactory.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>(es);
#else
        es.AddComponent<StandaloneInputModule>();
#endif

        var bg = NewImage("Background", canvasRt, S("background"), Vector2.zero, Vector2.zero);
        Stretch(bg.rectTransform, 0f);

        var scorePanel = NewImage("ScorePanel", canvasRt, S("score-background"), new Vector2(-250, 760), new Vector2(400, 160));
        var scoreText = NewText(scorePanel.transform);

        var bestPanel = NewImage("BestPanel", canvasRt, S("score-background"), new Vector2(250, 760), new Vector2(400, 160));
        NewImage("BestIcon", bestPanel.transform, S("icon-best-score"), new Vector2(0, 85), new Vector2(300, 75));
        var bestText = NewText(bestPanel.transform);

        var frame = NewImage("Board", canvasRt, S("board"), new Vector2(0, 150), new Vector2(960, 960));
        var grid = NewRT("Grid", frame.transform, Vector2.zero, new Vector2(840, 840));
        float cs = GameConfig.CellSize;
        float half = cs * GameConfig.Size / 2f;
        var fills = new Image[GameConfig.Size * GameConfig.Size];
        for (int r = 0; r < GameConfig.Size; r++)
        {
            for (int c = 0; c < GameConfig.Size; c++)
            {
                var pos = new Vector2(-half + cs * (c + 0.5f), half - cs * (r + 0.5f));
                var cell = NewImage("Cell_" + r + "_" + c, grid, S("board-bg"), pos, new Vector2(cs, cs));
                var fill = NewImage("Fill", cell.transform, null, Vector2.zero, Vector2.zero);
                Stretch(fill.rectTransform, 0f);
                fill.enabled = false;
                fills[r * GameConfig.Size + c] = fill;
            }
        }

        var slots = new RectTransform[GameConfig.PieceCount];
        float[] xs = { -330f, 0f, 330f };
        for (int i = 0; i < slots.Length; i++)
            slots[i] = NewRT("Slot" + i, canvasRt, new Vector2(xs[i], -560), new Vector2(320, 320));

        var replay = NewImage("ReplayButton", canvasRt, S("button-replay-normal"), new Vector2(0, -840), new Vector2(180, 180));
        replay.raycastTarget = true;
        var btn = replay.gameObject.AddComponent<Button>();
        btn.targetGraphic = replay;
        btn.transition = Selectable.Transition.SpriteSwap;
        var ss = new SpriteState();
        ss.pressedSprite = S("button-replay-active");
        btn.spriteState = ss;
        replay.gameObject.SetActive(false);

        var dragLayer = NewRT("DragLayer", canvasRt, Vector2.zero, Vector2.zero);
        Stretch(dragLayer, 0f);

        var mgr = new GameObject("GameManager");
        var board = mgr.AddComponent<BoardController>();
        board.GridRect = grid;
        board.Fills = fills;
        board.NormalSprite = S("cell-normal");
        board.HighlightSprite = S("cell-highlight");

        var spawner = mgr.AddComponent<BlockSpawner>();
        spawner.Slots = slots;
        spawner.DragLayer = dragLayer;
        spawner.CanvasRect = canvasRt;
        spawner.CellSprite = S("cell-normal");

        var score = mgr.AddComponent<ScoreManager>();
        score.ScoreText = scoreText;
        score.BestText = bestText;

        var ui = mgr.AddComponent<UIManager>();
        ui.ReplayButton = btn;

        var gm = mgr.AddComponent<GameManager>();
        gm.Board = board;
        gm.Spawner = spawner;
        gm.Score = score;
        gm.UI = ui;
        spawner.Game = gm;

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Block Blast", "Xong. Bấm Play.", "OK");
    }

    static bool EnsureTMP()
    {
        if (TMP_Settings.instance != null && TMP_Settings.defaultFontAsset != null) return true;
        EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
        EditorUtility.DisplayDialog("Block Blast", "Bấm Import TMP Essentials, xong chạy lại Tools > Build Block Blast.", "OK");
        return false;
    }

    static void ImportAsSprite(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return;
        if (imp.textureType == TextureImporterType.Sprite && imp.spriteImportMode == SpriteImportMode.Single && !imp.mipmapEnabled) return;
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.SaveAndReimport();
    }

    static Sprite S(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + name + ".png");
    }

    static RectTransform NewRT(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static Image NewImage(string name, Transform parent, Sprite sprite, Vector2 pos, Vector2 size)
    {
        var rt = NewRT(name, parent, pos, size);
        var im = rt.gameObject.AddComponent<Image>();
        im.sprite = sprite;
        im.raycastTarget = false;
        return im;
    }

    static void Stretch(RectTransform rt, float inset)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
    }

    static TextMeshProUGUI NewText(Transform parent)
    {
        var rt = NewRT("Text", parent, Vector2.zero, Vector2.zero);
        Stretch(rt, 20f);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = TMP_Settings.defaultFontAsset;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
        t.enableAutoSizing = true;
        t.fontSizeMin = 30;
        t.fontSizeMax = 72;
        t.color = Color.white;
        t.raycastTarget = false;
        t.text = "0";
        return t;
    }
}
