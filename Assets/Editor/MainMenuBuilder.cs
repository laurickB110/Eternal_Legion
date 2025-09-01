using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.LowLevel;   // GlyphRenderMode

public static class MainMenuBuilder
{
    private const string MenuScenePath       = "Assets/Scenes/MainMenu.unity";
    private const string VersusScenePath     = "Assets/Scenes/Main scene - Versus.unity";
    private const string BackgroundPath      = "Assets/Art/Backgrounds/Menu.png";
    private const string CinzelBoldTtfPath   = "Assets/Fonts/Cinzel/static/Cinzel-Bold.ttf";
    private const string CinzelBoldAssetDir  = "Assets/Fonts/Cinzel";
    private const string CinzelBoldAssetPath = CinzelBoldAssetDir + "/Cinzel-Bold SDF.asset";

    [MenuItem("Tools/Eternal Legion/Create/Refresh Main Menu")]
    public static void CreateOrRefresh()
    {
        // Ensure folders exist
        Directory.CreateDirectory("Assets/Scenes");

        // Fresh empty scene
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- Ensure a Main Camera exists (avoid 'No cameras rendering') ---
        var cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cam = camGO.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.07f, 0.07f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0, 0, -10f);
            cam.tag = "MainCamera";
        }

        // Canvas
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas   = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; // simple & fiable
        // Recommended for TMP effects visibility
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
        // (Option) Screen Space Camera :
        // canvas.renderMode = RenderMode.ScreenSpaceCamera;
        // canvas.worldCamera = cam;
        // canvas.planeDistance = 1f;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Background
        var backgroundGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundGO.transform.SetParent(canvasGO.transform, false);
        var bgRect = backgroundGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImage = backgroundGO.GetComponent<Image>();
        bgImage.raycastTarget = false; // never block buttons

        string backgroundUsed = "placeholder gray";
        var bgSprite = LoadSpriteEnsureImport(BackgroundPath);
        if (bgSprite != null)
        {
            bgImage.sprite = bgSprite;
            bgImage.color = Color.white;
            bgImage.preserveAspect = true;
            backgroundUsed = BackgroundPath;
        }
        else
        {
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            Debug.LogWarning($"MainMenuBuilder: Background not found, using placeholder gray: {BackgroundPath}");
        }

        // No explicit title text in scene: the background already includes it

        // TMP font asset (Cinzel-Bold) -> create dynamic if missing, repair if broken
        bool cinzelCreated  = false;
        bool cinzelRepaired = false;
        var  fontAsset      = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CinzelBoldAssetPath);

        bool IsBroken(TMP_FontAsset fa)
        {
            if (fa == null) return true;
            var tex = fa.atlasTextures;
            return tex == null || tex.Length == 0 || tex.Any(t => t == null);
        }

        TMP_FontAsset BuildDynamicFromTTF(Font ttfFont)
        {
            var created = TMP_FontAsset.CreateFontAsset(
                ttfFont,
                90,                    // point size
                9,                     // padding
                GlyphRenderMode.SDFAA, // SDF (try SDFAA_HINTED if needed)
                1024, 1024,            // atlas size
                AtlasPopulationMode.Dynamic,
                true                   // multi-atlas
            );

            // Seed atlas with basic characters to force texture creation
            created.TryAddCharacters(
                "Eternal LegionJOUEROptionsQuitterjoueroptionsquitteréèàâêîôûùç!?-.,:;()[]{}",
                out _
            );

            if (created.material == null)
            {
                var shader = Shader.Find("TextMeshPro/Distance Field");
                created.material = new Material(shader);
            }
            created.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            created.isMultiAtlasTexturesEnabled = true;
            return created;
        }

        Font ttf = AssetDatabase.LoadAssetAtPath<Font>(CinzelBoldTtfPath);
        if (fontAsset == null)
        {
            if (ttf != null)
            {
                try
                {
                    var created = BuildDynamicFromTTF(ttf);
                    Directory.CreateDirectory(CinzelBoldAssetDir);
                    AssetDatabase.CreateAsset(created, CinzelBoldAssetPath);
                    AssetDatabase.SaveAssets();
                    fontAsset = created;
                    cinzelCreated = true;
                    Debug.Log($"MainMenuBuilder: Created TMP Font Asset (Dynamic) at {CinzelBoldAssetPath}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"MainMenuBuilder: Failed to create TMP Font Asset from {CinzelBoldTtfPath}. Using default TMP font. Error: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"MainMenuBuilder: Cinzel-Bold.ttf not found at {CinzelBoldTtfPath}. Using default TMP font.");
            }
        }
        else if (IsBroken(fontAsset))
        {
            if (ttf != null)
            {
                try
                {
                    var rebuilt = BuildDynamicFromTTF(ttf);
                    EditorUtility.CopySerialized(rebuilt, fontAsset); // keep GUID/path, fix data
                    fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                    fontAsset.isMultiAtlasTexturesEnabled = true;
                    EditorUtility.SetDirty(fontAsset);
                    AssetDatabase.SaveAssets();
                    cinzelRepaired = true;
                    Debug.Log($"MainMenuBuilder: Repaired TMP Font Asset atlas: {CinzelBoldAssetPath}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"MainMenuBuilder: Failed to repair TMP Font Asset {CinzelBoldAssetPath}. Using default TMP font. Error: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"MainMenuBuilder: Cinzel-Bold.ttf not found; cannot repair {CinzelBoldAssetPath}. Using default TMP font.");
            }
        }

        // fontAsset is used for buttons below if available

        // Buttons container
        var buttonsGO = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        buttonsGO.transform.SetParent(canvasGO.transform, false);
        var buttonsRect = buttonsGO.GetComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.5f, 0.4f);
        buttonsRect.anchorMax = new Vector2(0.5f, 0.4f);
        buttonsRect.anchoredPosition = Vector2.zero;
        buttonsRect.sizeDelta = new Vector2(600, 300);
        var vlg = buttonsGO.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 80f;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        Button CreateButton(string name, string label, bool interactable = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(buttonsGO.transform, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            var btn = go.GetComponent<Button>();
            btn.interactable = interactable;
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = 500;
            le.preferredHeight = 80;

            var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(go.transform, false);
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 36;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Truncate;
            if (fontAsset != null)
            {
                tmp.font = fontAsset;
                if (fontAsset.material != null)
                {
                    // Use shared font material; ensure it is visible
                    var mat = fontAsset.material;
                    var faceId = TMPro.ShaderUtilities.ID_FaceColor;
                    if (mat.HasProperty(faceId))
                    {
                        var c = mat.GetColor(faceId);
                        c.r = 1f; c.g = 1f; c.b = 1f; c.a = 1f;
                        mat.SetColor(faceId, c);
                    }
                    var outlineWidthId = TMPro.ShaderUtilities.ID_OutlineWidth;
                    if (mat.HasProperty(outlineWidthId)) mat.SetFloat(outlineWidthId, 0f);
                    tmp.fontSharedMaterial = mat;
                }
            }
            return btn;
        }

        var playBtn    = CreateButton("Button - Jouer",   "Jouer");
        var optionsBtn = CreateButton("Button - Options", "Options", interactable:false);
        var quitBtn    = CreateButton("Button - Quitter", "Quitter");

        // Controller host
        var controllerGO = new GameObject("MenuController", typeof(EternalLegion.UI.MenuController), typeof(EternalLegion.Utilities.EnsureCamera));
        controllerGO.transform.SetParent(canvasGO.transform, false);
        var controller = controllerGO.GetComponent<EternalLegion.UI.MenuController>();

        // Helper to clear persistent listeners
        void ClearPersistent(Button b)
        {
            var e = b.onClick;
            for (int i = e.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(e, i);
        }

        // Wire up (idempotent)
        ClearPersistent(playBtn);
        ClearPersistent(optionsBtn);
        ClearPersistent(quitBtn);
        UnityEventTools.AddPersistentListener(playBtn.onClick, controller.OnPlay);
        UnityEventTools.AddPersistentListener(optionsBtn.onClick, controller.OnOptions);
        UnityEventTools.AddPersistentListener(quitBtn.onClick, controller.OnQuit);
        EditorUtility.SetDirty(playBtn);
        EditorUtility.SetDirty(optionsBtn);
        EditorUtility.SetDirty(quitBtn);

        // EventSystem
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // Save the scene
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), MenuScenePath);
        AssetDatabase.Refresh();

        // Build settings
        UpdateBuildSettings(MenuScenePath, VersusScenePath);

        Debug.Log($"MainMenuBuilder: Done. Background: {backgroundUsed}. TMP Cinzel-Bold created: {cinzelCreated}, repaired: {cinzelRepaired}.");
    }

    private static void UpdateBuildSettings(string mainMenuPath, string versusPath)
    {
        var existing = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var result   = new List<EditorBuildSettingsScene>();

        // Ensure MainMenu at index 0
        result.Add(new EditorBuildSettingsScene(mainMenuPath, true));

        // Keep existing (no duplicates)
        foreach (var s in existing)
            if (!string.Equals(s.path, mainMenuPath, StringComparison.OrdinalIgnoreCase))
                result.Add(s);

        // Ensure Versus listed
        if (File.Exists(versusPath) && !result.Any(s => string.Equals(s.path, versusPath, StringComparison.OrdinalIgnoreCase)))
            result.Add(new EditorBuildSettingsScene(versusPath, true));

        EditorBuildSettings.scenes = result.ToArray();
    }

    private static Sprite LoadSpriteEnsureImport(string assetPath)
    {
        // Quick exit if sprite already imported
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null) return sprite;

        // If the asset exists but isn't a sprite, switch importer and reimport
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return null;

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            changed = true;
        }
        if (changed)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }
}
