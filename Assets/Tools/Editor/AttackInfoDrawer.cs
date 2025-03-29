using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AttackInfo))]
public class AttackInfoDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight; // �t�H�[���h�A�E�g�̍���

        if (property.isExpanded)
        {
            height += EditorGUI.GetPropertyHeight(property, GUIContent.none, true); // �ʏ�̃t�B�[���h
            height += (EditorGUIUtility.singleLineHeight + 4) * 2; // �v�Z����4�� + �]��
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // **�J�X�^��Foldout�i�f�t�H���g�̓�d�\����h���j**
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            label
        );

        if (property.isExpanded)
        {
            // �����̃v���p�e�B��`��iFoldout�̓�d�\����h������ `GUIContent.none` ���w��j
            Rect fieldRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUI.GetPropertyHeight(property, GUIContent.none, true));
            EditorGUI.indentLevel++; // �C���f���g��t���ĊK�w�𑵂���
            EditorGUI.PropertyField(fieldRect, property, GUIContent.none, true);
            EditorGUI.indentLevel--;

            // **�e�v���p�e�B�̎擾**
            SerializedProperty guardFrameProp = property.FindPropertyRelative("GuardFrame");
            SerializedProperty activeFrameProp = property.FindPropertyRelative("ActiveFrame");
            SerializedProperty recoveryFrameProp = property.FindPropertyRelative("RecoveryFrame");
            SerializedProperty hitFrameProp = property.FindPropertyRelative("HitFrame");

            // **�v�Z���ʂ̎Z�o**
            int calculatedValue1 = guardFrameProp.intValue - (activeFrameProp.intValue + recoveryFrameProp.intValue);
            int calculatedValue2 = hitFrameProp.intValue - (activeFrameProp.intValue + recoveryFrameProp.intValue);


            // **�v�Z���ʂ̕\���i���Ԃɉ��֔z�u�j**
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

