using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;

public static class MainMenuFixer
{
    private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string CinzelBoldAssetPath = "Assets/Fonts/Cinzel/Cinzel-Bold SDF.asset";

    [MenuItem("Tools/Eternal Legion/Fix Current Menu UI")]
    public static void FixCurrent()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.isLoaded || scene.path != MenuScenePath)
        {
            if (EditorUtility.DisplayDialog("Fix Menu UI", "Open MainMenu.unity to fix?", "Open", "Cancel"))
            {
                EditorSceneManager.OpenScene(MenuScenePath);
                scene = EditorSceneManager.GetActiveScene();
            }
            else return;
        }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CinzelBoldAssetPath);
        var labels = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                           .Where(t => t.gameObject.name == "Label");

        foreach (var tmp in labels)
        {
            if (font != null) tmp.font = font;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Truncate;
            if (font != null && font.material != null)
            {
                var mat = font.material;
                var faceId = TMPro.ShaderUtilities.ID_FaceColor;
                if (mat.HasProperty(faceId))
                {
                    var c = mat.GetColor(faceId); c.r = c.g = c.b = c.a = 1f; mat.SetColor(faceId, c);
                }
                var outlineWidthId = TMPro.ShaderUtilities.ID_OutlineWidth;
                if (mat.HasProperty(outlineWidthId)) mat.SetFloat(outlineWidthId, 0f);
                tmp.fontSharedMaterial = mat;
            }
            EditorUtility.SetDirty(tmp);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"MainMenuFixer: Fixed {labels.Count()} labels.\nFont assigned: {(font != null)}");
    }
}
