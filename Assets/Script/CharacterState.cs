using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CharacterState : MonoBehaviour
{
    [SerializeField] private float _defaultMaxHp;
    [SerializeField] private float _defaultMaxSP;
    [SerializeField] private float _defaultFrontSpeed;
    [SerializeField] private float _defaultBackSpeed;
    [SerializeField] private float _defaultJumpPower; 

    public float CurrentHp { get; private set; }
    public float CurrentSP { get; private set; }
    public float CurrentFrontSpeed { get; private set; }
    public float CurrentBackSpeed { get; private set; }
    public float CurrentJumpPower { get; private set; }
    public float CurrentJumpElementY { get; private set; }
    public bool OnGround { get; private set; }
    public bool IsLeftSide { get; private set; }
    public List<AnomalyState> AnomalyStates { get; private set; }
    public int ConboCount { get; private set; }
    public bool IsRecoveringHit { get { return _hitCTS != null; } }

    //�U�����󂯂����̏����ɕK�v�ȕϐ�
    private CancellationTokenSource _hitCTS;
    

    public void Awake()
    {
        ResetState();
    }

    public void ResetState()
    {
        CurrentHp = _defaultMaxHp;
        CurrentSP = _defaultMaxSP;
        CurrentFrontSpeed = _defaultFrontSpeed;
        CurrentBackSpeed = _defaultBackSpeed;
        CurrentJumpPower = _defaultJumpPower;
        AnomalyStates = new List<AnomalyState>();
        AnomalyStates.Clear();
    }

    public void SetIsLeft(bool value)
    {
        IsLeftSide = value;
    }

    public void TakeDamage(int damageValue)
    {
        CurrentHp -= damageValue;

        //���S����
        if(CurrentHp <= 0)
        {
            AnomalyStates.Add(AnomalyState.DEAD);
        }
    }

    /// <summary>
    /// �q�b�g�d�����I��������
    /// </summary>
    public void CancelHitStun()
    {
        _hitCTS?.Cancel();
        _hitCTS = null;
    }

    /// <summary>
    /// HitCTS�𐶐������X�g�ɓ����
    /// </summary>
    /// <returns></returns>
    
    public CancellationToken CreateHitCT()
    {
        _hitCTS = new CancellationTokenSource();
        return _hitCTS.Token;
    }
    
    /// <summary>
    /// �R���{�𐔂���
    /// </summary>
    public void SetComboCount(int value)
    {
        ConboCount = value;
    }

    //�ڒn����
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            OnGround = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            OnGround = false;
        }
    }
}

/// <summary>
/// ���肩��t�^������Ԉُ�A�܂��͎��S���
/// </summary>
public enum AnomalyState
{
    DEAD,
    POISON
}
