using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// UŒ‚‚Ì“–‚½‚è”»’è‚ğŠÇ—‚·‚éƒNƒ‰ƒX
/// </summary>
public class HitBoxManager : MonoBehaviour
{
    [SerializeField] private LayerMask _hurtBoxLayer;
    [SerializeField] private bool _sceneCheckMode;

    private AttackInfo _attackInfo;
    private GameObject _self;

    /// <summary>
    /// “–‚½‚è”»’è‚Ì—LŒøó‘Ô
    /// </summary>
    public bool IsActive { get; private set; }
    public UnityAction Hit { get; set; }
    public UnityAction<Bullet> HitBullet { get; set; }
    public UnityAction Guard { get; set; }
    public UnityAction<Bullet> GuardBullet { get; set; }

    public void InitializeHitBox(AttackInfo attackInfo, GameObject self)
    {
        _attackInfo = attackInfo;
        _self = self;
    }

    public void SetIsActive(bool value)
    {
        IsActive = value;
    }

    private void Update()
    {
        // ‘±’†‚©‚Ç‚¤‚©
        if(!IsActive || FightingPhysics.FightingTimeScale <= 0) return;

        //“–‚½‚è”»’è‚É–ÊÏ‚ª‚È‚¢ê‡–³Œø‚É‚·‚é
        if (transform.lossyScale.x == 0 || transform.lossyScale.y == 0) return;

        Collider2D[] colliders = Physics2D.OverlapBoxAll
            (transform.position, transform.lossyScale, transform.rotation.z, _hurtBoxLayer);

        foreach (Collider2D collider in colliders)
        {
            //©g‚É‚Í“–‚½‚ç‚È‚¢
            if (collider.transform.parent == _self.transform) continue;

            // UŒ‚‚ª“–‚½‚Á‚½î•ñ‚ğ“G‚É‘—‚é
            GameObject enemy = collider.transform.parent.gameObject;
            CharacterState enemyCS = enemy.GetComponent<CharacterState>();
            if(!enemyCS.IsGuarding)
            {
                //ƒqƒbƒg
                Debug.Log($"UŒ‚‚ªƒqƒbƒg");
                if(!enemyCS.AnormalyStates.Contains(AnormalyState.Dead))
                {
                    enemy.GetComponent<CharacterActions>()?.TakeAttack(_attackInfo);
                    Hit?.Invoke();
                    HitBullet?.Invoke(transform.parent.GetComponent<Bullet>());
                }
            }
            else
            {
                //ƒK[ƒh‚³‚ê‚½
                Debug.Log($"UŒ‚‚ªƒK[ƒh‚³‚ê‚½");

                if(!enemyCS.AnormalyStates.Contains(AnormalyState.Dead))
                {
                    enemy.GetComponent<CharacterActions>()?.Guard(_attackInfo);
                    Guard?.Invoke();
                    GuardBullet?.Invoke(transform.parent.GetComponent<Bullet>());
                }
            }
            SetIsActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        //‘±’†‚Ì‚İ“–‚½‚è”»’è•`‰æ
        if(_sceneCheckMode)
        {
            if (!IsActive) return;
        }

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, transform.lossyScale);
    }
}
