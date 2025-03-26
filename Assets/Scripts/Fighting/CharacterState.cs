using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class CharacterState : MonoBehaviour
{
    [SerializeField] private float _defaultMaxHP;
    [SerializeField] private float _defaultMaxSP;
    [SerializeField] private float _defaultMaxUP;
    [SerializeField] private float _upGainSpeed;
    [SerializeField] private float _defaultFrontSpeed;
    [SerializeField] private float _defaultBackSpeed;
    [SerializeField] private float _defaultJumpPower; 

    public float CurrentHP { get; private set; }
    public float CurrentSP { get; private set; }
    public float CurrentUP { get; private set; }
    public float CurrentFrontSpeed { get; private set; }
    public float CurrentBackSpeed { get; private set; }
    public float CurrentJumpPower { get; private set; }
    public float MaxHP { get { return _defaultMaxHP; } }
    public float MaxSP { get { return _defaultMaxSP; } }
    public float MaxUP { get { return _defaultMaxUP; } }
    public float UPgainSpeed { get { return _upGainSpeed; } }

    public bool AcceptOperations { get; private set; } = false;
    /// <summary>
    /// 敵の左側にいるならtrue
    /// </summary>
    public bool IsLeftSide { get; private set; } = true;
    /// <summary>
    /// 歩き入力をしているならtrue
    /// </summary>
    public bool IsInputtingWalk { get; private set; } = false;
    /// <summary>
    /// 敵に乗られているならtrue
    /// </summary>
    public bool IsRidenByEnemy { get; private set; } = false;
    /// <summary>
    /// ヒット硬直中ならtrue
    /// </summary>
    public bool IsRecoveringHit { get { return _hitCTS != null; } }
    /// <summary>
    /// ガードしているかどうか
    /// </summary>
    public bool IsGuarding { get; private set; }
    /// <summary>
    /// ガード硬直中ならtrue
    /// </summary>
    public bool IsRecoveringGuard { get { return _guardCTS != null; } }
    public List<AnormalyState> AnormalyStates { get; private set; }
    public int ConboCount { get; private set; }
    public List<string> NameOfGivenAttack { get; private set; }

    //デリゲート
    public UnityAction Break { get; set; }
    public UnityAction RecoverBreak { get; set; }

    //攻撃を受けた時の処理に必要な変数
    private CancellationTokenSource _hitCTS;
    
    //ガードしたときの処理に必要な変数
    private CancellationTokenSource _guardCTS;

    public void Awake()
    {
        CurrentUP = 0;
        ResetState();
    }

    public void ResetState()
    {
        CurrentHP = _defaultMaxHP;
        CurrentSP = _defaultMaxSP;
        CurrentFrontSpeed = _defaultFrontSpeed;
        CurrentBackSpeed = _defaultBackSpeed;
        CurrentJumpPower = _defaultJumpPower;
        AnormalyStates = new List<AnormalyState>();
        NameOfGivenAttack = new List<string>();
        ConboCount = 0;
    }

    public void SetAcceptOperations(bool value)
    {
        AcceptOperations = value;
    }

    public void SetIsLeft(bool value)
    {
        IsLeftSide = value;
    }

    public void SetIsInputtingWalk(bool value)
    {
        IsInputtingWalk = value;
    }

    public void SetIsRidenByEnemy(bool value)
    {
        IsRidenByEnemy = value;
    }

    public void SetIsGuarding(bool value)
    {
        IsGuarding = value;
    }

    public void TakeDamage(float damageValue)
    {
        CurrentHP -= damageValue;

        if(CurrentHP > MaxHP)
        {
            CurrentHP = MaxHP;
        }
        if(CurrentHP < 0)
        {
            CurrentHP = 0;
        }
    }

    public void SetCurrentSP(float value)
    {
        CurrentSP += value;

        //疲労状態になる
        if (CurrentSP <= 0)
        {
            CurrentSP = 0;
            TakeAnormalyState(AnormalyState.Fatigue);
            Break?.Invoke();
        }

        if(CurrentSP >= MaxSP)
        {
            CurrentSP = MaxSP;

            if(AnormalyStates.Contains(AnormalyState.Fatigue))
            {
                //疲労状態回復
                RecoverAnormalyState(AnormalyState.Fatigue);
                RecoverBreak?.Invoke();
            }           
        }
    }

    public void SetCurrentUP(float value)
    {
        CurrentUP += value;

        if(CurrentUP > MaxUP)
        {
            CurrentUP = MaxUP;
        }
        else if(CurrentUP < 0)
        {
            CurrentUP = 0;
        }
    }

    /// <summary>
    /// ヒット硬直を終了させる
    /// </summary>
    public void CancelHitStun()
    {
        _hitCTS?.Cancel();
        _hitCTS = null;
    }

    /// <summary>
    /// HitCTSを生成しリストに入れる
    /// <summary> 
    public CancellationToken CreateHitCT()
    {
        _hitCTS = new CancellationTokenSource();
        return _hitCTS.Token;
    }
    
    /// <summary>
    /// コンボを数える
    /// </summary>
    public void SetComboCount(int value)
    {
        ConboCount = value;
    }

    /// <summary>
    /// ヒット硬直を終了させる
    /// </summary>
    public void CancelGuardStun()
    {
        _guardCTS?.Cancel();
        _guardCTS = null;
    }

    /// <summary>
    /// HitCTSを生成しリストに入れる
    /// </summary>
    /// <returns></returns>  
    public CancellationToken CreateGuardCT()
    {
        _guardCTS = new CancellationTokenSource();
        return _guardCTS.Token;
    }

    public void TakeAnormalyState(AnormalyState anomalyState)
    {
        if(!AnormalyStates.Contains(anomalyState))
        {
            AnormalyStates.Add(anomalyState);
        }
    }

    public void RecoverAnormalyState(AnormalyState anomalyState)
    {
        if(AnormalyStates.Contains(anomalyState))
        {
            AnormalyStates.Remove(anomalyState);
        }
    }

    public void AddAttackName(AttackInfo attack)
    {
        NameOfGivenAttack.Add(attack.Name);
    }

    public void ClearNameOfGivenAttack()
    {
        NameOfGivenAttack.Clear();
    }

}

/// <summary>
/// 相手から付与される状態異常、または死亡状態
/// </summary>
public enum AnormalyState
{
    Default,
    Dead,
    Poison,
    Fatigue,
    Bind
}

/// <summary>
/// 歩きの状態
/// </summary>
public enum WalkMode
{
    Default,
    Front,
    Back
}
