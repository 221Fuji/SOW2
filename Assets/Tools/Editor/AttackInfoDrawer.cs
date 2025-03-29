using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AttackInfo))]
public class AttackInfoDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight; // フォールドアウトの高さ

        if (property.isExpanded)
        {
            height += EditorGUI.GetPropertyHeight(property, GUIContent.none, true); // 通常のフィールド
            height += (EditorGUIUtility.singleLineHeight + 4) * 2; // 計算結果4つ分 + 余白
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // **カスタムFoldout（デフォルトの二重表示を防ぐ）**
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            label
        );

        if (property.isExpanded)
        {
            // 既存のプロパティを描画（Foldoutの二重表示を防ぐため `GUIContent.none` を指定）
            Rect fieldRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUI.GetPropertyHeight(property, GUIContent.none, true));
            EditorGUI.indentLevel++; // インデントを付けて階層を揃える
            EditorGUI.PropertyField(fieldRect, property, GUIContent.none, true);
            EditorGUI.indentLevel--;

            // **各プロパティの取得**
            SerializedProperty guardFrameProp = property.FindPropertyRelative("GuardFrame");
            SerializedProperty activeFrameProp = property.FindPropertyRelative("ActiveFrame");
            SerializedProperty recoveryFrameProp = property.FindPropertyRelative("RecoveryFrame");
            SerializedProperty hitFrameProp = property.FindPropertyRelative("HitFrame");

            // **計算結果の算出**
            int calculatedValue1 = guardFrameProp.intValue - (activeFrameProp.intValue + recoveryFrameProp.intValue);
            int calculatedValue2 = hitFrameProp.intValue - (activeFrameProp.intValue + recoveryFrameProp.intValue);


            // **計算結果の表示（順番に下へ配置）**
            float yOffset = fieldRect.y + fieldRect.height + 2;

            Rect resultRect1 = new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(resultRect1, "GuardAdvantageFrame:", calculatedValue1.ToString());
            yOffset += EditorGUIUtility.singleLineHeight + 2;

            Rect resultRect2 = new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(resultRect2, "HitAdvantageFrame:", calculatedValue2.ToString());
            yOffset += EditorGUIUtility.singleLineHeight + 2;
        }

        EditorGUI.EndProperty();
    }
}

