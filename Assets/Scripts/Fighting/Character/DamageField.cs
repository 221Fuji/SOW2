using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

/// <summary>
/// 持続ダメージエリア
/// </summary>
public class DamageField<TCharacterActions> : FightingRigidBody where TCharacterActions : CharacterActions
{
    [SerializeField] private float _speed;
    [SerializeField] private float _damage;
    [SerializeField] private LayerMask _hurtBoxLayer;
    [SerializeField] private Vector2 _fieldBox;
    [SerializeField] private Vector2 _boxOffset;

    protected TCharacterActions _self;
    private CancellationTokenSource _destroyCTS;
    private const float LOWEST_HP = 2;

    public bool IsActive { get; private set; }
    public int PlayerNum 
    {
        get { return _self.PlayerNum; }
    }

    private CharacterState _enemyCS;

    public void Initialize(bool isLeftSide, TCharacterActions self)
    {
        _self = self;
        _enemyCS = self.EnemyCA.GetComponent<CharacterState>();

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
        Debug.Log("aaa" + value);
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
    }

    private void HitCheck()
    {
        //当たり判定に面積がない場合無効にする
        if (_fieldBox.x == 0 || _fieldBox.y == 0) return;
        Collider2D[] colliders = Physics2D.OverlapBoxAll
            ((Vector2)transform.position + _boxOffset, _fieldBox, transform.rotation.z, _hurtBoxLayer);

        foreach (Collider2D collider in colliders)
        {
            // 攻撃が当たった情報を敵に送る
            if(_enemyCS.gameObject != collider.transform.parent.gameObject) continue;
            if (_enemyCS.CurrentHP - _damage > LOWEST_HP)
            {
                _enemyCS.TakeDamage(_damage);
            }
            else if (_enemyCS.CurrentHP - _damage > LOWEST_HP)
            {
                _enemyCS.TakeDamage(_enemyCS.CurrentHP - LOWEST_HP);
            }
        }
    }

    /// <summary>
    /// 自身がエリアの中かどうか
    /// </summary>
    /// <returns></returns>
    public bool IsInSelf()
    {
        if(_destroyCTS != null) return false;

        Collider2D[] colliders = Physics2D.OverlapBoxAll
            ((Vector2)transform.position + _boxOffset, _fieldBox, transform.rotation.z, _hurtBoxLayer);

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
        animtor.SetTrigger("EndTrigger");

        await UniTask.WaitUntil(() =>
        {
            if (animtor == null) return true;
            var startStateInfo = animtor.GetCurrentAnimatorStateInfo(0);
            return startStateInfo.IsName("End") && startStateInfo.normalizedTime >= 1f;
        }, cancellationToken: _destroyCTS.Token);

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube((Vector2)transform.position + _boxOffset, _fieldBox);
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
