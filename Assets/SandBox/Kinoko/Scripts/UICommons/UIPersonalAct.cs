using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// �{�^������UI���ꂼ��ɑ��݂���N���X�̌p����
/// </summary>
public abstract class UIPersonalAct : MonoBehaviour
{

    /// <summary>
    /// UI�ړ��ɂ����ė�O�I�ȓ��������������ۂ̃I�v�V����(�N��������������Ɍ���Ȃ�)
    /// </summary>
    /// <returns></returns>
    public virtual bool MovingException(GameObject ob)
    {
        return false;
    }

    public virtual void FocusedAction(GameObject _ob)
    {
        
    }

    public virtual void SeparateAction(GameObject _ob)
    {

    }
    public virtual async void ClickedAction(GameObject _ob)
    {

    }

    public virtual void InstanceObject(GameObject _ob)
    {

    }


}



