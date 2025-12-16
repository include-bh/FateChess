using UnityEngine;
using UnityEditor;
using System.IO;

public class ExportTMPAtlas
{
    [MenuItem("Tools/Export TMP Font Atlas as PNG")]
    public static void ExportAtlas()
    {
        var fontAsset = Selection.activeObject as TMPro.TMP_FontAsset;
        if (fontAsset == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a TMP Font Asset in Project window.", "OK");
            return;
        }

        var atlasTexture = fontAsset.atlasTexture;
        if (atlasTexture == null)
        {
            EditorUtility.DisplayDialog("Error", "Font Asset has no atlas texture.", "OK");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(fontAsset);
        string pngPath = Path.ChangeExtension(assetPath, ".png");

        // 读取纹理像素
        RenderTexture prevRT = RenderTexture.active;
        RenderTexture tempRT = RenderTexture.GetTemporary(
            atlasTexture.width,
            atlasTexture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear // 关键：避免 sRGB 转换
        );

        Graphics.Blit(atlasTexture, tempRT);
        RenderTexture.active = tempRT;

        Texture2D exported = new Texture2D(atlasTexture.width, atlasTexture.height, TextureFormat.RGBA32, false);
        exported.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        exported.Apply();

        RenderTexture.active = prevRT;
        RenderTexture.ReleaseTemporary(tempRT);

        // 编码为 PNG
        byte[] bytes = exported.EncodeToPNG();
        File.WriteAllBytes(pngPath, bytes);
        Object.DestroyImmediate(exported);

        AssetDatabase.ImportAsset(pngPath);
        Debug.Log($"✅ Exported TMP Atlas to: {pngPath}");

        // 自动修正导入设置
        var importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;          // 关闭 sRGB
            importer.mipmapEnabled = false;        // 关闭 Mip Maps
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            Debug.Log("✅ Auto-fixed texture import settings.");
        }

        EditorUtility.RevealInFinder(pngPath);
    }
}