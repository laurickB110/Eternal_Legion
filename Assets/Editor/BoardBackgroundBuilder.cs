using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EternalLegion.Utilities;

public static class BoardBackgroundBuilder
{
    private const string BackgroundTexPath = "Assets/Art/Backgrounds/Board.png";
    private const string BackgroundMatPath = "Assets/Art/Backgrounds/Board.mat";

    [MenuItem("Tools/Eternal Legion/Setup Board Background")]
    public static void Setup()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("BoardBackgroundBuilder: No Main Camera found.");
            return;
        }

        // Load texture
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundTexPath);
        if (tex == null)
        {
            Debug.LogWarning($"BoardBackgroundBuilder: Background not found at {BackgroundTexPath}");
            return;
        }

        // Material
        var mat = AssetDatabase.LoadAssetAtPath<Material>(BackgroundMatPath);
        if (mat == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            mat = new Material(shader);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            Directory.CreateDirectory(Path.GetDirectoryName(BackgroundMatPath));
            AssetDatabase.CreateAsset(mat, BackgroundMatPath);
            AssetDatabase.SaveAssets();
        }
        else
        {
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            EditorUtility.SetDirty(mat);
        }

        // Try to make sure it cannot occlude gameplay (disable depth write when available)
        if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 1000; // background

        // Create or reuse quad
        var existing = GameObject.Find("BoardBackground");
        GameObject quad = existing ?? GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "BoardBackground";
        var col = quad.GetComponent<Collider>(); if (col != null) Object.DestroyImmediate(col);
        var mr = quad.GetComponent<MeshRenderer>(); if (mr == null) mr = quad.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;

        // Fit to camera
        var fitter = quad.GetComponent<BoardBackgroundFitter>();
        if (fitter == null) fitter = quad.AddComponent<BoardBackgroundFitter>();
        fitter.targetCamera = cam;
        fitter.autoDistance = true; // place near far clip so it never intersects the board
        fitter.lockToCamera = true;
        fitter.distanceFromCamera = Mathf.Max(20f, cam.farClipPlane - 5f);
        fitter.Fit();

        // Parent under camera so it follows
        quad.transform.SetParent(cam.transform, false);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Board background set up safely (far from gameplay, no occlusion).");
    }
}

