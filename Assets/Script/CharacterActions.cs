using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CharacterActions : MonoBehaviour
{
    //�R���|�[�l���g
    protected Rigidbody2D _rb { get; private set; }
    protected FightingInputReceiver _inputReciever { get; private set; }
    protected CharacterState _characterState { get; private set; }
    protected Animator _animator { get; private set; }

    //���ʕϐ�
    protected GameObject _enemyObject { get; private set; }

    //�d�����ł̍s�������v���p�e�B
    protected virtual bool CanWalk { get; set; }
    protected virtual bool CanJump { get; set; }
    protected virtual bool _canHit { get; set; }


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _characterState = GetComponent<CharacterState>();
        _animator = GetComponent<Animator>();

        if (_inputReciever = GetComponent<FightingInputReceiver>())
        {
            SetActionDelegate();
        }

        SetHitBox();
    }

    /// <summary>
    /// InputReceiver�̃f���Q�[�g������������
    /// </summary>
    protected virtual void SetActionDelegate()
    {
        _inputReciever.JumpDelegate = Jump;
    }

    /// <summary>
    /// �e�U�����̓����蔻��̏�����
    /// </summary>
    protected virtual void SetHitBox()
    {
        Debug.Log("���̃L�����N�^�[�͓����蔻��̂���U��������܂���");
    }

    /// <summary>
    /// �G����ݒ肷��
    /// </summary>
    public void SetEnemy(GameObject enemyObject)
    {
        _enemyObject = enemyObject;
    }

    private void Update()
    {
        if(_inputReciever == null) return;

        //�q�b�g�d�����͈ȉ��̍s�����ł��Ȃ�
        if (_characterState.IsRecoveringHit) return;

        //Animator�p�����[�^��ݒ�
        _animator.SetFloat("YspeedFloat", _rb.velocity.y);

        //����
        Walk(_inputReciever._WalkValue);

        //�󒆂ɂ���Ƃ��A�j���[�V������؂�ւ���
        ChangeAnimatorLayer();

        if (_enemyObject == null) return;

        //�L���������������킹��
        DirectionReversal();
    }

    /// <summary>
    /// �L�������m�����������킹��
    /// </summary>
    protected virtual void DirectionReversal()
    {
        //�ڒn���̂ݔ��]
        if (!CanWalk && !_characterState.OnGround) return;

        if (transform.position.x > _enemyObject.transform.position.x)
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
    protected virtual void ChangeAnimatorLayer()
    {
        // �󒆂Ȃ�AirLayer�ɐ؂�ւ���
        if(!_characterState.OnGround)
        {
            SetLayerWeightByName("WalkLayer", 0);
            SetLayerWeightByName("AirLayer", 1);
        }
        else
        {
            SetLayerWeightByName("WalkLayer", 1);
            SetLayerWeightByName("AirLayer", 0);
        }
    }

    /// <summary>
    /// ���͂̕����ɂ���ăL�������ړ�������
    /// </summary>
    /// <param name="inputValue">�󂯎��������(1,-1,0�̂����ꂩ)</param>
    protected virtual void Walk(float inputValue)
    {
        if (!CanWalk) return;
        float speed = inputValue;

        //�L�����̌����Ɠ��͂ɑ��x�ƃA�j���[�V��������v������
        if (_characterState.IsLeftSide)
        {
            if (inputValue > 0)
            {
                speed *= _characterState.CurrentFrontSpeed;
            }
            else
            {
                speed *= _characterState.CurrentBackSpeed;
            }
            _animator.SetFloat("WalkFloat", inputValue);
        }
        else
        {
            if (inputValue > 0)
            {
                speed *= _characterState.CurrentBackSpeed;
            }
            else
            {
                speed *= _characterState.CurrentFrontSpeed;
            }
            _animator.SetFloat("WalkFloat", -inputValue);
        }

        _rb.velocity = new Vector2(speed, _rb.velocity.y);
    }

    /// <summary>
    /// �W�����v������
    /// </summary>
    protected virtual void Jump()
    {
        if (!CanJump) return;

        //�ڒn���̂݉\
        if (!_characterState.OnGround) return;

        //��������
        Vector2 jumpVector = new Vector2(_rb.velocity.x, _characterState.CurrentJumpPower);
        _rb.AddForce(jumpVector, ForceMode2D.Impulse);

        //�A�j���[�V����
        _animator.SetTrigger("JumpTrigger");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            Land();
        }
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
        Debug.Log("�U�����󂯂��I");

        //�R���{����
        if (_characterState.IsRecoveringHit)
        {
            _characterState.CancelHitStun();
            _characterState.SetComboCount(_characterState.ConboCount + 1);
            Debug.Log($"{_characterState.ConboCount}�R���{");
        }
        else
        {
            _characterState.SetComboCount(1);
        }

        //�q�b�g�o�b�N����
        _rb.velocity = Vector2.zero;
        Vector2 jumpVector;
        if (_characterState.IsLeftSide)
        {
            jumpVector = attackInfo.HitBackDirection * new Vector2(-1, 1);
        }
        else
        {
            jumpVector = attackInfo.HitBackDirection;
        }
        _rb.AddForce(jumpVector, ForceMode2D.Impulse);

        //�A�j���[�V��������
        SetLayerWeightByName("HitLayer", 1); // Animator��Layer��HitLayer���ŗD��ɕύX
        _animator.SetTrigger("HitTrigger"); // ��炢�A�j���[�V�����Đ�

        //�q�b�g�d������
        Debug.Log($"�q�b�g�d���J�n:{attackInfo.HitFrame}");
        await HitStun(attackInfo.HitFrame, _characterState.CreateHitCT());

        SetLayerWeightByName("HitLayer", 0); // Animator��Layer��HitLayer���ŗD�x�����ɖ߂�
    }

    /// <summary>
    /// �q�b�g�d���̏���
    /// </summary>
    /// <param name="recoveryFrame">�d������(�t���[��)</param>
    public virtual async UniTask HitStun(int recoveryFrame, CancellationToken token)
    {
        await UniTask.DelayFrame(recoveryFrame, cancellationToken: token);

        if(token.IsCancellationRequested)
        {
            Debug.Log("�q�b�g�d�����L�����Z�����ꂽ");
            _characterState.CancelHitStun();
            return;
        }

        //���n����܂Ńq�b�g�d��������
        await UniTask.WaitUntil(() => _characterState.OnGround);

        _characterState.CancelHitStun();
    }

    /// <summary>
    /// Animator��SetlayerWeight��layer�̖��O�ŌĂяo��
    /// </summary>
    protected void SetLayerWeightByName(string layerName, float weight)
    {
        int layerIndex = _animator.GetLayerIndex(layerName);
        if (layerIndex != -1)
        {
            _animator.SetLayerWeight(layerIndex, weight);
        }
        else
        {
            Debug.LogWarning($"���C���[�� '{layerName}' ��������܂���ł���");
        }
    }
}
