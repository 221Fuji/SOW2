using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CmdListBox : ScriptableObject
{
    [Header("技名")]
    [SerializeField] private string _skillName;
    [Header("操作ガイドのオプション")]
    [SerializeField] private string _operationGuide;
    [Header("操作ガイド(何ボタンを押したら出るのか)")]
    [SerializeField] private _SkillOpEnums _skillOpEnum;
    public enum _SkillOpEnums : byte
    {
        NormalMove,
        SpecialMove1,
        SpecialMove2,
        Ultimate,
    }
    [Header("gifアニメーション")]
    [SerializeField] private GameObject _imageGif;
    [Header("技の説明")]
    [SerializeField] private string _skillDiscription;


}
