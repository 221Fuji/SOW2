using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Reflection;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


public class CPUAgent : Agent
{
    public FightingInputReceiver InputReciever { get; private set; }
    public int SelfCharaNum { get; private set; }
    public int EnemyCharaNum { get; private set; }
    public CharacterActions SelfCA { get; private set; }
    public CharacterActions EnemyCA { get; private set; }
    public CharacterState SelfCS { get; private set; }
    public CharacterState EnemysCS { get; private set; }

<<<<<<< HEAD
    private int _preMoveNum = 0; // �O��̍s���ԍ�
    private int _leftFrameSinceHit = 0; // �U���𓖂Ăāi�K�[�h�����āj����̎���
    private readonly int _assertiveness = 120; // �U���𓖂Ă悤�Ƃ���ϋɐ�(��������������)
    private int _leftFrameSinceChangeVector; // ����������ς��Ă���̎���
    private readonly int _frequencyWalkChange = 30; // ���������̐؂�ւ��p�x
    private int _leftFrameSinceGuard = 0; // �K�[�h���̃t���[��
    private readonly int _frequencyGuardChange = 30; // �K�[�h�؂�ւ��p�x
=======
>>>>>>> 5120a4b11980d8433d5ee9514b5d1bf021675f04

    public void SetCAandCS()
    {
        InputReciever = GetComponent<FightingInputReceiver>();
        SelfCA = GetComponent<CharacterActions>();
        SelfCS = GetComponent<CharacterState>();
        EnemyCA = SelfCA.EnemyCA;
        EnemysCS = EnemyCA.GetComponent<CharacterState>();

        OnEpisodeStart();
    }

    private void OnEpisodeStart()
    {
        // �f���Q�[�g�̐ݒ�
<<<<<<< HEAD
        SelfCA.OnHurtAI += OnHurt;
        SelfCA.OnGuardAI += OnGuard;
        SelfCA.OnComboAI += OnCombo;
        SelfCA.OnDieAI += OnDie;
        SelfCA.OnBreakAI += OnBreak;
        SelfCA.OnMissAI += OnMiss;

        EnemyCA.OnHurtAI += OnHit;
        EnemyCA.OnGuardAI += OnGuarded;
        EnemyCA.OnDieAI += OnKill;
        EnemyCA.OnMissAI = OnAvert;
=======
        SelfCA.OnHurtAI = OnHurt;
        SelfCA.OnComboAI = OnCombo;
        SelfCA.OnDieAI = OnDie;
        SelfCA.OnGuardAI = OnGuard;
        SelfCA.OnBreakAI = OnBreak;
        EnemyCA.OnHurtAI = OnHit;
        EnemyCA.OnGuardAI = OnGuarded;
        EnemyCA.OnBreakAI = OnBroken;
        EnemyCA.OnDieAI = OnKill;
>>>>>>> 5120a4b11980d8433d5ee9514b5d1bf021675f04
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // �����̊�{�����ϑ�
        sensor.AddObservation(SelfCharaNum);
        sensor.AddObservation(SelfCA.GetPushBackBox().center);
        sensor.AddObservation(SelfCS.CurrentHP / SelfCS.MaxHP);
        sensor.AddObservation(SelfCS.CurrentSP / SelfCS.MaxSP);
        sensor.AddObservation(SelfCS.CurrentUP / SelfCS.MaxUP);

        // ����̊�{�����ϑ�
        sensor.AddObservation(EnemyCharaNum);
<<<<<<< HEAD
        sensor.AddObservation(EnemyCA.HurtBox.transform.position);
        sensor.AddObservation(EnemyCS.CurrentHP / EnemyCS.MaxHP);
        sensor.AddObservation(EnemyCS.CurrentSP / EnemyCS.MaxSP);
        sensor.AddObservation(EnemyCS.CurrentUP / SelfCS.MaxUP);
        bool isEnemyBroken = EnemyCS.AnormalyStates.Contains(AnormalyState.Fatigue);
        sensor.AddObservation(isEnemyBroken ? 1 : 0);

        //���Ώ��
        Vector2 distance = SelfCA.GetPushBackBox().center - (Vector2)EnemyCA.HurtBox.transform.position;
        sensor.AddObservation(distance);
=======
        sensor.AddObservation(EnemyCA.GetPushBackBox().center);
        sensor.AddObservation(EnemysCS.CurrentHP / EnemysCS.MaxHP);
        sensor.AddObservation(EnemysCS.CurrentSP / EnemysCS.MaxSP);

        /*
        //�����̏��
        foreach(FightingRigidBody frb in FightingRigidBody.FightingRigidBodies)
        {
            if(frb is Fog fog)
            {
                sensor.AddObservation(fog.PlayerNum);
                sensor.AddObservation(fog.transform.position);
                sensor.AddObservation(fog.transform.lossyScale);
            }
        }

        //�U���̓����蔻��
        foreach(HitBoxManager hitBox in HitBoxManager.HitBoxes)
        {
            sensor.AddObservation(hitBox.PlayerNum);
            sensor.AddObservation(hitBox.transform.position);
            sensor.AddObservation(hitBox.transform.lossyScale);
        }

        //�ŗL���\�[�X�̊ϑ�
        if(SelfCA is ViolaCloud selfCloud)
        {
            sensor.AddObservation(selfCloud.CurrentFogResource / selfCloud.FogMaxResource);
        }
        if(EnemyCA is ViolaCloud enemysCloud)
        {
            sensor.AddObservation(enemysCloud.CurrentFogResource / enemysCloud.FogMaxResource);
        }
        */
>>>>>>> 5120a4b11980d8433d5ee9514b5d1bf021675f04
    }

    private void Update()
    {
        RequestDecision();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int move = actions.DiscreteActions[0];

        // �Ⴆ�� 0 = Idle, 1 = Walk Left, 2 = Walk Right, 3 = Attack, 4 = Guard, ...
        switch (move)
        {
            case 0: InputReciever.SetWalkDirectionFromAI(0); break;
            case 1: InputReciever.SetWalkDirectionFromAI(-1f); break;
            case 2: InputReciever.SetWalkDirectionFromAI(1f); break;
            case 3: InputReciever.OnJump(); break;
            case 4: InputReciever.OnNomalMove(); break;
            case 5: InputReciever.OnSpecialMove1(); break;
            case 6: InputReciever.OnUltimate(); break;
            case 7: InputReciever.SetGuardFromAI(true); break;
            case 8: InputReciever.SetGuardFromAI(false); break;
        }

        //�󒆂ɂ���̂͂��܂�悭�Ȃ�
        if(SelfCA.OnGround)
        {
            AddReward(-0.005f);
        }
<<<<<<< HEAD

        if (SelfCA.CanJump)
        {
            actionMask.SetActionEnabled(0, 3, true);
        }

        if (SelfCA.CanGuard)
        {
            actionMask.SetActionEnabled(0, 8, true);
        }

        if (SelfCS.IsGuarding)
        {
            actionMask.SetActionEnabled(0, 9, true);
        }

        DiscreteUniqueActionMask(actionMask);
    }

    public abstract void DiscreteUniqueActionMask(IDiscreteActionMask actionMask);


    /// <summary>
    /// ���t���[����V��^���鍀��
    /// </summary>
    private void AddRewardEveryFrame(int currentMoveNum)
    {
        //�����~�܂�̌��_
        if(currentMoveNum == 0)
        {
            AddReward(-0.0005f);
        }

        //�J�N�J�N���Ȃ��悤�ɂ��邽��
        bool changeFromLeft = _preMoveNum == 1 && (currentMoveNum == 0 || currentMoveNum == 2);
        bool changeFromRight = _preMoveNum == 2 && (currentMoveNum == 0 || currentMoveNum == 1);
        if(changeFromLeft || changeFromRight)
        {
            if (_leftFrameSinceChangeVector < _frequencyWalkChange)
            {
                AddReward(-0.01f);
            }
            _leftFrameSinceChangeVector = 0;
        }

        //�K�[�h�؂�ւ��A�ł����Ȃ��悤�ɂ��邽��
        if (currentMoveNum == 9)
        {
            if (_leftFrameSinceGuard < _frequencyGuardChange)
            {
                AddReward(-0.05f);
            }
        }
        if (SelfCS.IsGuarding)
        {
            _leftFrameSinceGuard++;
        }
        else
        {
            _leftFrameSinceGuard = 0;
        }

        //�ϋɓI�ɍU������悤��
        if(_leftFrameSinceHit < _assertiveness)
        {
            AddReward(-0.0005f);
        }

        _preMoveNum = currentMoveNum;
    }

    //�U�����������Ƃ�
    protected virtual void OnHurt(AttackInfo attackInfo)
    {
        AddReward(CalRewardByDamage(-1f, attackInfo.Damage));
    }

    //�U�����q�b�g�����Ƃ�
    protected virtual void OnHit(AttackInfo attackInfo)
    {
        AddReward(CalRewardByDamage(1.25f, attackInfo.Damage));
        _leftFrameSinceHit = 0;
    }

    //�U�����K�[�h�����Ƃ�
    protected virtual void OnGuard(AttackInfo attackInfo)
    {
        AddReward(CalRewardByDrainSP(0.1f, attackInfo.DrainSP));
        //�U���K�[�h�����シ���ɃK�[�h���������Ă�OK
        _leftFrameSinceGuard = _frequencyGuardChange;
    }

    //�U�����K�[�h�������Ƃ�
    protected virtual void OnGuarded(AttackInfo attackInfo)
    {
        AddReward(CalRewardByDrainSP(1, attackInfo.DrainSP));
        _leftFrameSinceHit = 0;
    }

    //�U������U�肵���Ƃ�
    protected virtual void OnMiss()
    {
        AddReward(-0.5f);
    }

    //�U�����������
    protected virtual void OnAvert()
    {
        AddReward(0.25f);
    }

    //�R���{�����Ƃ�
    protected virtual void OnCombo()
    {
        AddReward(0.75f);
    }

    //���g��Break��ԂɂȂ����Ƃ�
    protected virtual void OnBreak()
=======
    }

    //�U�����������Ƃ�
    private void OnHurt()
>>>>>>> 5120a4b11980d8433d5ee9514b5d1bf021675f04
    {
        AddReward(-1f);
    }

    //�U�����q�b�g�������Ƃ�
    private void OnHit()
    {
        AddReward(1f);
    }

    //�U�����K�[�h�����Ƃ�
    private void OnGuard()
    {
        AddReward(0.5f);
    }

    //�U�����K�[�h�������Ƃ�
    private void OnGuarded()
    {
        AddReward(0.5f);
    }

    //�R���{�����Ƃ�
    private void OnCombo()
    {
        AddReward(0.5f);
    }

    //���g��Break��ԂɂȂ����Ƃ�
    private void OnBreak()
    {
        AddReward(-2f);
    }

    //�����Break��Ԃɂ����Ƃ�
    private void OnBroken() 
    {
        AddReward(0.5f);
    }

    //�|�����Ƃ�
    private void OnKill()
    {
        SetReward(5f);
        EndEpisode();
    }

    //�|���ꂽ�Ƃ�
    private void OnDie()
    {
        SetReward(-5f);
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // �J���p�Ƀe�X�g������L�q�i����͕s�v�ł�OK�j
    }

    protected float CalRewardByDamage(float baseReward, float damage)
    {       
        return baseReward * (damage / 20);
    }

    protected float CalRewardByDrainSP(float baseReward, float drainSP)
    {
        return baseReward * (drainSP / 20);
    }
}
