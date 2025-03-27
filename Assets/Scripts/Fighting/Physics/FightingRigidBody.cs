using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FightingRigidBody : MonoBehaviour
{
    [Header("�d�͂̉e���x")]
    [SerializeField] private float _gravityScale;
    [Header("������������̑傫��")]
    [SerializeField] private Vector2 _pushBackBoxSize;
    [Header("������������̃I�t�Z�b�g")]
    [SerializeField] private Vector2 _pushBackBoxOffset;
    [Header("�Œ�")]
    [SerializeField] private bool _isFixed;
    [Header("�J�x��")]
    [SerializeField] private bool _isWall;

    private static List<FightingRigidBody> _fightingRigidBodies;

    private Vector2 _velocity;
    private Vector2 _currentPosition;
    private Vector2 _previousPosition;

    private CancellationTokenSource _fightingUpdateCTS;

    /// <summary>
    /// ������������
    /// </summary>
    public Vector2 PushBackBoxSize
    { 
        get => _pushBackBoxSize; 
        private set => _pushBackBoxSize = value;
    }
    public Vector2 PushBackBoxOffset
    {
        get => _pushBackBoxOffset;
        private set => _pushBackBoxOffset = value;
    }
    /// <summary>
    /// ���x��ύX����
    /// </summary>
    public Vector2 Velocity
    {
        get 
        { 
            if(Time.deltaTime == 0)
            {
                return PreviousVelocity;
            }

            Vector2 velocity = (_currentPosition - _previousPosition) / Time.deltaTime;
            PreviousVelocity = velocity; //���x��ۑ�
            return velocity; 
        }
        set { _velocity = value; }
    }
    public Vector2 PreviousVelocity
    {
        get; private set;
    }
    /// <summary>
    /// �ڒn
    /// </summary>
    public bool OnGround
    {
        get; private set;
    }
    /// <summary>
    /// �Œ艻
    /// </summary>
    public void SetIsFixed(bool value)
    {
        _isFixed = value;
    }

    public void SetIsWall(bool value)
    {
        _isWall = value;
    }
    
    public void SetGravityScale(float value)
    {
        _gravityScale = value;
    }

    protected virtual void Awake()
    {
        _fightingUpdateCTS = new CancellationTokenSource();
        StartFrameLoop(_fightingUpdateCTS.Token).Forget();

        if (_fightingRigidBodies == null)
        {
            _fightingRigidBodies = new List<FightingRigidBody>();
        }
        _fightingRigidBodies.Add(this);
    }

    protected virtual void Update()
    {

        if (_isWall) return;

        if (!OnGround && !_isFixed)
        {
            //�d��
            _velocity += new Vector2(0, -FightingPhysics.GravityAcceleration * _gravityScale * Time.deltaTime);
        }

        //�ړ��O�̍��W���L�^
        _previousPosition = transform.position;

        //���x�ɉ����č��W���X�V
        Vector2 nextPos = transform.position + (Vector3)_velocity * Time.deltaTime * FightingPhysics.FightingTimeScale;
        transform.position = nextPos;

        //��������������m�F
        CheckPushBackCollision();

        //�ړ���̍��W���L�^
        _currentPosition = transform.position;

        //���C���v�Z
        Friction();
    }

    private void LateUpdate()
    {
        //���W�̌Œ艻
        if (_isFixed && !_isWall)
        {
            transform.position = _previousPosition;
        }
    }

    private async UniTaskVoid StartFrameLoop(CancellationToken token)
    {
        if (_isWall) return;

        while (!token.IsCancellationRequested)
        {
            FightingUpdate();
            await FightingPhysics.DelayFrameWithTimeScale(1, token);
        }
    }

    protected virtual void FightingUpdate()
    {
        //�������L�q
    }

    /// <summary>
    /// ������������̃`�F�b�N
    /// </summary>
    private void CheckPushBackCollision()
    {
        // ������������̗̈�
        Rect myBox = GetPushBackBox();

        //�ꎞ�I�ȃt���O
        bool onGround = false;

        foreach (var other in _fightingRigidBodies)
        {
            if (other == this) continue; // �������g�����O

            Rect otherBox = other.GetPushBackBox();

            // ��`���m�̏Փ˔���
            if (myBox.Overlaps(otherBox))
            {
                OnPushBackCollision(other);
            }
        }

        //�ڒn����
        if(myBox.yMin <= StageParameter.GroundPosY)
        {
            LandGround();
            onGround = true;
        }

        SetGround(onGround);
    }

    /// <summary>
    /// ������������̋�`���擾
    /// </summary>
    public Rect GetPushBackBox()
    {
        Vector2 pushBackBoxOffset = _pushBackBoxOffset;

        if(transform.rotation.y > 0)
        {
            pushBackBoxOffset *= new Vector2(-1, 1);
        }

        Vector2 boxCenter = (Vector2)transform.position + pushBackBoxOffset;
        Vector2 boxMin = boxCenter - _pushBackBoxSize / 2;
        return new Rect(boxMin, _pushBackBoxSize);
    }

    /// <summary>
    /// ������������ɐN�������Ƃ��ɌĂ΂��R�[���o�b�N
    /// </summary>
    /// <param name="other">�N����������FightingRigidBody</param>
    protected virtual void OnPushBackCollision(FightingRigidBody other)
    {
        //�L�����N�^�[�Ƃ̐ڐG
        if(other.gameObject.tag == "Character")
        {
            PushBackCharacter(other);
        }

        //�ǂƂ̐ڐG
        if(other._isWall)
        {
            OnWall(other);
        }
    }

    /// <summary>
    /// �L�����N�^�[�̉�����̐ڐG
    /// </summary>
    protected virtual void PushBackCharacter(FightingRigidBody other)
    {
        //�L�����N�^�[�ƐڐG�����Ƃ��̏���
    }

    /// <summary>
    /// �ڒn����
    /// </summary>
    protected virtual async void LandGround()
    {
        float thisBottomPos = GetPushBackBox().yMin;
        float overlapLength = StageParameter.GroundPosY - thisBottomPos;

        //�኱�߂荞�܂����ʒu�ɕ␳
        transform.position += new Vector3(0, overlapLength, 0);
        _velocity = new Vector2(_velocity.x, 0);

        if(!OnGround)
        {
            await UniTask.Yield();
        }

        OnGround = true;
    }

    /// <summary>
    /// �ǂƂ̐ڐG
    /// </summary>
    protected virtual void OnWall(FightingRigidBody other)
    {
        if (other.gameObject.tag == "RightWall")
        {
            float wallLeftPos = other.GetPushBackBox().xMin;
            float thisRightPos = GetPushBackBox().xMax;
            float overlapLength = wallLeftPos - thisRightPos;

            if (overlapLength < 0)
            {
                transform.position += new Vector3(overlapLength, 0, 0);
            }
        }
        if (other.gameObject.tag == "LeftWall")
        {
            float wallRightPos = other.GetPushBackBox().xMax;
            float thisLeftPos = GetPushBackBox().xMin;
            float overlapLength = wallRightPos - thisLeftPos;

            if (overlapLength > 0)
            {
                transform.position += new Vector3(overlapLength, 0, 0);
            }
        }
    }

    /// <summary>
    /// ���C�ɂ�鑬�x����
    /// </summary>
    private void Friction()
    {
        if (!OnGround || Velocity.x == 0 || _gravityScale <= 0) return;

        if(Mathf.Abs(Velocity.x) <= FightingPhysics.FrictionCoefficient)
        {
            _velocity = Vector2.zero;
        }
        else
        {
            float frictionPower = FightingPhysics.FrictionCoefficient;
            if (_velocity.x > 0)
            {
                frictionPower *= -1; 
            }
            _velocity += new Vector2 (frictionPower, 0);
        }  
    }

    public void AddForce(Vector2 forceDirection)
    {
        _velocity += forceDirection;
    }

    public void SetGround(bool value)
    {
        OnGround = value;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0, 0.5f);
        Gizmos.DrawCube(GetPushBackBox().center, _pushBackBoxSize);

        Gizmos.color = Color.yellow;
        Vector2 rightEdge = new Vector2(StageParameter.StageLength / 2, StageParameter.GroundPosY);
        Vector2 leftEdge = new Vector2(-StageParameter.StageLength / 2, StageParameter.GroundPosY);
        Gizmos.DrawLine(rightEdge, leftEdge);
    }

    protected virtual void OnDestroy()
    {
        if(_fightingRigidBodies.Contains(this))
        {
            _fightingRigidBodies.Remove(this);
        }

        if (_fightingUpdateCTS != null)
        {
            _fightingUpdateCTS.Cancel();
        }
    }
}
