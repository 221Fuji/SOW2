using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// 攻撃の当たり判定を管理するクラス
/// </summary>
public class HitBoxManager : MonoBehaviour
{
    [SerializeField] private LayerMask _hurtBoxLayer;
    [SerializeField] private bool _sceneCheckMode;

    private AttackInfo _attackInfo;
    private GameObject _self;

    /// <summary>
    /// 当たり判定の有効状態
    /// </summary>
    public bool IsActive { get; private set; }
    public int PlayerNum 
    {
        get { return _self.GetComponent<CharacterActions>().PlayerNum; }
    }
    public UnityAction Hit { get; set; }
    public UnityAction<Bullet> HitBullet { get; set; }
    public UnityAction Guard { get; set; }
    public UnityAction<Bullet> GuardBullet { get; set; }

    public static List<HitBoxManager> HitBoxes { get; private set; }

    public void InitializeHitBox(AttackInfo attackInfo, GameObject self)
    {
        _attackInfo = attackInfo;
        _self = self;

        if(HitBoxes == null)
        {
            HitBoxes = new List<HitBoxManager>();
        }
        
        if(HitBoxes != null)
        {
            if(!HitBoxes.Contains(this))
            {
                HitBoxes.Add(this);
            }
        }
    }

    public void SetIsActive(bool value)
    {
        IsActive = value;
    }

    private void Update()
    {
        // 持続中かどうか
        if(!IsActive || FightingPhysics.FightingTimeScale <= 0) return;

        //当たり判定に面積がない場合無効にする
        if (transform.lossyScale.x == 0 || transform.lossyScale.y == 0) return;

        Collider2D[] colliders = Physics2D.OverlapBoxAll
            (transform.position, transform.lossyScale, transform.rotation.z, _hurtBoxLayer);

        foreach (Collider2D collider in colliders)
        {
            //自身には当たらない
            if (collider.transform.parent == _self.transform) continue;

            HurtBoxManager hurtBox = collider.GetComponent<HurtBoxManager>();
            //自身のオブジェクトには当たらない
            if (hurtBox.PlayerNum == PlayerNum) continue;

            // 攻撃が当たった情報を敵に送る
            GameObject enemy = collider.transform.parent.gameObject;
            CharacterState enemyCS = enemy.GetComponent<CharacterState>();
            bool IsEnemyGuarding = enemyCS ? enemyCS.IsGuarding : false;


            if (!IsEnemyGuarding)
            {
                if (!hurtBox.IsActive) continue;

                //ヒット
                Debug.Log($"攻撃がヒット");

                collider.GetComponent<HurtBoxManager>().TakeAttack(_attackInfo);
                Hit?.Invoke();
                HitBullet?.Invoke(transform.parent.GetComponent<Bullet>());
            }
            else
            {
                //ガードされた
                Debug.Log($"攻撃がガードされた");

                if(!enemyCS.AnormalyStates.Contains(AnormalyState.Dead))
                {
                    enemy.GetComponent<CharacterActions>()?.Guard(_attackInfo);
                    Guard?.Invoke();
                    GuardBullet?.Invoke(transform.parent.GetComponent<Bullet>());
                }
            }
            SetIsActive(false);

            return;
        }
    }

    private void OnDestroy()
    {
        if(HitBoxes != null)
        {
            if(HitBoxes.Contains(this))
            {
                HitBoxes.Remove(this);
            }
        }
    }

    private void OnDrawGizmos()
    {
        //持続中のみ当たり判定描画
        if(_sceneCheckMode)
        {
            if (!IsActive) return;
        }

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, transform.lossyScale);
    }
}
