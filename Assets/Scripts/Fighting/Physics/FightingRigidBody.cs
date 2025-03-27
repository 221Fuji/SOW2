using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FightingRigidBody : MonoBehaviour
{
    [Header("重力の影響度")]
    [SerializeField] private float _gravityScale;
    [Header("押し合い判定の大きさ")]
    [SerializeField] private Vector2 _pushBackBoxSize;
    [Header("押し合い判定のオフセット")]
    [SerializeField] private Vector2 _pushBackBoxOffset;
    [Header("固定")]
    [SerializeField] private bool _isFixed;
    [Header("カベか")]
    [SerializeField] private bool _isWall;

    private static List<FightingRigidBody> _fightingRigidBodies;

    private Vector2 _velocity;
    private Vector2 _currentPosition;
    private Vector2 _previousPosition;

    private CancellationTokenSource _fightingUpdateCTS;

    /// <summary>
    /// 押し合い判定
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
    /// 速度を変更する
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
            PreviousVelocity = velocity; //速度を保存
            return velocity; 
        }
        set { _velocity = value; }
    }
    public Vector2 PreviousVelocity
    {
        get; private set;
    }
    /// <summary>
    /// 接地
    /// </summary>
    public bool OnGround
    {
        get; private set;
    }
    /// <summary>
    /// 固定化
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
            //重力
            _velocity += new Vector2(0, -FightingPhysics.GravityAcceleration * _gravityScale * Time.deltaTime);
        }

        //移動前の座標を記録
        _previousPosition = transform.position;

        //速度に応じて座標を更新
        Vector2 nextPos = transform.position + (Vector3)_velocity * Time.deltaTime * FightingPhysics.FightingTimeScale;
        transform.position = nextPos;

        //押し合い判定を確認
        CheckPushBackCollision();

        //移動後の座標を記録
        _currentPosition = transform.position;

        //摩擦を計算
        Friction();
    }

    private void LateUpdate()
    {
        //座標の固定化
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
        //処理を記述
    }

    /// <summary>
    /// 押し合い判定のチェック
    /// </summary>
    private void CheckPushBackCollision()
    {
        // 押し合い判定の領域
        Rect myBox = GetPushBackBox();

        //一時的なフラグ
        bool onGround = false;

        foreach (var other in _fightingRigidBodies)
        {
            if (other == this) continue; // 自分自身を除外

            Rect otherBox = other.GetPushBackBox();

            // 矩形同士の衝突判定
            if (myBox.Overlaps(otherBox))
            {
                OnPushBackCollision(other);
            }
        }

        //接地判定
        if(myBox.yMin <= StageParameter.GroundPosY)
        {
            LandGround();
            onGround = true;
        }

        SetGround(onGround);
    }

    /// <summary>
    /// 押し合い判定の矩形を取得
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
    /// 押し合い判定に侵入したときに呼ばれるコールバック
    /// </summary>
    /// <param name="other">侵入した他のFightingRigidBody</param>
    protected virtual void OnPushBackCollision(FightingRigidBody other)
    {
        //キャラクターとの接触
        if(other.gameObject.tag == "Character")
        {
            PushBackCharacter(other);
        }

        //壁との接触
        if(other._isWall)
        {
            OnWall(other);
        }
    }

    /// <summary>
    /// キャラクターの横からの接触
    /// </summary>
    protected virtual void PushBackCharacter(FightingRigidBody other)
    {
        //キャラクターと接触したときの処理
    }

    /// <summary>
    /// 接地処理
    /// </summary>
    protected virtual async void LandGround()
    {
        float thisBottomPos = GetPushBackBox().yMin;
        float overlapLength = StageParameter.GroundPosY - thisBottomPos;

        //若干めり込ませた位置に補正
        transform.position += new Vector3(0, overlapLength, 0);
        _velocity = new Vector2(_velocity.x, 0);

        if(!OnGround)
        {
            await UniTask.Yield();
        }

        OnGround = true;
    }

    /// <summary>
    /// 壁との接触
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
    /// 摩擦による速度減衰
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
