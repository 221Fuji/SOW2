using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPhysics : FightingRigidBody
{
    [Header("�n�ʃo�E���h���邩")]
    [SerializeField] private bool _bound;
    [SerializeField] private Vector2 _boundVelocity;

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
        
        if(transform.position.x > 10 + StageParameter.StageLength / 2
            || transform.position.x < -10 - StageParameter.StageLength / 2)
        {
            Destroy(gameObject);
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
