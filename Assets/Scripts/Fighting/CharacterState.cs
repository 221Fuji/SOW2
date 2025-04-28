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
    /// “G‚Ì¶‘¤‚É‚¢‚é‚È‚çtrue
    /// </summary>
    public bool IsLeftSide { get; private set; } = true;
    /// <summary>
    /// •à‚«“ü—Í‚ğ‚µ‚Ä‚¢‚é‚È‚çtrue
    /// </summary>
    public bool IsInputtingWalk { get; private set; } = false;
    /// <summary>
    /// “G‚Éæ‚ç‚ê‚Ä‚¢‚é‚È‚çtrue
    /// </summary>
    public bool IsRidenByEnemy { get; private set; } = false;
    /// <summary>
    /// ƒqƒbƒgd’¼’†‚È‚çtrue
    /// </summary>
    public bool IsRecoveringHit { get { return _hitCTS != null; } }
    /// <summary>
    /// ƒK[ƒh‚µ‚Ä‚¢‚é‚©‚Ç‚¤‚©
    /// </summary>
    public bool IsGuarding { get; private set; }
    /// <summary>
    /// ƒK[ƒhd’¼’†‚È‚çtrue
    /// </summary>
    public bool IsRecoveringGuard { get { return _guardCTS != null; } }
    public List<AnormalyState> AnormalyStates { get; private set; } = new List<AnormalyState>();
    public int ConboCount { get; private set; }
    public List<string> NameOfGivenAttack { get; private set; } = new List<string>();

    //ƒfƒŠƒQ[ƒg
    public UnityAction Break { get; set; }
    public UnityAction RecoverBreak { get; set; }

    //UŒ‚‚ğó‚¯‚½‚Ìˆ—‚É•K—v‚È•Ï”
    private CancellationTokenSource _hitCTS;
    
    //ƒK[ƒh‚µ‚½‚Æ‚«‚Ìˆ—‚É•K—v‚È•Ï”
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
        ConboCount = 0;
        AnormalyStates.Clear();
    }

    public void SetAcceptOperations(bool value)
    {
        Debug.Log($"AccceptOperation:{value}");
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

        //”æ˜Jó‘Ô‚É‚È‚é
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
                //”æ˜Jó‘Ô‰ñ•œ
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
    /// ƒqƒbƒgd’¼‚ğI—¹‚³‚¹‚é
    /// </summary>
    public void CancelHitStun()
    {
        _hitCTS?.Cancel();
        _hitCTS = null;
    }

    /// <summary>
    /// HitCTS‚ğ¶¬‚µƒŠƒXƒg‚É“ü‚ê‚é
    /// <summary> 
    public CancellationToken CreateHitCT()
    {
        _hitCTS = new CancellationTokenSource();
        return _hitCTS.Token;
    }
    
    /// <summary>
    /// ƒRƒ“ƒ{‚ğ”‚¦‚é
    /// </summary>
    public void SetComboCount(int value)
    {
        ConboCount = value;
    }

    /// <summary>
    /// ƒqƒbƒgd’¼‚ğI—¹‚³‚¹‚é
    /// </summary>
    public void CancelGuardStun()
    {
        _guardCTS?.Cancel();
        _guardCTS = null;
    }

    /// <summary>
    /// HitCTS‚ğ¶¬‚µƒŠƒXƒg‚É“ü‚ê‚é
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
/// ‘Šè‚©‚ç•t—^‚³‚ê‚éó‘ÔˆÙíA‚Ü‚½‚Í€–Só‘Ô
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
/// •à‚«‚Ìó‘Ô
/// </summary>
public enum WalkMode
{
    Default,
    Front,
    Back
}
