using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu]
public class CmdListBox : ScriptableObject
{
    [Header("技名")]
    [SerializeField] private string _skillName;
    [Header("操作ガイドのオプション")]
    [SerializeField] private string _operationGuide;
    [Header("操作ガイド(何ボタンを押したら出るのか)")]
    [SerializeField] private UIActionList _skillOpEnum;
    [Header("イメージソース")]
    [SerializeField] private Sprite _skillImage;
    [Header("技の説明")]
    [SerializeField] private string _skillDiscription;

    public string SkillName { get { return _skillName; } }
    public string OperationGuide { get { return _operationGuide; } }
    public UIActionList SkillOpEnum { get { return _skillOpEnum; } }
    public Sprite SkillImage { get { return _skillImage; } }
    public string SkillDiscription { get { return _skillDiscription;} }

}
