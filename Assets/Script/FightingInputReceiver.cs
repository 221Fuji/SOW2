using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// �퓬���̃v���C���[�̓��͂��󂯎��N���X
/// </summary>
public class FightingInputReceiver : MonoBehaviour
{
    //�󂯂����͂̃v���p�e�B
    //���ƂŃZ�b�^�[��private�ɂ��Ƃ�
    public float _WalkValue { get; set; } = 0;
    //�f�o�b�O�p
    public bool join = false;

    //�e��s���̃f���Q�[�g
    public UnityAction JumpDelegate { get; set; }
    public Func<UniTask> NomalMove { get; set; }
    public Func<UniTask> SpecialMove1 { get; set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        //�f�o�b�O�p
        Application.targetFrameRate = 60;
    }

    public void OnFourDirections(InputValue value)
    {
        //�������͂�1,-1,0�̂ǂꂩ
        if(value.Get<Vector2>().x > 0)
        {
            _WalkValue = 1f;
        }
        else if(value.Get<Vector2>().x < 0) 
        {
            _WalkValue = -1f;
        }
        else
        {
            _WalkValue = 0f;
        }
    }

    //�W�����v
    public void OnJump(InputValue value)
    {
        JumpDelegate?.Invoke();
    }

    //�ʏ�Z
    public void OnNomalMove()
    {
        NomalMove?.Invoke();
    }

    //�K�E�Z�P
    public void OnSpecialMove1()
    {
        SpecialMove1?.Invoke();
    }
}
