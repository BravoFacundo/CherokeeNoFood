using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TMPro;

// Editor tool: creates a neon hover material preset for the selected TMP font asset.
// Atlas data comes from the font's own material; the glow look is copied from an existing *NeonHover preset.
public static class NeonPresetCreator
{
    private static readonly string[] FloatProperties =
    {
        "_GlowOffset", "_GlowInner", "_GlowOuter", "_GlowPower", "_FaceDilate",
        "_UnderlayOffsetX", "_UnderlayOffsetY", "_UnderlayDilate", "_UnderlaySoftness"
    };
    private static readonly string[] ColorProperties = { "_GlowColor", "_UnderlayColor" };

    [MenuItem("Cherokee/Create Neon Hover Preset From Selected Font")]
    private static void CreatePreset()
    {
        if (Selection.activeObject is not TMP_FontAsset font)
        {
            EditorUtility.DisplayDialog("Neon Hover Preset", "Select a TMP Font Asset (SDF) in the Project window first.", "OK");
            return;
        }

        string fontPath = AssetDatabase.GetAssetPath(font);
        Material reference = FindReferencePreset(fontPath);

        // Copy of the font's base material: correct atlas, gradient scale and texture size
        Material preset = new Material(font.material) { name = font.name + "_NeonHover" };

        if (reference != null) CopyNeonLook(reference, preset);
        else ApplyDefaultNeonLook(preset);

        string path = Path.Combine(Path.GetDirectoryName(fontPath), preset.name + ".mat").Replace('\\', '/');
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        AssetDatabase.CreateAsset(preset, path);
        AssetDatabase.SaveAssets();

        Selection.activeObject = preset;
        EditorGUIUtility.PingObject(preset);
        Debug.Log($"Neon hover preset created: {path}" + (reference != null ? $" (look copied from {reference.name})" : " (default look)"));
    }

    // Any existing *NeonHover material that belongs to another font
    private static Material FindReferencePreset(string fontPath)
    {
        string fontFolder = Path.GetDirectoryName(fontPath).Replace('\\', '/');
        string fontName = Path.GetFileNameWithoutExtension(fontPath);

        return AssetDatabase.FindAssets("NeonHover t:Material")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => !Path.GetFileName(p).StartsWith(fontName))
            .Select(AssetDatabase.LoadAssetAtPath<Material>)
            .FirstOrDefault(m => m != null);
    }

    private static void CopyNeonLook(Material from, Material to)
    {
        foreach (string property in FloatProperties)
            if (from.HasProperty(property) && to.HasProperty(property)) to.SetFloat(property, from.GetFloat(property));
        foreach (string property in ColorProperties)
            if (from.HasProperty(property) && to.HasProperty(property)) to.SetColor(property, from.GetColor(property));

        SetKeyword(to, ShaderUtilities.Keyword_Glow, from.IsKeywordEnabled(ShaderUtilities.Keyword_Glow));
        SetKeyword(to, ShaderUtilities.Keyword_Underlay, from.IsKeywordEnabled(ShaderUtilities.Keyword_Underlay));
    }

    private static void ApplyDefaultNeonLook(Material material)
    {
        material.SetColor("_GlowColor", new Color(1f, 0.3f, 0.9f, 0.8f));
        material.SetFloat("_GlowOuter", 0.6f);
        material.SetFloat("_GlowPower", 0.75f);
        SetKeyword(material, ShaderUtilities.Keyword_Glow, true);
    }

    private static void SetKeyword(Material material, string keyword, bool enabled)
    {
        if (enabled) material.EnableKeyword(keyword);
        else material.DisableKeyword(keyword);
    }
}
