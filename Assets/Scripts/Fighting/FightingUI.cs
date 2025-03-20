using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;

public class FightingUI : MonoBehaviour
{
    [Header("�Q�[�W")]
    [SerializeField] private Slider _hpSlider1P;
    [SerializeField] private Slider _hpSlider2P;
    [SerializeField] private Slider _redSlider1P;
    [SerializeField] private Slider _redSlider2P;
    [SerializeField] private Slider _spSlider1P;
    [SerializeField] private Slider _spSlider2P;
    [SerializeField] private Slider _upSlider1P;
    [SerializeField] private Slider _upSlider2P;
    [Space]
    [Header("Ult�Q�[�W���o")]
    [SerializeField] private Image _upFill1P;
    [SerializeField] private Image _upFill2P;
    [SerializeField] private Image _upElectricity1P;
    [SerializeField] private Image _upElectricity2P;
    [SerializeField] private Color _notMaxColor;
    [Space]
    [Header("��������")]
    [SerializeField] private TextMeshProUGUI _timeLimitText;
    [Space]
    [Header("�L�����̊�")]
    [SerializeField] private Transform _faceUp1P;
    [SerializeField] private Transform _faceUp2P;
    [Space]
    [Header("�R���{���o")]
    [SerializeField] private TextMeshProUGUI _comboCounter1P;
    [SerializeField] private TextMeshProUGUI _comboCounter2P;
    [Space]
    [Header("�c�@�n�[�g")]
    [SerializeField] private Image _firstHeart1P;
    [SerializeField] private Image _secondHeart1P;
    [SerializeField] private Image _firstHeart2P;
    [SerializeField] private Image _secondHeart2P;
    [Space]
    [Header("���E���h�R�[��")]
    [SerializeField] private Animator _round;
    [SerializeField] private Animator _gameSet;
    [Space]
    [Header("�Ó]�p�p�l��")]
    [SerializeField] private Image _panel;
    [Space]
    [Header("�ŗL���\�[�X")]
    [SerializeField] private Transform _hudCanvas;
    [SerializeField] private Slider _fogMeter;
    private Slider _fogMeterPos1P = null;
    private Slider _fogMeterPos2P = null;

    private CharacterState _cs1P;
    private CharacterState _cs2P;
    private CharacterActions _ca1P;
    private CharacterActions _ca2P;

    //1�t���[���ŐԃQ�[�W�̌����
    private float redBarSpeed = 0.0025f;

    //�R���{���o�֘A
    private int _conboCounterDisplayFrame = 90; //�R���{���o��������t���[��
    private CancellationTokenSource _comboCounterCTS;

    //���E���h�R�[���֘A
    private CancellationTokenSource _roundCallCTS;

    //�Q�[���Z�b�g�֘A
    private CancellationTokenSource _gameSetCTS;

    /// <summary>
    /// �L�����N�^�[�����ꂼ��ݒ�
    /// </summary>
    public void SetPlayer(CharacterState cs1P, CharacterState cs2P)
    {
        _cs1P = cs1P;
        _cs2P = cs2P;

        _ca1P = cs1P.GetComponent<CharacterActions>();
        _ca2P = cs2P.GetComponent<CharacterActions>();

        //�f���Q�[�g�̓o�^
        _ca1P.ComboCount = ComboCount;
        _ca2P.ComboCount = ComboCount;

        //�ŗL���\�[�X�̐ݒ�
        InitializeUniqueResource(_ca1P);
        InitializeUniqueResource(_ca2P);

        //��
        InstantiateFaceUpImage(_ca1P.CharacterData, 1);
        InstantiateFaceUpImage(_ca2P.CharacterData, 2);
    }

    private void InstantiateFaceUpImage(CharacterData charaData, int playerNum)
    {
        if(playerNum == 1)
        {
            Transform face1P = Instantiate(charaData.FightingFaceUpImage.transform);
            face1P.SetParent(_faceUp1P, false);
        }
        else
        {
            Transform face2P = Instantiate(charaData.FightingFaceUpImage.transform);
            face2P.SetParent(_faceUp2P, false);
            face2P.localPosition *= new Vector2(-1, 1);
            face2P.localScale *= new Vector2(-1, 1);
        }
    }

    /// <summary>
    /// �ŗL�Q�[�W�̐ݒ�
    /// </summary>
    public void InitializeUniqueResource(CharacterActions ca)
    {
        //�N���E�h�̌ŗL���\�[�X�ݒ�
        if(ca is ViolaCloud)
        {
            InstantiateFogMeter(ca.PlayerNum);
        }
    }

    private void Update()
    {
        ApplyBar();
        SPcolorChange();
        UltElectricity();
        UpDateUniqueResource();
    }

    /// <summary>
    /// ���ꂼ��̃Q�[�W���X���C�_�[�ɔ��f������
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
            //�R���{���I���Ό���n�߂�
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
            //�R���{���I���Ό���n�߂�
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
    /// SPBar�̐F��ς���
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
    /// UPBar�����^���̂Ƃ��̉��o
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

    /// <summary>
    /// �ŗL���\�[�X��UI�X�V
    /// </summary>
    private void UpDateUniqueResource()
    {
        if(_ca1P is ViolaCloud cloud1P)
        {
            _fogMeterPos1P.value = cloud1P.CurrentFogResource / cloud1P.FogMaxResource;
        }
        if(_ca2P is ViolaCloud cloud2P)
        {
            _fogMeterPos2P.value = cloud2P.CurrentFogResource / cloud2P.FogMaxResource;
        }

        //�ǉ��̌ŗL���\�[�XUI����
    }

    private void InstantiateFogMeter(int playerNum)
    {
        if (playerNum == 1)
        {
            _fogMeterPos1P = Instantiate(_fogMeter);
            _fogMeterPos1P.transform.SetParent(_hudCanvas, false);
            _fogMeterPos1P.value = 1;
        }
        else
        {
            _fogMeterPos2P = Instantiate(_fogMeter);
            _fogMeterPos2P.transform.SetParent(_hudCanvas, false);
            _fogMeterPos2P.GetComponent<RectTransform>().anchoredPosition *= new Vector2(-1, 1);
            _fogMeterPos2P.transform.localScale *= new Vector2(-1, 1);
            _fogMeterPos2P.value = 1;
        }
    }

    public void SetTimeLimitText(string timeString)
    {
        _timeLimitText.text = timeString;
    }

    /// <summary>
    /// �c�@�n�[�g�̕\��
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

        //Fight�J�n
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

        await _panel.DOFade(1f, 1f).ToUniTask(cancellationToken: token);

        GameSetCancel();
    }

    /// <summary>
    /// �R���{���o
    /// </summary>
    private async UniTask ComboCount(int playerNum, int comboNum)
    {
        if (comboNum <= 1) return;

        //�O���UniTask����
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
    /// ���̊ԃR���{���o��\��
    /// </summary>
    private async UniTask ComboCountDisplay(CancellationToken token)
    {
        await UniTask.DelayFrame(_conboCounterDisplayFrame, cancellationToken: token);
        ComboCountCancel();
    }

    /// <summary>
    /// �R���{���o���L�����Z��
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
