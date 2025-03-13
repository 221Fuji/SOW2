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
    // �󂯂����͂̃v���p�e�B
    public float WalkValue { get; set; } = 0;
    public bool IsInputingGuard { get; private set; }
    public bool join = false;

    // �e��s���̃f���Q�[�g
    public UnityAction JumpDelegate { get; set; }
    public UnityAction<bool> GuardDelegate { get; set; }
    public Func<UniTask> NormalMove { get; set; }
    public Func<UniTask> SpecialMove1 { get; set; }
    public Func<UniTask> SpecialMove2 { get; set; }
    public Func<UniTask> Ultimate { get; set; }

    // ���݂̓��͏�Ԃ��Ǘ�
    private bool isProcessingInput = false; // ���̓��͏������t���O

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void OnFourDirections(InputValue value)
    {

        Vector2 direction = value.Get<Vector2>();
        if (direction.x > 0)
        {
            WalkValue = 1;
        }
        else if (direction.x < 0)
        {
            WalkValue = -1;
        }
        else
        {
            WalkValue = 0f;
        }
    }

    // �W�����v
    public void OnJump()
    {
        if (FightingPhysics.FightingFrameRate == 0 || FightingPhysics.FightingTimeScale == 0) return;

        if (!isProcessingInput)
        {
            isProcessingInput = true;
            JumpDelegate?.Invoke();
            ResetInputProcessing();
        }
    }

    // �ʏ�Z
    public void OnNomalMove()
    {
        if (FightingPhysics.FightingFrameRate == 0 || FightingPhysics.FightingTimeScale == 0) return;

        if (!isProcessingInput)
        {
            isProcessingInput = true;
            NormalMove?.Invoke();
            ResetInputProcessing();
        }
    }

    // �K�E�Z�P
    public void OnSpecialMove1()
    {
        if (FightingPhysics.FightingFrameRate == 0 || FightingPhysics.FightingTimeScale == 0) return;

        isProcessingInput = true;
        SpecialMove1?.Invoke();
        ResetInputProcessing();
    }

    // �K�E�Z2
    public void OnSpecialMove2()
    {
        if (FightingPhysics.FightingFrameRate == 0 || FightingPhysics.FightingTimeScale == 0) return;

        isProcessingInput = true;
        SpecialMove2?.Invoke();
        ResetInputProcessing();
    }

    //���K�E�Z
    public void OnUltimate()
    {
        if (FightingPhysics.FightingFrameRate == 0 || FightingPhysics.FightingTimeScale == 0) return;

        isProcessingInput = true;
        Ultimate?.Invoke();
        ResetInputProcessing();
    }

    //�K�[�h
    public void OnGuard(InputValue inputValue)
    {

        if (inputValue.Get<float>() == 1)
        {
            // �{�^���������ꂽ���̏���
            IsInputingGuard = true;
            GuardDelegate?.Invoke(true);
            isProcessingInput = true;
        }
        else
        {
            // �{�^���������ꂽ���̏���
            IsInputingGuard = false;
            GuardDelegate?.Invoke(false);
            ResetInputProcessing();
        }
    }

    private void ResetInputProcessing()
    {
        isProcessingInput = false; // ���͏������t���O�����Z�b�g
    }
}
