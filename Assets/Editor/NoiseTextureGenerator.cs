using UnityEngine;
using UnityEditor;

public class NoiseTextureGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Noise Texture")]
    public static void Generate()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        float scale = 4f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = x / (float)size * scale;
                float ny = y / (float)size * scale;

                float val = 0f;
                val += Mathf.PerlinNoise(nx,        ny)        * 1.0f;
                val += Mathf.PerlinNoise(nx * 2f,   ny * 2f)   * 0.5f;
                val += Mathf.PerlinNoise(nx * 4f,   ny * 4f)   * 0.25f;
                val += Mathf.PerlinNoise(nx * 8f,   ny * 8f)   * 0.125f;
                val /= 1.875f;

                val = Mathf.Clamp01(val);
                tex.SetPixel(x, y, new Color(val, val, val, val));
            }
        }

        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        string path = "Assets/Textures/NoiseTexture.png";

        System.IO.Directory.CreateDirectory("Assets/Textures");
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }

        Debug.Log("Noise texture saved to: " + path);
        EditorUtility.DisplayDialog("Done!", "Noise texture created at:\n" + path, "OK");
    }
}
