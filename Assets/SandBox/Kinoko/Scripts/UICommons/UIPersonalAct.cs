using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ボタン等のUIそれぞれに存在するクラスの継承元
/// </summary>
public class UIPersonalAct : MonoBehaviour
{
    /// <summary>
    /// UI移動において例外的な動きをさせたい際のオプション(侵入方向を左からに限定など)
    /// </summary>
    /// <returns></returns>
    public virtual bool MovingException(GameObject ob)
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



