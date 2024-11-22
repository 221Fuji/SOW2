using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CharacterActions : MonoBehaviour
{
    //コンポーネント
    protected Rigidbody2D _rb { get; private set; }
    protected FightingInputReceiver _inputReciever { get; private set; }
    protected CharacterState _characterState { get; private set; }
    protected Animator _animator { get; private set; }

    //共通変数
    protected GameObject _enemyObject { get; private set; }

    //硬直等での行動制限プロパティ
    protected virtual bool CanWalk { get; set; }
    protected virtual bool CanJump { get; set; }
    protected virtual bool _canHit { get; set; }


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _characterState = GetComponent<CharacterState>();
        _animator = GetComponent<Animator>();

        if (_inputReciever = GetComponent<FightingInputReceiver>())
        {
            SetActionDelegate();
        }

        SetHitBox();
    }

    /// <summary>
    /// InputReceiverのデリゲートを初期化する
    /// </summary>
    protected virtual void SetActionDelegate()
    {
        _inputReciever.JumpDelegate = Jump;
    }

    /// <summary>
    /// 各攻撃等の当たり判定の初期化
    /// </summary>
    protected virtual void SetHitBox()
    {
        Debug.Log("このキャラクターは当たり判定のある攻撃がありません");
    }

    /// <summary>
    /// 敵情報を設定する
    /// </summary>
    public void SetEnemy(GameObject enemyObject)
    {
        _enemyObject = enemyObject;
    }

    private void Update()
    {
        if(_inputReciever == null) return;

        //ヒット硬直中は以下の行動ができない
        if (_characterState.IsRecoveringHit) return;

        //Animatorパラメータを設定
        _animator.SetFloat("YspeedFloat", _rb.velocity.y);

        //歩き
        Walk(_inputReciever._WalkValue);

        //空中にいるときアニメーションを切り替える
        ChangeAnimatorLayer();

        if (_enemyObject == null) return;

        //キャラを向かい合わせる
        DirectionReversal();
    }

    /// <summary>
    /// キャラ同士を向かい合わせる
    /// </summary>
    protected virtual void DirectionReversal()
    {
        //接地中のみ反転
        if (!CanWalk && !_characterState.OnGround) return;

        if (transform.position.x > _enemyObject.transform.position.x)
        {
            transform.rotation = new Quaternion(0, 180, 0, 0);
            _characterState.SetIsLeft(false);
        }
        else
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
            _characterState.SetIsLeft(true);
        }
    }

    /// <summary>
    /// AnmatorのLayerを切り替える
    /// </summary>
    protected virtual void ChangeAnimatorLayer()
    {
        // 空中ならAirLayerに切り替える
        if(!_characterState.OnGround)
        {
            SetLayerWeightByName("WalkLayer", 0);
            SetLayerWeightByName("AirLayer", 1);
        }
        else
        {
            SetLayerWeightByName("WalkLayer", 1);
            SetLayerWeightByName("AirLayer", 0);
        }
    }

    /// <summary>
    /// 入力の方向によってキャラを移動させる
    /// </summary>
    /// <param name="inputValue">受け取った入力(1,-1,0のいずれか)</param>
    protected virtual void Walk(float inputValue)
    {
        if (!CanWalk) return;
        float speed = inputValue;

        //キャラの向きと入力に速度とアニメーションを一致させる
        if (_characterState.IsLeftSide)
        {
            if (inputValue > 0)
            {
                speed *= _characterState.CurrentFrontSpeed;
            }
            else
            {
                speed *= _characterState.CurrentBackSpeed;
            }
            _animator.SetFloat("WalkFloat", inputValue);
        }
        else
        {
            if (inputValue > 0)
            {
                speed *= _characterState.CurrentBackSpeed;
            }
            else
            {
                speed *= _characterState.CurrentFrontSpeed;
            }
            _animator.SetFloat("WalkFloat", -inputValue);
        }

        _rb.velocity = new Vector2(speed, _rb.velocity.y);
    }

    /// <summary>
    /// ジャンプさせる
    /// </summary>
    protected virtual void Jump()
    {
        if (!CanJump) return;

        //接地中のみ可能
        if (!_characterState.OnGround) return;

        //物理挙動
        Vector2 jumpVector = new Vector2(_rb.velocity.x, _characterState.CurrentJumpPower);
        _rb.AddForce(jumpVector, ForceMode2D.Impulse);

        //アニメーション
        _animator.SetTrigger("JumpTrigger");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            Land();
        }
    }

    /// <summary>
    /// 着地したときに呼ばれるメソッド
    /// ジャンプ攻撃をキャンセルしたりする用
    /// </summary>
    protected virtual void Land()
    {
        Debug.Log("着地");
    }

    /// <summary>
    /// 攻撃を受けた時の処理
    /// </summary>
    /// /// <param name="attackInfo">受けた攻撃の情報</param>
    public virtual async UniTask TakeAttack(AttackInfo attackInfo)
    {
        Debug.Log("攻撃を受けた！");

        //コンボ処理
        if (_characterState.IsRecoveringHit)
        {
            _characterState.CancelHitStun();
            _characterState.SetComboCount(_characterState.ConboCount + 1);
            Debug.Log($"{_characterState.ConboCount}コンボ");
        }
        else
        {
            _characterState.SetComboCount(1);
        }

        //ヒットバック処理
        _rb.velocity = Vector2.zero;
        Vector2 jumpVector;
        if (_characterState.IsLeftSide)
        {
            jumpVector = attackInfo.HitBackDirection * new Vector2(-1, 1);
        }
        else
        {
            jumpVector = attackInfo.HitBackDirection;
        }
        _rb.AddForce(jumpVector, ForceMode2D.Impulse);

        //アニメーション処理
        SetLayerWeightByName("HitLayer", 1); // AnimatorのLayerをHitLayerを最優先に変更
        _animator.SetTrigger("HitTrigger"); // 喰らいアニメーション再生

        //ヒット硬直処理
        Debug.Log($"ヒット硬直開始:{attackInfo.HitFrame}");
        await HitStun(attackInfo.HitFrame, _characterState.CreateHitCT());

        SetLayerWeightByName("HitLayer", 0); // AnimatorのLayerをHitLayerを最優度を元に戻す
    }

    /// <summary>
    /// ヒット硬直の処理
    /// </summary>
    /// <param name="recoveryFrame">硬直時間(フレーム)</param>
    public virtual async UniTask HitStun(int recoveryFrame, CancellationToken token)
    {
        await UniTask.DelayFrame(recoveryFrame, cancellationToken: token);

        if(token.IsCancellationRequested)
        {
            Debug.Log("ヒット硬直がキャンセルされた");
            _characterState.CancelHitStun();
            return;
        }

        //着地するまでヒット硬直が続く
        await UniTask.WaitUntil(() => _characterState.OnGround);

        _characterState.CancelHitStun();
    }

    /// <summary>
    /// AnimatorのSetlayerWeightをlayerの名前で呼び出す
    /// </summary>
    protected void SetLayerWeightByName(string layerName, float weight)
    {
        int layerIndex = _animator.GetLayerIndex(layerName);
        if (layerIndex != -1)
        {
            _animator.SetLayerWeight(layerIndex, weight);
        }
        else
        {
            Debug.LogWarning($"レイヤー名 '{layerName}' が見つかりませんでした");
        }
    }
}
