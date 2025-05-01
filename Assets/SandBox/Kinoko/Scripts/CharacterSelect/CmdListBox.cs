using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CmdListBox : ScriptableObject
{
    [Header("�Z��")]
    [SerializeField] private string _skillName;
    [Header("����K�C�h�̃I�v�V����")]
    [SerializeField] private string _operationGuide;
    [Header("����K�C�h(���{�^������������o��̂�)")]
    [SerializeField] private _SkillOpEnums _skillOpEnum;
    public enum _SkillOpEnums : byte
    {
        NormalMove,
        SpecialMove1,
        SpecialMove2,
        Ultimate,
    }
    [Header("gif�A�j���[�V����")]
    [SerializeField] private GameObject _imageGif;
    [Header("�Z�̐���")]
    [SerializeField] private string _skillDiscription;


}
