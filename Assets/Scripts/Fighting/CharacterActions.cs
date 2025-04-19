using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Events;
using System.Linq;


public abstract class CharacterActions : FightingRigidBody
{
    [Space]
    [Header("�L�����N�^�[�̐ݒ�")]
    [SerializeField] private GameObject _hurtBox;

    //��{���
    private readonly float _spGainSpeed = 0.2f; // SP�̎��R�񕜗�

    //�R���|�[�l���g
    protected FightingInputReceiver _inputReciever { get; private set; }
    protected CharacterState _characterState { get; private set; }
    protected Animator _animator { get; private set; }

    //���̑��v���p�e�B
    public CharacterActions EnemyCA { get; private set; }
    public int PlayerNum { get; private set; } = 0;

    //�d�����ł̍s�������v���p�e�B
    protected virtual bool CanEveryAction
    {
        get
        {
            return _characterState.AcceptOperations
                && !_characterState.IsRecoveringHit
                && !_characterState.IsRecoveringGuard
                && _IsCompleteLandStun
                && !_characterState.AnormalyStates.Contains(AnormalyState.Bind)
                && !_characterState.AnormalyStates.Contains(AnormalyState.Dead);
        }
    }
    protected virtual bool CanWalk
    {
        get
        {
            return CanEveryAction
                && !_characterState.IsGuarding;
        }
    }
    protected virtual bool CanJump
    {
        get
        {
            return CanEveryAction
                && !_characterState.IsGuarding;
        }
    }
    protected virtual bool CanHit
    {
        get
        {
            return !_characterState.AnormalyStates.Contains(AnormalyState.Dead)
                && _characterState.AcceptOperations;
        }
    }
    protected virtual bool CanGuard
    {
        get
        {
            return CanEveryAction
                && !_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)
                && OnGround;
        }
    }

    //�W�����v�d��
    private bool _IsCompleteLandStun = true;

    //UI�֘A�f���Q�[�g
    public delegate UniTask ComboCountDelegate(int playerNum, int comboNum);
    public ComboCountDelegate ComboCount { get; set; }

    //�G�t�F�N�g�֘A�f���Q�[�g
    public UnityAction<Vector2, FightingEffect> OnEffect { get; set; }

    //���K�E�Z���o
    public UnityAction<Vector2, float, int> PerformUltimate { get; set; }

    //���S
    public UnityAction<int> OnDie { get; set; }

    //AI�w�K�p�f���Q�[�g
    public UnityAction OnHurtAI { get; set; }
    public UnityAction OnDieAI { get; set; }
    public UnityAction OnGuardAI { get; set; } 
    public UnityAction OnBreakAI { get; set; }
    public UnityAction OnComboAI { get; set; }


    protected override void Awake()
    {
        tag = "Character";

        //cs�̏�����
        if (_characterState == null) _characterState = GetComponent<CharacterState>();
        _characterState.Break = Break;
        _characterState.RecoverBreak = RecoverBreak;

        if (_animator == null) _animator = GetComponent<Animator>();

        //���̓f���Q�[�g�̐ݒ�
        if (_inputReciever = GetComponent<FightingInputReceiver>())
        {
            SetActionDelegate();
        }

        //�����蔻��̐ݒ�
        SetHitBox();

        base.Awake();
    }

    /// <summary>
    /// InputReceiver�̃f���Q�[�g������������
    /// </summary>
    protected abstract void SetActionDelegate();

    /// <summary>
    /// �e�U�����̓����蔻��̏�����
    /// </summary>
    protected abstract void SetHitBox();

    /// <summary>
    /// ��Ԃ̏�����
    /// </summary>
    public virtual void InitializeCA(int playerNum, CharacterActions enemyCA)
    {
        PlayerNum = playerNum;
        EnemyCA = enemyCA;
        Debug.Log($"�v���C���[�ԍ�{PlayerNum}");

        Velocity = Vector2.zero;

        //�p�����[�^������
        if (_characterState == null) _characterState = GetComponent<CharacterState>();
        _characterState.ResetState();

        //�F�����ɖ߂�
        GetComponent<SpriteRenderer>().color = Color.white;

        //���������킹
        if(playerNum == 1)
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
            _characterState.SetIsLeft(true);
        }
        else
        {
            transform.rotation = new Quaternion(0, 180, 0, 0);
            _characterState.SetIsLeft(false);
        }

        //�S�Ă�CTS���L�����Z��
        _characterState.CancelGuardStun();
        _characterState.CancelHitStun();
        CancelActionByHit();

        //Animator������
        if (_animator == null) _animator = GetComponent<Animator>();
        RuntimeAnimatorController controller = _animator.runtimeAnimatorController;
        _animator.runtimeAnimatorController = null;
        _animator.runtimeAnimatorController = controller;
    }

    protected override void FightingUpdate()
    {
        if (_characterState == null) return;

        //�K�[�h����SP�񕜂Ȃ�
        if (!_characterState.IsGuarding)
        {
            float spGainValue = _spGainSpeed;
            //Break��Ԃ͓�{�̑��x�ŉ񕜂���
            if (_characterState.AnormalyStates.Contains(AnormalyState.Fatigue))
            {
                spGainValue *= 2;
            }
            _characterState.SetCurrentSP(spGainValue);
        }

        //UP�Q�[�W���R����
        UPgain(_characterState.UPgainSpeed);

        //Animator�p�����[�^��ݒ�
        _animator.SetFloat("YspeedFloat", Velocity.y);

        //����
        Walk(_inputReciever.WalkValue);

        //�󒆂ɂ���Ƃ��A�j���[�V������؂�ւ���
        ChangeAnimatorLayer();

        //�K�[�h���͂���Ă���Ȃ�K�[�h
        GuardStance(_inputReciever.IsInputingGuard);

        //�X�^�~�i�A�E�g���K�[�h��������
        if(_characterState.AnormalyStates.Contains(AnormalyState.Fatigue)
            && _characterState.IsGuarding)
        {
            GuardRelease();
        }

        //�K�[�h��Ԃŋ󒆂ɂ���Ƃ��̓K�[�h������
        if(!OnGround && _characterState.IsGuarding)
        {
            GuardRelease();
        }

        if (EnemyCA != null)
        {
            //�L���������������킹��
            DirectionReversal();
        }
    }

    /// <summary>
    /// �L�������m�����������킹��
    /// </summary>
    private void DirectionReversal()
    {
        // �K�[�h���͐U�������
        if(!_characterState.IsGuarding)
        {
            //�ڒn���̂ݔ��]
            if (!CanWalk || !OnGround) return;
        }

        if (GetPushBackBox().center.x > EnemyCA.GetPushBackBox().center.x)
        {
            transform.rotation = new Quaternion(0, 180, 0, 0);
            _characterState.SetIsLeft(false);
        }
        else
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
            _characterState.SetIsLeft(true);
        }
    }

    /// <summary>
    /// Anmator��Layer��؂�ւ���
    /// </summary>
    private void ChangeAnimatorLayer()
    {
        // �󒆂Ȃ�AirLayer�ɐ؂�ւ���
        if(!OnGround)
        {
            AnimatorByLayerName.SetLayerWeightByName(_animator, "WalkLayer", 0);
            AnimatorByLayerName.SetLayerWeightByName(_animator, "AirLayer", 1);
        }
        else
        {
            AnimatorByLayerName.SetLayerWeightByName(_animator, "WalkLayer", 1);
            AnimatorByLayerName.SetLayerWeightByName(_animator, "AirLayer", 0);
        }
    }

    /// <summary>
    /// ���͂̕����ɂ���ăL�������ړ�������
    /// </summary>
    /// <param name="inputValue">�󂯎��������(1,-1,0�̂����ꂩ)</param>
    protected virtual void Walk(float inputValue)
    {
        if (!CanWalk || !OnGround || _characterState.IsRidenByEnemy)
        {
            _animator.SetFloat("WalkFloat", 0);
            return;
        }

        if(inputValue == 0)
        {
            Velocity = new Vector2(0, Velocity.y);
            _animator.SetFloat("WalkFloat", inputValue);
            return;
        }

        float speedPower = inputValue; //�L�����N�^�[�ɉ����鉡�����̗�

        //�L�����̌����Ɠ��͂ɑ��x�ƃA�j���[�V��������v������
        if (_characterState.IsLeftSide)
        {
            if (inputValue > 0)
            {
                speedPower *= _characterState.CurrentFrontSpeed;
            }
            else
            {
                speedPower *= _characterState.CurrentBackSpeed;
            }
            _animator.SetFloat("WalkFloat", inputValue);
        }
        else
        {
            if (inputValue > 0)
            {
                speedPower *= _characterState.CurrentBackSpeed;
            }
            else
            {
                speedPower *= _characterState.CurrentFrontSpeed;
            }
            _animator.SetFloat("WalkFloat", -inputValue);
        }

        Velocity = new Vector2(speedPower, Velocity.y);
    }

    /// <summary>
    /// �W�����v������
    /// </summary>
    protected virtual void Jump()
    {
        if (!CanJump) return;

        //�ڒn���̂݉\
        if (!OnGround) return;

        SetGround(false);

        //��������
        Vector2 jumpVector = new Vector2(0, _characterState.CurrentJumpPower);

        AddForce(jumpVector);

        //�A�j���[�V����
        _animator.SetTrigger("JumpTrigger");
    }

    /// <summary>
    /// �L�����N�^�[�Ƃ̐ڐG��FightingRigidBody����󂯎��
    /// </summary>
    protected override void PushBackCharacter(FightingRigidBody other)
    {
        //�d�Ȃ��Ă��镔���̒���
        float overlapLength;

        //�����o�����x
        float pushPower = 0.2f;

        //�ǂ��瑤�ɂ��邩
        bool isleftSide = GetPushBackBox().center.x < other.GetPushBackBox().center.x;

        if(isleftSide)
        {
            float enemyLeftPos = other.GetPushBackBox().xMin;
            float thisRightPos = GetPushBackBox().xMax;
            overlapLength = enemyLeftPos - thisRightPos;

            if(overlapLength < 0)
            {
                if (-overlapLength < pushPower)
                {
                    pushPower = -overlapLength;
                }

                other.transform.position += new Vector3(pushPower, 0, 0);
            }
        }
        else 
        {
            float enemyRightPos = other.GetPushBackBox().xMax;
            float thisLeftPos = GetPushBackBox().xMin;
            overlapLength = thisLeftPos - enemyRightPos;

            if (overlapLength < 0)
            {
                if (-overlapLength < pushPower)
                {
                    pushPower = -overlapLength;
                }

                other.transform.position -= new Vector3(pushPower, 0, 0);
            }
        }
    }

    protected override async void OnLand()
    {
        Velocity = Vector2.zero;
        _IsCompleteLandStun = false;
        await FightingPhysics.DelayFrameWithTimeScale(1);
        _IsCompleteLandStun = true;
        Land();
    }

    /// <summary>
    /// ���n�����Ƃ��ɌĂ΂�郁�\�b�h
    /// �W�����v�U�����L�����Z�������肷��p
    /// </summary>
    protected virtual void Land()
    {
        Debug.Log("���n");
    }

    /// <summary>
    /// �U�����󂯂����̏���
    /// </summary>
    /// /// <param name="attackInfo">�󂯂��U���̏��</param>
    public virtual async UniTask TakeAttack(AttackInfo attackInfo)
    {
        //�e��s���L�����Z��
        CancelActionByHit();

        //AI�w�K
        OnHurtAI?.Invoke();

        //�R���{����
        if (_characterState.IsRecoveringHit)
        {
            _characterState.CancelHitStun();
            _characterState.SetComboCount(_characterState.ConboCount + 1);
            OnComboAI?.Invoke();
            Debug.Log($"{_characterState.ConboCount}�R���{");

            //���o���f
            if(ComboCount != null)
            {
                ComboCount(PlayerNum, _characterState.ConboCount).Forget();
            }          
        }
        else
        {
            _characterState.SetComboCount(1);
        }

        //�G�t�F�N�g����
        Vector2 hurtBoxPos = transform.TransformPoint(_hurtBox.GetComponent<BoxCollider2D>().offset);
        if (attackInfo.IsHeavy)
        {
            OnEffect?.Invoke(hurtBoxPos, FightingEffect.LargeHit);
        }
        else
        {
            OnEffect?.Invoke(hurtBoxPos, FightingEffect.SmallHit);
        }

        //�_���[�W����
        if (CanHit)
        {
            _characterState.TakeDamage(attackInfo.Damage);
            Debug.Log($"Player{PlayerNum}��{attackInfo.Damage}�_���[�W�󂯂�");
            if (_characterState.CurrentHP <= 0)
            {
                Die();
                return;
            }
        }

        //�A�j���[�V��������
        AnimatorByLayerName.SetLayerWeightByName(_animator, "HurtLayer", 1); // Animator��Layer��HitLayer���ŗD��ɕύX
        _animator.SetTrigger("HurtTrigger"); // ��炢�A�j���[�V�����Đ�

        //�R���{�Z�o�^
        _characterState.AddAttackName(attackInfo);

        //�q�b�g�X�g�b�v����
        await HitStop(attackInfo.HitStopFrame);

        //�q�b�g�o�b�N����(Bind��Ԃł̓q�b�g�o�b�N���Ȃ�)
        if(!_characterState.AnormalyStates.Contains(AnormalyState.Bind))
        {
            Velocity = Vector2.zero;
            Vector2 hitBackVector = attackInfo.HitBackDirection;
            if (EnemyCA.GetPushBackBox().x > GetPushBackBox().center.x)
            {
                hitBackVector = attackInfo.HitBackDirection * new Vector2(-1, 1);
            }

            int sameAttackCount = _characterState.NameOfGivenAttack.Count(n => n == attackInfo.Name);

            //���Z�␳
            if (sameAttackCount != 0)
            {
                float correction = 1 / (float)sameAttackCount;

                //y�����ɕ����ɂ����Ȃ��Ă���
                hitBackVector *= new Vector2(1, correction);
                Debug.Log(hitBackVector);
            }
            AddForce(hitBackVector);
        }

        //�q�b�g�d������
        await HitStun(attackInfo.HitFrame, _characterState.CreateHitCT());

        AnimatorByLayerName.SetLayerWeightByName(_animator, "HurtLayer", 0); // Animator��Layer��HitLayer���ŗD�x�����ɖ߂�
    }

    /// <summary>
    /// �U�����󂯂����Ƃɂ��e�A�N�V�����̃L�����Z��
    /// </summary>
    public abstract void CancelActionByHit();

    /// <summary>
    /// �q�b�g�d���̏���
    /// </summary>
    /// <param name="recoveryFrame">�d������(�t���[��)</param>
    private async UniTask HitStun(int recoveryFrame, CancellationToken token)
    {
        await FightingPhysics.DelayFrameWithTimeScale(recoveryFrame, cancellationToken: token);

        //�o�C���h��Ԃł̓q�b�g�d������񕜂��Ȃ�
        await UniTask.WaitUntil(() => 
        !_characterState.AnormalyStates.Contains(AnormalyState.Bind), cancellationToken: token);

        //���n����܂Ńq�b�g�d��������
        await UniTask.WaitUntil(() => OnGround, cancellationToken: token);


        if (token.IsCancellationRequested)
        {
            Debug.Log("�q�b�g�d�����L�����Z�����ꂽ");
        }
        else
        {
            _characterState.ClearNameOfGivenAttack();
        }

        _characterState.CancelHitStun();
    }

    /// <summary>
    /// �q�b�g�X�g�b�v
    /// </summary>
    private async UniTask HitStop(int stopFrame)
    {
        Time.timeScale = 0;
        FightingPhysics.SetFightTimeScale(0);
        await UniTask.DelayFrame(stopFrame - 1);
        Time.timeScale = 1;
        FightingPhysics.SetFightTimeScale(1);
        await UniTask.Yield(); // 1�t���[���ҋ@���đ��x�K�p��ۏ�
    }

    /// <summary>
    /// �K�[�h�\��������
    /// </summary>
    protected virtual void GuardStance(bool isGuarding)
    {
        if (!CanGuard) return;

        // Layer��ύX
        if (isGuarding)
        {
            AnimatorByLayerName.SetLayerWeightByName(_animator, "GuardLayer", 1);
            //������SP����
            if(!_characterState.IsGuarding)
            {
                _characterState.SetCurrentSP(-10);
            }
            _characterState.SetIsGuarding(true);
        }
        else
        {
            //�K�[�h����
            GuardRelease();
        }
    }

    /// <summary>
    /// �K�[�h����
    /// </summary>
    private void GuardRelease()
    {
        AnimatorByLayerName.SetLayerWeightByName(_animator, "GuardLayer", 0);
        _characterState.SetIsGuarding(false);
    }

    /// <summary>
    /// �U�����K�[�h�������̏���
    /// </summary>
    /// <param name="attackInfo">�󂯂��U���̏��</param>
    public virtual async UniTask Guard(AttackInfo attackInfo)
    {
        //AI�w�K
        OnGuardAI?.Invoke();

        //�K�[�h�o�b�N����
        Velocity = Vector2.zero;
        Vector2 guardBackVector = attackInfo.GuardBackDirection;
        if (EnemyCA.GetPushBackBox().x > GetPushBackBox().center.x)
        {
            guardBackVector *= new Vector2(-1, 1);
        }
        AddForce(guardBackVector);

        //����̂̂�����
        EnemyCA.GuardedBack(attackInfo);

        //�G�t�F�N�g����
        Vector2 hurtBoxPos = transform.TransformPoint(_hurtBox.GetComponent<BoxCollider2D>().offset);
        OnEffect?.Invoke(hurtBoxPos, FightingEffect.Guard);

        //SP���
        _characterState.SetCurrentSP(-attackInfo.DrainSP);

        //�A���K�[�h����
        if(_characterState.IsRecoveringGuard)
        {
            //�O��̍d��������
            _characterState.CancelGuardStun();
        }

        await GuardStun(attackInfo.GuardFrame, _characterState.CreateGuardCT());
    }

    private async UniTask GuardStun(int guardFrame, CancellationToken token)
    {
        await FightingPhysics.DelayFrameWithTimeScale(guardFrame, cancellationToken: token);

        if (token.IsCancellationRequested)
        {
            Debug.Log("�K�[�h�d�����L�����Z�����ꂽ");
            _characterState.CancelGuardStun();
            return;
        }

        _characterState.CancelGuardStun();
    }

    /// <summary>
    /// �U�����K�[�h���ꂽ�Ƃ��̂̂�����
    /// </summary>
    private void GuardedBack(AttackInfo attackInfo)
    {
        Vector2 guardedBackVector = attackInfo.GuardedBackDirection;
        if (EnemyCA.GetPushBackBox().x > GetPushBackBox().center.x)
        {
            guardedBackVector *= new Vector2(-1, 1);
        }
        AddForce(guardedBackVector);
    }

    /// <summary>
    /// Break��ԂɂȂ�
    /// </summary>
    protected virtual void Break()
    {
        _characterState.TakeAnormalyState(AnormalyState.Fatigue);
        OnEffect?.Invoke(GetPushBackBox().center, FightingEffect.Break);
        GetComponent<SpriteRenderer>().color -= new Color(0.3f, 0.3f, 0, 0);
        OnBreakAI?.Invoke();
    }

    /// <summary>
    /// Break��Ԃ����
    /// </summary>
    protected virtual void RecoverBreak()
    {
        _characterState.RecoverAnormalyState(AnormalyState.Fatigue);
        OnEffect?.Invoke(GetPushBackBox().center, FightingEffect.RecoverBreak);
        GetComponent<SpriteRenderer>().color += new Color(0.3f, 0.3f, 0, 0);
    }

    /// <summary>
    /// UP����
    /// </summary>
    public virtual void UPgain(float value)
    {
        _characterState.SetCurrentUP(value);
    }

    /// <summary>
    /// ���S����
    /// HP��0�ȉ��ɂȂ�ƌĂ΂��
    /// </summary>
    protected virtual void Die() 
    {
        _characterState.TakeAnormalyState(AnormalyState.Dead);
        AnimatorByLayerName.SetLayerWeightByName(_animator, "DieLayer", 1);
        _animator.SetBool("DieBool", true);
        OnDie?.Invoke(PlayerNum);
        OnDieAI?.Invoke();
    }

    /// <summary>
    /// ���͂̕���
    /// </summary>
    /// <returns>0:���͂Ȃ�, 1:�O, -1:���</returns>
    protected int WalkDirection()
    {
        int direction;

        if(GetPushBackBox().center.x <= EnemyCA.GetPushBackBox().center.x)
        {
            direction = (int)_inputReciever.WalkValue;
        }
        else
        {
            direction = (int)-_inputReciever.WalkValue;
        }

        return direction;
    }
}
