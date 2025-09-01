using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.TextCore.LowLevel;

// Ensures TMP font assets used by the menu are always valid.
// Repairs missing atlas textures automatically on editor load/import.
public static class TMPFontSelfHeal
{
    private const string CinzelBoldTtfPath   = "Assets/Fonts/Cinzel/static/Cinzel-Bold.ttf";
    private const string CinzelBoldAssetPath = "Assets/Fonts/Cinzel/Cinzel-Bold SDF.asset";

    [InitializeOnLoadMethod]
    private static void HealOnLoad()
    {
        try
        {
            HealCinzelBold();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"TMPFontSelfHeal: Unexpected error while healing fonts: {e.Message}");
        }
    }

    private static void HealCinzelBold()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CinzelBoldAssetPath);
        if (asset == null) return; // nothing to heal

        bool IsBroken(TMP_FontAsset fa)
        {
            var tex = fa.atlasTextures;
            return tex == null || tex.Length == 0 || tex.Any(t => t == null);
        }

        // Ensure name, sub-assets and material are valid
        EnsureValidName(asset, CinzelBoldAssetPath);
        PersistSubAssets(asset);
        EnsureMaterialVisible(asset);
        if (!IsBroken(asset)) return;

        var ttf = AssetDatabase.LoadAssetAtPath<Font>(CinzelBoldTtfPath);
        if (ttf == null)
        {
            Debug.LogWarning($"TMPFontSelfHeal: {CinzelBoldAssetPath} has no atlas textures and TTF not found at {CinzelBoldTtfPath}. Using TMP default until provided.");
            return;
        }

        TMP_FontAsset Build(Font f)
        {
            var created = TMP_FontAsset.CreateFontAsset(f, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            created.TryAddCharacters("Eternal LegionJOUEROptionsQuitterjoueroptionsquitteréèàâêîôûùç!?-.,:;()[]{}", out _);
            if (created.material == null)
            {
                var shader = Shader.Find("TextMeshPro/Distance Field");
                created.material = new Material(shader);
            }
            created.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            created.isMultiAtlasTexturesEnabled = true;
            return created;
        }

        try
        {
            var temp = Build(ttf);
            EditorUtility.CopySerialized(temp, asset); // keep GUID/path, fix data
            EnsureValidName(asset, CinzelBoldAssetPath);
            PersistSubAssets(asset);
            EnsureMaterialVisible(asset);
            asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            asset.isMultiAtlasTexturesEnabled = true;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Debug.Log($"TMPFontSelfHeal: Repaired atlas textures for {CinzelBoldAssetPath}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"TMPFontSelfHeal: Failed to repair {CinzelBoldAssetPath}. Error: {e.Message}");
        }
    }

    private static void PersistSubAssets(TMP_FontAsset asset)
    {
        bool dirty = false;
        // Ensure atlas textures are saved as sub-assets
        if (asset.atlasTextures != null)
        {
            foreach (var tex in asset.atlasTextures)
            {
                if (tex == null) continue;
                var path = AssetDatabase.GetAssetPath(tex);
                if (string.IsNullOrEmpty(path))
                {
                    AssetDatabase.AddObjectToAsset(tex, asset);
                    dirty = true;
                }
            }
        }
        // Ensure material is saved as sub-asset
        if (asset.material != null)
        {
            var mpath = AssetDatabase.GetAssetPath(asset.material);
            if (string.IsNullOrEmpty(mpath))
            {
                AssetDatabase.AddObjectToAsset(asset.material, asset);
                dirty = true;
            }
        }
        if (dirty)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
    }

    private static void EnsureMaterialVisible(TMP_FontAsset asset)
    {
        if (asset == null) return;
        var mat = asset.material;
        if (mat == null)
        {
            var shader = Shader.Find("TextMeshPro/Distance Field");
            mat = new Material(shader);
            asset.material = mat;
        }
        // Force face color to opaque white and no outline
        var faceId = ShaderUtilities.ID_FaceColor;
        if (mat.HasProperty(faceId))
        {
            var c = mat.GetColor(faceId);
            c.a = 1f; c.r = 1f; c.g = 1f; c.b = 1f;
            mat.SetColor(faceId, c);
        }
        var outlineColorId = ShaderUtilities.ID_OutlineColor;
        if (mat.HasProperty(outlineColorId))
        {
            var oc = mat.GetColor(outlineColorId);
            oc.a = 0f; mat.SetColor(outlineColorId, oc);
        }
        var outlineWidthId = ShaderUtilities.ID_OutlineWidth;
        if (mat.HasProperty(outlineWidthId))
        {
            mat.SetFloat(outlineWidthId, 0f);
        }
        // Persist as sub-asset if needed
        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(mat)))
        {
            AssetDatabase.AddObjectToAsset(mat, asset);
        }
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
    }

    private static void EnsureValidName(TMP_FontAsset asset, string path)
    {
        if (asset == null) return;
        var expected = System.IO.Path.GetFileNameWithoutExtension(path);
        if (string.IsNullOrEmpty(asset.name) || asset.name != expected)
        {
            asset.name = expected;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
    }
}
