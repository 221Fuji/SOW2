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

    private CharacterState _characterState1P;
    private CharacterState _characterState2P;

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
    public void SetPlayer(CharacterState cs1p, CharacterState cs2p)
    {
        _characterState1P = cs1p;
        _characterState2P = cs2p;

        //�f���Q�[�g�̓o�^
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
    /// ���ꂼ��̃Q�[�W���X���C�_�[�ɔ��f������
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
        if (_characterState2P != null)
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
    /// SPBar�̐F��ς���
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
    /// UPBar�����^���̂Ƃ��̉��o
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
    /// �c�@�n�[�g�̕\���i�K���Ȏ����j
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

        await _panel.DOFade(1f, 0.5f).ToUniTask(cancellationToken: token);

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
