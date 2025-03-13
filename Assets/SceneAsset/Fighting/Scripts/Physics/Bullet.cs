using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : FightingRigidBody
{
    [Header("�n�ʃo�E���h���邩")]
    [SerializeField] private bool _bound;
    [SerializeField] private Vector2 _boundVelocity;
    [SerializeField] private HitBoxManager _hitBox;

    public HitBoxManager HitBox { get => _hitBox; }

    //�e�폜�f���Q�[�g
    public UnityAction<GameObject> DestroyBullet { get; private set; }

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
            DestroyBullet?.Invoke(gameObject);
        }
    }

    protected override void OnWall(FightingRigidBody other)
    {
        //�ǂ͂��蔲����
    }

    /// <summary>
    /// Succubus�̒n�ʃo�E���h
    /// </summary>
    protected override void LandGround()
    {
        if (!_bound) return;
        base.LandGround();
        Velocity = _boundVelocity;
    }
}
