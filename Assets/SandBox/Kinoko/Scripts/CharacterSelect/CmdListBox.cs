using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu]
public class CmdListBox : ScriptableObject
{
    [Header("�Z��")]
    [SerializeField] private string _skillName;
    [Header("����K�C�h�̃I�v�V����")]
    [SerializeField] private string _operationGuide;
    [Header("����K�C�h(���{�^������������o��̂�)")]
    [SerializeField] private UIActionList _skillOpEnum;
    [Header("gif�A�j���[�V����")]
    [SerializeField] private VideoClip _skillVideo;
    [Header("�Z�̐���")]
    [SerializeField] private string _skillDiscription;

    public string SkillName { get { return _skillName; } }
    public string OperationGuide { get { return _operationGuide; } }
    public UIActionList SkillOpEnum { get { return _skillOpEnum; } }
    public VideoClip SkillVideo { get { return _skillVideo; } }
    public string SkillDiscription { get { return _skillDiscription;} }

}
