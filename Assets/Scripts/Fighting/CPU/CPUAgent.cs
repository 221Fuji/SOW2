using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


public abstract class CPUAgent : Agent
{
    [SerializeField] protected int _numActions;

    public FightingInputReceiver InputReciever { get; private set; }
    public int SelfCharaNum { get; private set; }
    public int EnemyCharaNum { get; private set; }
    public CharacterActions SelfCA { get; private set; }
    public CharacterActions EnemyCA { get; private set; }
    public CharacterState SelfCS { get; private set; }
    public CharacterState EnemyCS { get; private set; }

    private int _preMoveNum = 0; // �O��̍s���ԍ�
    private int _leftFrameSinceHit = 0; // �U���𓖂Ăāi�K�[�h�����āj����̎���
    private readonly int _assertiveness = 120; // �U���𓖂Ă悤�Ƃ���ϋɐ�(��������������)
    private int _leftFrameSinceChangeVector; // ����������ς��Ă���̎���
    private readonly int _frequencyWalkChange = 30; //���������̐؂�ւ��p�x

    public virtual void SetCAandCS()
    {
        InputReciever = GetComponent<FightingInputReceiver>();
        SelfCA = GetComponent<CharacterActions>();
        SelfCS = GetComponent<CharacterState>();
        EnemyCA = SelfCA.EnemyCA;
        EnemyCS = EnemyCA.GetComponent<CharacterState>();

        OnEpisodeStart();
    }

    protected virtual void OnEpisodeStart()
    {
        // �f���Q�[�g�̐ݒ�
        SelfCA.OnHurtAI += OnHurt;
        SelfCA.OnComboAI += OnCombo;
        SelfCA.OnDieAI += OnDie;
        SelfCA.OnBreakAI += OnBreak;
        SelfCA.OnMissAI += OnMiss;

        EnemyCA.OnHurtAI += OnHit;
        EnemyCA.OnGuardAI += OnGuarded;
        EnemyCA.OnDieAI += OnKill;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // �����̊�{�����ϑ�
        sensor.AddObservation(SelfCA.HurtBox.transform.position);
        sensor.AddObservation(SelfCS.CurrentHP / SelfCS.MaxHP);
        sensor.AddObservation(SelfCS.CurrentSP / SelfCS.MaxSP);
        sensor.AddObservation(SelfCS.CurrentUP / SelfCS.MaxUP);
        bool isSelfBroken = SelfCS.AnormalyStates.Contains(AnormalyState.Fatigue);
        sensor.AddObservation(isSelfBroken ? 1 : 0);

        // ����̊�{�����ϑ�
        sensor.AddObservation(EnemyCharaNum);
        sensor.AddObservation(EnemyCA.HurtBox.transform.position);
        sensor.AddObservation(EnemyCS.CurrentHP / EnemyCS.MaxHP);
        sensor.AddObservation(EnemyCS.CurrentSP / EnemyCS.MaxSP);
        bool isEnemyBroken = EnemyCS.AnormalyStates.Contains(AnormalyState.Fatigue);
        sensor.AddObservation(isEnemyBroken ? 1 : 0);
    }

    private void Update()
    {
        if (SelfCS == null) return;

        if(FightingPhysics.FightingTimeScale <= 0) return;

        if(SelfCS.AcceptOperations)
        {
            RequestDecision();

            //���t���[���J�E���g
            _leftFrameSinceHit++;
            _leftFrameSinceChangeVector++;
        }       
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int move = actions.DiscreteActions[0];

        switch (move)
        {
            case 0: InputReciever.SetWalkDirectionFromAI(0); break;
            case 1: InputReciever.SetWalkDirectionFromAI(-1f); break;
            case 2: InputReciever.SetWalkDirectionFromAI(1f); break;
            case 3: InputReciever.OnJump(); break;
            case 4: InputReciever.OnNomalMove(); break;
            case 5: InputReciever.OnSpecialMove1(); break;
            case 6: InputReciever.OnSpecialMove2(); break;
            case 7: InputReciever.OnUltimate(); break;
            case 8: InputReciever.SetGuardFromAI(true); break;
            case 9: InputReciever.SetGuardFromAI(false); break;
        }

        AddRewardEveryFrame(move);
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        for (int i = 3; i < _numActions; i++)
        {
            actionMask.SetActionEnabled(0, i, false);
        }

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
                _leftFrameSinceChangeVector = 0;
            }
        }

        //�ϋɓI�ɍU������悤��
        if(_leftFrameSinceHit < _assertiveness)
        {
            AddReward(-0.0005f);
        }

        _preMoveNum = currentMoveNum;
    }

    //�U�����������Ƃ�
    protected virtual void OnHurt()
    {
        AddReward(-0.75f);
    }

    //�U�����q�b�g�����Ƃ�
    protected virtual void OnHit()
    {
        AddReward(1f);
        _leftFrameSinceHit = 0;
    }

    //�U�����K�[�h�������Ƃ�
    protected virtual void OnGuarded()
    {
        AddReward(1f);
        _leftFrameSinceHit = 0;
    }

    //�U������U�肵���Ƃ�
    protected virtual void OnMiss()
    {
        AddReward(-0.75f);
        Debug.Log("CPU��U��");
    }

    //�R���{�����Ƃ�
    protected virtual void OnCombo()
    {
        AddReward(0.75f);
    }

    //���g��Break��ԂɂȂ����Ƃ�
    protected virtual void OnBreak()
    {
        AddReward(-1.5f);
    }

    //�|�����Ƃ�
    protected virtual void OnKill()
    {
        float reward = 2f;
        reward += (SelfCS.CurrentHP / SelfCS.MaxHP) * 1.5f;
        SetReward(reward);
        OnEpisodeEnd();
        EndEpisode();
    }

    //�|���ꂽ�Ƃ�
    protected virtual void OnDie()
    {
        float reward = -2f;
        reward -= (EnemyCS.CurrentHP/EnemyCS.MaxHP) * 1.5f;
        SetReward(reward);
        OnEpisodeEnd();
        EndEpisode();
    }

    private void OnEpisodeEnd()
    {
        float totalReward = GetCumulativeReward();

        Debug.Log($"Player{SelfCA.PlayerNum}����V:{totalReward}���l��");

        //�f���Q�[�g�̏�����
        SelfCA.OnHurtAI = null;
        SelfCA.OnComboAI = null;
        SelfCA.OnDieAI = null;
        SelfCA.OnGuardAI = null;
        SelfCA.OnBreakAI = null;
        SelfCA.OnMissAI = null;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // �J���p�Ƀe�X�g������L�q
    }
}
