using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using System.Collections.Generic;

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
    [Header("制限時間")]
    [SerializeField] private TextMeshProUGUI _timeLimitText;
    [Space]
    [Header("キャラの顔")]
    [SerializeField] private Transform _faceUp1P;
    [SerializeField] private Transform _faceUp2P;
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
    [SerializeField] private Animator _timeOver;
    [Space]
    [Header("暗転用パネル")]
    [SerializeField] private Image _panel;
    [Space]
    [Header("固有リソース")]
    [SerializeField] private Transform _hudCanvas;
    [SerializeField] private Slider _fogMeter;
    [SerializeField] private Slider _uramiMeter;
    [SerializeField] private GameObject _rabiriGage;
    private List<UniqueResourceUI> _urUIList = new();

    private CharacterState _cs1P;
    private CharacterState _cs2P;
    private CharacterActions _ca1P;
    private CharacterActions _ca2P;

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
    public void SetPlayer(PlayerData pd1, PlayerData pd2)
    {
        _cs1P = pd1.CharacterState;
        _cs2P = pd2.CharacterState;

        _ca1P = pd1.CharacterActions;
        _ca2P = pd2.CharacterActions;

        //デリゲートの登録
        _ca1P.ComboCount = ComboCount;
        _ca2P.ComboCount = ComboCount;

        //固有リソースの設定
        InitializeUniqueResource(_ca1P);
        InitializeUniqueResource(_ca2P);

        //顔
        InstantiateFaceUpImage(pd1);
        InstantiateFaceUpImage(pd2);
    }

    private void InstantiateFaceUpImage(PlayerData playerData)
    {
        if(playerData.PlayerNum == 1)
        {
            Transform face1P = Instantiate(playerData.CharacterData.FightingFaceUpImage.transform);
            face1P.SetParent(_faceUp1P, false);
        }
        else
        {
            Transform face2P = Instantiate(playerData.CharacterData.FightingFaceUpImage.transform);
            face2P.SetParent(_faceUp2P, false);
            face2P.localPosition *= new Vector2(-1, 1);
            face2P.localScale *= new Vector2(-1, 1);
        }
    }

    /// <summary>
    /// 固有ゲージの設定
    /// </summary>
    public void InitializeUniqueResource(CharacterActions ca)
    {
        //Cloudの固有リソース設定
        if(ca is ViolaCloud cloud)
        {
            Slider slider = InstantiateUniqueMeter(ca.PlayerNum, _fogMeter.gameObject).GetComponent<Slider>();
            _urUIList.Add(new FogMeter(cloud, slider));
        }

        //Teddyの固有リソース設定
        if (ca is Teddy teddy)
        {
            Slider slider = InstantiateUniqueMeter(ca.PlayerNum, _uramiMeter.gameObject).GetComponent<Slider>();
            _urUIList.Add(new UramiMeter(teddy, slider));
        }

        //Rabiriの固有リソース設定
        if (ca is Rabiri rabiri)
        {
            GameObject gage = InstantiateUniqueMeter(ca.PlayerNum, _rabiriGage);
            _urUIList.Add(new RabiriGage(rabiri, gage));
        }
    }

    private void Update()
    {
        ApplyBar();
        SPcolorChange();
        UltElectricity();
        foreach(var urUI in _urUIList) 
        {
            urUI.UpdateUniueResourceUI();
        }
    }

    /// <summary>
    /// それぞれのゲージをスライダーに反映させる
    /// </summary>
    private void ApplyBar()
    {
        //SPBer
        if(_cs1P != null)
        {
            _spSlider1P.value = _cs1P.CurrentSP / _cs1P.MaxSP;
        }
        if (_cs2P != null)
        {
            _spSlider2P.value = _cs2P.CurrentSP / _cs2P.MaxSP;
        }

        //HPBer      
        if (_cs1P != null)
        {
            _hpSlider1P.value = _cs1P.CurrentHP / _cs1P.MaxHP;
        }
        if (_cs2P != null)
        {
            _hpSlider2P.value = _cs2P.CurrentHP / _cs2P.MaxHP;
        }

        //RedBar
        if(_cs1P != null)
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
        if (_cs2P != null)
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
        if (_cs1P != null)
        {
            _upSlider1P.value = _cs1P.CurrentUP / _cs1P.MaxUP;
        }
        if(_cs2P != null)
        {
            _upSlider2P.value = _cs2P.CurrentUP / _cs2P.MaxUP;
        }
    }

    /// <summary>
    /// SPBarの色を変える
    /// </summary>
    private void SPcolorChange()
    {
        Image imageSP1P = _spSlider1P.GetComponentInChildren<Image>();
        Image imageSP2P = _spSlider2P.GetComponentInChildren<Image>();

        if(_cs1P != null)
        {
            if (_cs1P.AnormalyStates.Contains(AnormalyState.Fatigue))
            {
                imageSP1P.color = new Color(0.75f, 0.75f, 0.75f);
            }
            else
            {
                imageSP1P.color = new Color(1, _spSlider1P.value, 0);
            }
        }

        if(_cs2P != null)
        {
            if (_cs2P.AnormalyStates.Contains(AnormalyState.Fatigue))
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
        if(_cs1P != null)
        {
            if(_cs1P.CurrentUP >= 100)
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

        if (_cs2P != null)
        {
            if (_cs2P.CurrentUP >= 100)
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

    private GameObject InstantiateUniqueMeter(int playerNum, GameObject urUI)
    {
        if (playerNum == 1)
        {
            GameObject _uniqueMeter1P = Instantiate(urUI);
            _uniqueMeter1P.transform.SetParent(_hudCanvas, false);
            return _uniqueMeter1P;
            
        }
        else
        {
            GameObject uniqueMeter2P = Instantiate(urUI);
            uniqueMeter2P.transform.SetParent(_hudCanvas, false);
            uniqueMeter2P.GetComponent<RectTransform>().anchoredPosition *= new Vector2(-1, 1);
            uniqueMeter2P.transform.localScale *= new Vector2(-1, 1);
            return uniqueMeter2P;
        }
    }

    public void SetTimeLimitText(string timeString)
    {
        _timeLimitText.text = timeString;
    }

    /// <summary>
    /// 残機ハートの表示
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

        try
        {
            await UniTask.WaitUntil(() =>
            {
                return AnimatorByLayerName.GetCurrentAnimationProgress(_round, "Base Layer") >= 1f;
            }, cancellationToken: token);

            await UniTask.DelayFrame(30);

            //Fight開始
            _round.SetTrigger("FightTrigger");
            SoundManager.I.SystemSEPlayer.PlaySE(10);

            await UniTask.WaitUntil(() =>
            {
                return AnimatorByLayerName.GetCurrentAnimationProgress(_round, "Base Layer") >= 1f;
            }, cancellationToken: token);
        }
        catch { }

        RoundCallCancel();
    }

    public async UniTask KO()
    {
        if (_gameSet == null) return;

        _gameSet.SetTrigger("KOTrigger");

        SoundManager.I.SystemSEPlayer.PlaySE(11);

        _gameSetCTS = new CancellationTokenSource();
        CancellationToken token = _gameSetCTS.Token;

        await UniTask.WaitUntil(() =>
        {
            return AnimatorByLayerName.GetCurrentAnimationProgress(_gameSet, "Base Layer") >= 1f;
        }, cancellationToken: token);

        await _panel.DOFade(1f, 1f).ToUniTask(cancellationToken: token);

        GameSetCancel();
    }

    public async UniTask TimeOver()
    {
        _timeOver.SetTrigger("TimeOverTrigger");

        _gameSetCTS = new CancellationTokenSource();
        CancellationToken token = _gameSetCTS.Token;

        if (_timeOver == null) return;

        await UniTask.WaitUntil(() =>
        {
            return AnimatorByLayerName.GetCurrentAnimationProgress(_timeOver, "Base Layer") >= 1f;
        }, cancellationToken: token);

        await _panel.DOFade(1f, 1f).ToUniTask(cancellationToken: token);

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
