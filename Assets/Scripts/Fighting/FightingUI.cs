using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using System.ComponentModel;
using static UnityEngine.Rendering.DebugUI;

public class FightingUI : MonoBehaviour
{
    [Header("ゲージ")]
    [SerializeField] private Slider _hpSlider1P;
    [SerializeField] private Slider _hpSlider2P;
    [SerializeField] private Slider _redSlider1P;
    [SerializeField] private Slider _redSlider2P;
    [SerializeField] private Slider _spSlider1P;
    [SerializeField] private Slider _spSlider2P;
    [SerializeField] private Slider _upSlider1P;
    [SerializeField] private Slider _upSlider2P;
    [Space]
    [Header("Ultゲージ演出")]
    [SerializeField] private Image _upFill1P;
    [SerializeField] private Image _upFill2P;
    [SerializeField] private Image _upElectricity1P;
    [SerializeField] private Image _upElectricity2P;
    [SerializeField] private Color _notMaxColor;
    [Space]
    [Header("コンボ演出")]
    [SerializeField] private TextMeshProUGUI _comboCounter1P;
    [SerializeField] private TextMeshProUGUI _comboCounter2P;
    [Space]
    [Header("残機ハート")]
    [SerializeField] private Image _firstHeart1P;
    [SerializeField] private Image _secondHeart1P;
    [SerializeField] private Image _firstHeart2P;
    [SerializeField] private Image _secondHeart2P;
    [Space]
    [Header("ラウンドコール")]
    [SerializeField] private Animator _round;
    [SerializeField] private Animator _gameSet;
    [Space]
    [Header("暗転用パネル")]
    [SerializeField] private Image _panel;

    private CharacterState _characterState1P;
    private CharacterState _characterState2P;

    //1フレームで赤ゲージの減る量
    private float redBarSpeed = 0.0025f;

    //コンボ演出関連
    private int _conboCounterDisplayFrame = 90; //コンボ演出が消えるフレーム
    private CancellationTokenSource _comboCounterCTS;

    //ラウンドコール関連
    private CancellationTokenSource _roundCallCTS;

    //ゲームセット関連
    private CancellationTokenSource _gameSetCTS;

    /// <summary>
    /// キャラクターをそれぞれ設定
    /// </summary>
    public void SetPlayer(CharacterState cs1p, CharacterState cs2p)
    {
        _characterState1P = cs1p;
        _characterState2P = cs2p;

        //デリゲートの登録
        cs1p.GetComponent<CharacterActions>().ComboCount = ComboCount;
        cs2p.GetComponent<CharacterActions>().ComboCount = ComboCount;
    }

    private void Update()
    {
        ApplyBar();
        SPcolorChange();
        UltElectricity();
    }

    /// <summary>
    /// それぞれのゲージをスライダーに反映させる
    /// </summary>
    private void ApplyBar()
    {
        //SPBer
        if(_characterState1P != null)
        {
            _spSlider1P.value = _characterState1P.CurrentSP / _characterState1P.MaxSP;
        }
        if (_characterState2P != null)
        {
            _spSlider2P.value = _characterState2P.CurrentSP / _characterState2P.MaxSP;
        }

        //HPBer      
        if (_characterState1P != null)
        {
            _hpSlider1P.value = _characterState1P.CurrentHP / _characterState1P.MaxHP;
        }
        if (_characterState2P != null)
        {
            _hpSlider2P.value = _characterState2P.CurrentHP / _characterState2P.MaxHP;
        }

        //RedBar
        if(_characterState1P != null)
        {
            //コンボが終われば減り始める
            //if (_characterState1P.IsRecoveringHit) return;

            if(_redSlider1P.value >= _hpSlider1P.value)
            {
                _redSlider1P.value -= redBarSpeed;
            }
            else
            {
                _redSlider1P.value = _hpSlider1P.value;
            }
        }
        if (_characterState2P != null)
        {
            //コンボが終われば減り始める
            //if (_characterState2P.IsRecoveringHit) return;

            if (_redSlider2P.value >= _hpSlider2P.value)
            {
                _redSlider2P.value -= redBarSpeed;
            }
            else
            {
                _redSlider2P.value = _hpSlider2P.value;
            }
        }

        //UPBer
        if (_characterState1P != null)
        {
            _upSlider1P.value = _characterState1P.CurrentUP / _characterState1P.MaxUP;
        }
        if(_characterState2P != null)
        {
            _upSlider2P.value = _characterState2P.CurrentUP / _characterState2P.MaxUP;
        }
    }

    /// <summary>
    /// SPBarの色を変える
    /// </summary>
    private void SPcolorChange()
    {
        Image imageSP1P = _spSlider1P.GetComponentInChildren<Image>();
        Image imageSP2P = _spSlider2P.GetComponentInChildren<Image>();

        if(_characterState1P != null)
        {
            if (_characterState1P.AnormalyStates.Contains(AnormalyState.Fatigue))
            {
                imageSP1P.color = new Color(0.75f, 0.75f, 0.75f);
            }
            else
            {
                imageSP1P.color = new Color(1, _spSlider1P.value, 0);
            }
        }

        if(_characterState2P != null)
        {
            if (_characterState2P.AnormalyStates.Contains(AnormalyState.Fatigue))
            {
                imageSP2P.color = new Color(0.75f, 0.75f, 0.75f);
            }
            else
            {
                imageSP2P.color = new Color(1, _spSlider2P.value, 0);
            }
        }
    }

    /// <summary>
    /// UPBarが満タンのときの演出
    /// </summary>
    private void UltElectricity()
    {
        if(_characterState1P != null)
        {
            if(_characterState1P.CurrentUP >= 100)
            {
                _upElectricity1P.color = Color.white;
                _upFill1P.color = Color.white;
            }
            else
            {
                _upElectricity1P.color = new Color(0, 0, 0, 0);
                _upFill1P.color = _notMaxColor;
            }
        }

        if (_characterState2P != null)
        {
            if (_characterState2P.CurrentUP >= 100)
            {
                _upElectricity2P.color = Color.white;
                _upFill2P.color = Color.white;
            }
            else
            {
                _upElectricity2P.color = new Color(0, 0, 0, 0);
                _upFill2P.color = _notMaxColor;
            }
        }
    }

    /// <summary>
    /// 残機ハートの表示（適当な実装）
    /// </summary>
    public void HeartLost(RoundData roundData)
    {
        Color invisible = new Color(0, 0, 0, 0);

        if(_firstHeart1P != null && _secondHeart1P != null)
        {
            if (roundData.Heart1P <= 1)
            {
                _firstHeart1P.color = invisible;
            }
            if (roundData.Heart1P <= 0)
            {
                _secondHeart1P.color = invisible;
            }
        }

        if(_firstHeart2P != null && _secondHeart2P != null)
        {
            if (roundData.Heart2P <= 1)
            {
                _firstHeart2P.color = invisible;
            }
            if (roundData.Heart2P <= 0)
            {
                _secondHeart2P.color = invisible;
            }
        }
    }

    public async UniTask RoundCall(int roundNum)
    {
        _round.SetInteger("RoundNumInt", roundNum);

        _roundCallCTS = new CancellationTokenSource();
        CancellationToken token = _roundCallCTS.Token;

        if (_round == null) return;

        await UniTask.WaitUntil(() =>
        {
            return AnimatorByLayerName.GetCurrentAnimationProgress(_round, "Base Layer") >= 1f;
        }, cancellationToken: token);

        await UniTask.DelayFrame(30);

        //Fight開始
        _round.SetTrigger("FightTrigger");

        await UniTask.WaitUntil(() =>
        {
            return AnimatorByLayerName.GetCurrentAnimationProgress(_round, "Base Layer") >= 1f;
        }, cancellationToken: token);

        RoundCallCancel();
    }

    public async UniTask KO()
    {
        _gameSet.SetTrigger("KOTrigger");

        _gameSetCTS = new CancellationTokenSource();
        CancellationToken token = _gameSetCTS.Token;

        if (_gameSet == null) return;

        await UniTask.WaitUntil(() =>
        {
            return AnimatorByLayerName.GetCurrentAnimationProgress(_gameSet, "Base Layer") >= 1f;
        }, cancellationToken: token);

        await _panel.DOFade(1f, 0.5f).ToUniTask(cancellationToken: token);

        GameSetCancel();
    }

    /// <summary>
    /// コンボ演出
    /// </summary>
    private async UniTask ComboCount(int playerNum, int comboNum)
    {
        if (comboNum <= 1) return;

        //前回のUniTask処理
        if(_comboCounterCTS != null)
        {
            ComboCountCancel();
        }

        TextMeshProUGUI _comboCounter = playerNum == 1 ? _comboCounter1P : _comboCounter2P;

        _comboCounter.text = $"{comboNum}<size=50>combo</size>";

        _comboCounterCTS = new CancellationTokenSource();
        await ComboCountDisplay(_comboCounterCTS.Token);

        _comboCounter.text = string.Empty;
    }

    /// <summary>
    /// この間コンボ演出を表示
    /// </summary>
    private async UniTask ComboCountDisplay(CancellationToken token)
    {
        await UniTask.DelayFrame(_conboCounterDisplayFrame, cancellationToken: token);
        ComboCountCancel();
    }

    /// <summary>
    /// コンボ演出をキャンセル
    /// </summary>
    public void ComboCountCancel()
    {
        _comboCounterCTS?.Cancel();
        _comboCounterCTS = null;
    }

    public void RoundCallCancel()
    {
        _roundCallCTS?.Cancel();
        _roundCallCTS = null;
    }

    public void GameSetCancel() 
    {
        _gameSetCTS?.Cancel();
        _gameSetCTS = null;
    }

    private void OnDestroy()
    {
        ComboCountCancel();
        RoundCallCancel();
        GameSetCancel();
    }
}
