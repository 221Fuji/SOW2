using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;

/// <summary>
/// �U���̓����蔻����Ǘ�����N���X
/// </summary>
public class HitBoxManager : MonoBehaviour
{
    [SerializeField] private LayerMask _hurtBoxLayer;
    [SerializeField] private bool _sceneCheckMode;
    AttackInfo _attackInfo;
    public bool IsActive { get; private set; } = false;

    public void InitializeHitBox(AttackInfo attackInfo)
    {
        _attackInfo = attackInfo;
    }

    /// <summary>
    /// �����蔻��̗L����Ԃ�ݒ肷��
    /// </summary>
    public void SetIsActive(bool value)
    {
        IsActive = value;
    }

    private void Update()
    {
        // ���������ǂ���
        if(!IsActive) return;

        Collider2D[] colliders = Physics2D.OverlapBoxAll
            (transform.position, transform.lossyScale, transform.rotation.z, _hurtBoxLayer);
        foreach (Collider2D collider in colliders)
        {
            if (collider.transform.parent == transform.parent) continue;
            Debug.Log($"�U�����q�b�g");

            // �U����������������G�ɑ���
            collider.transform.parent.GetComponent<CharacterActions>()?.TakeAttack(_attackInfo);
            IsActive = false;
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
