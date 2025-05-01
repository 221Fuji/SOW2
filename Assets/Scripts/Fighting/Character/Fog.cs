using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// CloudのSm1
/// </summary>
public class Fog : FightingRigidBody
{
    [SerializeField] private float _speed;
    [SerializeField] private float _consumptionValue;
    [SerializeField] private float _damage;
    [SerializeField] private LayerMask _hurtBoxLayer;
    [SerializeField] private Vector2 _fogBox;
    [SerializeField] private Vector2 _boxOffset;

    private GameObject _self;
    private CancellationTokenSource _destroyCTS;

    public bool IsActive { get; private set; }
    public int PlayerNum 
    {
        get { return _self.GetComponent<CharacterActions>().PlayerNum; }
    }

    public void InitializeFog(bool isLeftSide, GameObject self)
    {
        _self = self;

        float velocityX = _speed;
        if(!isLeftSide)
        {
            transform.localScale *= new Vector2(-1, 1); 
            velocityX *= -1;
        }
        Velocity = new Vector2(velocityX, 0);
    }

    public void SetIsActive(bool value)
    {
        IsActive = value;
    }

    protected override void OnWall(FightingRigidBody other)
    {
        //壁はすり抜ける
    }

    protected override void FightingUpdate()
    {
        // 持続中かどうか
        if (!IsActive) return;
        HitCheck();

        //リソース減少
        if(_self.TryGetComponent<ViolaCloud>(out var cloud))
        {
            cloud.ConsumptionFogResource(_consumptionValue);
        }
    }

    private void HitCheck()
    {
        //当たり判定に面積がない場合無効にする
        if (_fogBox.x == 0 || _fogBox.y == 0) return;

        Collider2D[] colliders = Physics2D.OverlapBoxAll
            ((Vector2)transform.position + _boxOffset, _fogBox, transform.rotation.z, _hurtBoxLayer);

        foreach (Collider2D collider in colliders)
        {
            //自身には当たらない
            if (collider.transform.parent == _self.transform) continue;

            GameObject enemy = collider.transform.parent.gameObject;
            CharacterState enemyCS = enemy.GetComponent<CharacterState>();

            if (enemyCS.CurrentHP - _damage > 2)
            {
                enemyCS.TakeDamage(_damage);
            }
            else if (enemyCS.CurrentHP - _damage > 0)
            {
                enemyCS.TakeDamage(enemyCS.CurrentHP - 2);
            }
        }
    }

    /// <summary>
    /// 自身が煙霧の中かどうか
    /// </summary>
    /// <returns></returns>
    public bool IsInSelf()
    {
        if(_destroyCTS != null) return false;

        Collider2D[] colliders = Physics2D.OverlapBoxAll
            ((Vector2)transform.position + _boxOffset, _fogBox, transform.rotation.z, _hurtBoxLayer);

        foreach (Collider2D collider in colliders)
        {
            if(collider.transform.parent == _self.transform) return true;
        }

        return false;
    }

    public async UniTask DestroyFog()
    {
        SetIsActive(false);
        _destroyCTS = new CancellationTokenSource();

        Animator animtor = GetComponent<Animator>();
        animtor.SetTrigger("EndFogTrigger");

        await UniTask.WaitUntil(() =>
        {
            if (animtor == null) return true;
            var startStateInfo = animtor.GetCurrentAnimatorStateInfo(0);
            return startStateInfo.IsName("FogEnd") && startStateInfo.normalizedTime >= 1f;
        }, cancellationToken: _destroyCTS.Token);

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube((Vector2)transform.position + _boxOffset, _fogBox);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(_destroyCTS != null)
        {
            _destroyCTS.Cancel();
        }
    }
}
