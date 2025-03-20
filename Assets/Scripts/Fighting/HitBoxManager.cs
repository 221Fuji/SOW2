using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// �U���̓����蔻����Ǘ�����N���X
/// </summary>
public class HitBoxManager : MonoBehaviour
{
    [SerializeField] private LayerMask _hurtBoxLayer;
    [SerializeField] private bool _sceneCheckMode;

    private AttackInfo _attackInfo;
    private GameObject _self;

    /// <summary>
    /// �����蔻��̗L�����
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
        // ���������ǂ���
        if(!IsActive || FightingPhysics.FightingTimeScale <= 0) return;

        //�����蔻��ɖʐς��Ȃ��ꍇ�����ɂ���
        if (transform.lossyScale.x == 0 || transform.lossyScale.y == 0) return;

        Collider2D[] colliders = Physics2D.OverlapBoxAll
            (transform.position, transform.lossyScale, transform.rotation.z, _hurtBoxLayer);

        foreach (Collider2D collider in colliders)
        {
            //���g�ɂ͓�����Ȃ�
            if (collider.transform.parent == _self.transform) continue;

            // �U����������������G�ɑ���
            GameObject enemy = collider.transform.parent.gameObject;
            CharacterState enemyCS = enemy.GetComponent<CharacterState>();
            if(!enemyCS.IsGuarding)
            {
                //�q�b�g
                Debug.Log($"�U�����q�b�g");
                if(!enemyCS.AnormalyStates.Contains(AnormalyState.Dead))
                {
                    enemy.GetComponent<CharacterActions>()?.TakeAttack(_attackInfo);
                    Hit?.Invoke();
                    HitBullet?.Invoke(transform.parent.GetComponent<Bullet>());
                }
            }
            else
            {
                //�K�[�h���ꂽ
                Debug.Log($"�U�����K�[�h���ꂽ");

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
        //�������̂ݓ����蔻��`��
        if(_sceneCheckMode)
        {
            if (!IsActive) return;
        }

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, transform.lossyScale);
    }
}
