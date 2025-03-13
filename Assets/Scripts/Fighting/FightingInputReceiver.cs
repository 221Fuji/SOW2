using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// 戦闘中のプレイヤーの入力を受け取るクラス
/// </summary>
public class FightingInputReceiver : MonoBehaviour
{
    // 受けた入力のプロパティ
    public float WalkValue { get; set; } = 0;
    public bool IsInputingGuard { get; private set; }
    public bool join = false;

    // 各種行動のデリゲート
    public UnityAction JumpDelegate { get; set; }
    public UnityAction<bool> GuardDelegate { get; set; }
    public Func<UniTask> NormalMove { get; set; }
    public Func<UniTask> SpecialMove1 { get; set; }
    public Func<UniTask> SpecialMove2 { get; set; }
    public Func<UniTask> Ultimate { get; set; }

    // 現在の入力状態を管理
    private bool isProcessingInput = false; // 他の入力処理中フラグ

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

    // ジャンプ
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

    // 通常技
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

    // 必殺技１
    public void OnSpecialMove1()
    {
        if (FightingPhysics.FightingFrameRate == 0 || FightingPhysics.FightingTimeScale == 0) return;

        isProcessingInput = true;
        SpecialMove1?.Invoke();
        ResetInputProcessing();
    }

    // 必殺技2
    public void OnSpecialMove2()
    {
        if (FightingPhysics.FightingFrameRate == 0 || FightingPhysics.FightingTimeScale == 0) return;

        isProcessingInput = true;
        SpecialMove2?.Invoke();
        ResetInputProcessing();
    }

    //超必殺技
    public void OnUltimate()
    {
        if (FightingPhysics.FightingFrameRate == 0 || FightingPhysics.FightingTimeScale == 0) return;

        isProcessingInput = true;
        Ultimate?.Invoke();
        ResetInputProcessing();
    }

    //ガード
    public void OnGuard(InputValue inputValue)
    {

        if (inputValue.Get<float>() == 1)
        {
            // ボタンが押された時の処理
            IsInputingGuard = true;
            GuardDelegate?.Invoke(true);
            isProcessingInput = true;
        }
        else
        {
            // ボタンが離された時の処理
            IsInputingGuard = false;
            GuardDelegate?.Invoke(false);
            ResetInputProcessing();
        }
    }

    private void ResetInputProcessing()
    {
        isProcessingInput = false; // 入力処理中フラグをリセット
    }
}
