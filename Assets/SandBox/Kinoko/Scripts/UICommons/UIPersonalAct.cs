using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// �{�^������UI���ꂼ��ɑ��݂���N���X�̌p����
/// </summary>
public class UIPersonalAct : MonoBehaviour
{
    /// <summary>
    /// UI�ړ��ɂ����ė�O�I�ȓ��������������ۂ̃I�v�V����(�N��������������Ɍ���Ȃ�)
    /// </summary>
    /// <returns></returns>
    public virtual bool MovingException(UIMovingCtrl ctrl)
    {
        return false;
    }

    public virtual void FocusedAction(GameObject ob)
    {
        
    }

    public virtual void SeparateAction(GameObject ob)
    {

    }
    public virtual void ClickedAction(GameObject ob)
    {

    }

    public virtual void InstanceObject(GameObject ob)
    {

    }


}



