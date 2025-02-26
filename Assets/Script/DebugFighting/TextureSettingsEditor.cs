using UnityEditor;
using UnityEngine;

public class TextureSettingsEditor : EditorWindow
{
    private Texture2D texture;
    private TextureImporterType textureType = TextureImporterType.Sprite;
    private SpriteImportMode spriteMode = SpriteImportMode.Multiple;
    private float pixelsPerUnit = 256f;
    private FilterMode filterMode = FilterMode.Point;
    private TextureImporterCompression compression = TextureImporterCompression.Uncompressed;
    private int sliceWidth = 210;
    private int sliceHeight = 90;
    private enum PivotOption { Custom, Bottom }
    private PivotOption pivotOption = PivotOption.Custom;
    private Vector2 customPivot = new Vector2(0.5f, 0.5f);

    [MenuItem("Tools/Texture Settings Editor")]
    public static void ShowWindow()
    {
        GetWindow<TextureSettingsEditor>("Texture Settings Editor");
    }

    [System.Obsolete]
    private void OnGUI()
    {
        GUILayout.Label("Texture Settings", EditorStyles.boldLabel);

        texture = (Texture2D)EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), false);
        textureType = (TextureImporterType)EditorGUILayout.EnumPopup("Texture Type", textureType);
        spriteMode = (SpriteImportMode)EditorGUILayout.EnumPopup("Sprite Mode", spriteMode);
        pixelsPerUnit = EditorGUILayout.FloatField("Pixels Per Unit", pixelsPerUnit);
        filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", filterMode);
        compression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Compression", compression);

        GUILayout.Label("Slice Settings", EditorStyles.boldLabel);
        sliceWidth = EditorGUILayout.IntField("Slice Width", sliceWidth);
        sliceHeight = EditorGUILayout.IntField("Slice Height", sliceHeight);

        pivotOption = (PivotOption)EditorGUILayout.EnumPopup("Pivot", pivotOption);
        if (pivotOption == PivotOption.Custom)
        {
            customPivot = EditorGUILayout.Vector2Field("Custom Pivot", customPivot);
        }

        if (GUILayout.Button("Apply Settings"))
        {
            ApplyTextureSettings();
        }

        if (GUILayout.Button("Slice Texture"))
        {
            SliceTexture();
        }
    }

    private void ApplyTextureSettings()
    {
        if (texture == null)
        {
            Debug.LogError("Texture not selected!");
            return;
        }

        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer != null)
        {
            importer.textureType = textureType;
            importer.spriteImportMode = spriteMode;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = filterMode;
            importer.textureCompression = compression;

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            Debug.Log("Texture settings applied successfully.");
        }
        else
        {
            Debug.LogError("Failed to load texture importer.");
        }
    }

    [System.Obsolete]
    private void SliceTexture()
    {
        if (texture == null)
        {
            Debug.LogError("Texture not selected!");
            return;
        }

        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer != null && importer.spriteImportMode == SpriteImportMode.Multiple)
        {
            int columns = texture.width / sliceWidth;
            int rows = texture.height / sliceHeight;
            Vector2 pivot = pivotOption == PivotOption.Bottom ? new Vector2(0.5f, 0f) : customPivot;

            SpriteMetaData[] slices = new SpriteMetaData[columns * rows];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    SpriteMetaData meta = new SpriteMetaData();
                    meta.rect = new Rect(x * sliceWidth, texture.height - (y + 1) * sliceHeight, sliceWidth, sliceHeight);
                    meta.name = texture.name + "_" + (y * columns + x);
                    meta.alignment = (int)SpriteAlignment.Custom;
                    meta.pivot = pivot;
                    slices[y * columns + x] = meta;
                }
            }

            importer.spritesheet = slices;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            Debug.Log("Texture sliced successfully.");
        }
        else
        {
            Debug.LogError("Texture must be set to Sprite (Multiple) mode before slicing.");
        }
    }
}









