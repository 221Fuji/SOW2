using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// HurtBoxにアタッチする
/// </summary>
public class HurtBoxManager : MonoBehaviour
{
    public bool IsActive { get; private set; }
    public int PlayerNum { get; private set; }
    public delegate UniTask OnHurt(AttackInfo attackInfo);
    public OnHurt OnHurtDelegate { get; set; }

    /// <summary>
    /// 攻撃によって破壊できるものに登録する
    /// </summary>
    public UnityAction<Transform> OnHurtWithTransform { get; set; }

    public void SetActive(bool value)
    {
        IsActive = value;
    }

    public void SetPlayerNum(int playerNum)
    {
        PlayerNum = playerNum;
    }

    public void TakeAttack(AttackInfo attackInfo)
    {
        if (!IsActive) return;
        OnHurtDelegate?.Invoke(attackInfo);
        OnHurtWithTransform?.Invoke(transform.parent);
    }
}
