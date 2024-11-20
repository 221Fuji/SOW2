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
    //受けた入力のプロパティ
    //あとでセッターをprivateにしとく
    public float _WalkValue { get; set; } = 0;
    //デバッグ用
    public bool join = false;

    //各種行動のデリゲート
    public UnityAction JumpDelegate { get; set; }
    public Func<UniTask> NomalMove { get; set; }
    public Func<UniTask> SpecialMove1 { get; set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        //デバッグ用
        Application.targetFrameRate = 60;
    }

    public void OnFourDirections(InputValue value)
    {
        //歩き入力は1,-1,0のどれか
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

    //ジャンプ
    public void OnJump(InputValue value)
    {
        JumpDelegate?.Invoke();
    }

    //通常技
    public void OnNomalMove()
    {
        NomalMove?.Invoke();
    }

    //必殺技１
    public void OnSpecialMove1()
    {
        SpecialMove1?.Invoke();
    }
}
