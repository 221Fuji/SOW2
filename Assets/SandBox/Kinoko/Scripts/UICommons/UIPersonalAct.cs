using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;

/// <summary>
/// ボタン等のUIそれぞれに存在するクラスの継承元
/// </summary>
public abstract class UIPersonalAct : MonoBehaviour
{

    /// <summary>
    /// UI移動において例外的な動きをさせたい際のオプション(侵入方向を左からに限定など)
    /// </summary>
    /// <returns></returns>
    public virtual bool MovingException(UIMovingCtrl _ctrl)
    {
        return false;
    }

    public virtual void FocusedAction(GameObject _ob)
    {
        
    }

    public virtual void SeparateAction(GameObject _ob)
    {

    }
    public virtual async void ClickedAction()
    {

    }


}



