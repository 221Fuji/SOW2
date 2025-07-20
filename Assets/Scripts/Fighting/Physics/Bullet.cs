using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : FightingRigidBody
{
    [Header("地面バウンドするか")]
    [SerializeField] private bool _bound;
    [SerializeField] private Vector2 _boundVelocity;
    [Space]
    [Header("地面と衝突後消滅するか")]
    [SerializeField] private bool _isExploded;
    [Space]
    [SerializeField] private HitBoxManager _hitBox;

    public HitBoxManager HitBox { get => _hitBox; }

    //弾削除デリゲート
    public UnityAction<Bullet> DestroyBullet { get; set; }

    public Vector2 BoundVelocity
    {
        get {  return _boundVelocity; }
    }

    public void SetBoundVelocity(Vector2 value)
    {
        _boundVelocity = value;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
        
        if(transform.position.x > 5 + StageParameter.StageLength / 2
            || transform.position.x < -5 - StageParameter.StageLength / 2)
        {
            DestroyBullet?.Invoke(this);
        }

        //地面に着弾した時の処理
        if (!_isExploded) return;
        if(_hitBox.transform.position.y <= StageParameter.GroundPosY)
        {
            _isExploded = false;
            DestroyBullet?.Invoke(this);
        }

    }

    protected override void OnWall(FightingRigidBody other)
    {
        //壁はすり抜ける
    }

    /// <summary>
    /// Succubusの地面バウンド
    /// </summary>
    protected override void LandGround()
    {
        if (_bound)
        {
            base.LandGround();
            Velocity = _boundVelocity;
            return;
        }
    }
}
