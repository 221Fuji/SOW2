using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UICSSkillBox : UIPersonalAct
{
    [SerializeField] private TextMeshProUGUI _skillName;
    [SerializeField] private TextMeshProUGUI _operationGuide;
    [SerializeField] private GameObject _skillButtonField;
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private TextMeshProUGUI _skillDiscription;

    public void SetData(CmdListBox listSource)
    {
        _skillName.text = listSource.SkillName;
        _operationGuide.text = listSource.OperationGuide;
        UIField skillField = _skillButtonField.GetComponent<UIField>();
        skillField.ChangeActionType(listSource.SkillOpEnum);
        skillField.ChangedIcon();
        _videoPlayer.clip = listSource.SkillVideo;
        _skillDiscription.text = listSource.SkillDiscription;
    }

    public override void FocusedAction(GameObject ob)
    {
        
    }

    public override void SeparateAction(GameObject ob)
    {
        
    }
}
