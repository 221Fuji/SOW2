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
    [Header("gifアニメーション")]
    [SerializeField] private VideoClip _skillVideo;
    [Header("技の説明")]
    [SerializeField] private string _skillDiscription;

    public string SkillName { get { return _skillName; } }
    public string OperationGuide { get { return _operationGuide; } }
    public UIActionList SkillOpEnum { get { return _skillOpEnum; } }
    public VideoClip SkillVideo { get { return _skillVideo; } }
    public string SkillDiscription { get { return _skillDiscription;} }

}
