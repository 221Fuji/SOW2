using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class FightingUI : MonoBehaviour
{
    [Header("�Q�[�W")]
    [SerializeField] Slider _hpSlider1P;
    [SerializeField] Slider _hpSlider2P;
    [SerializeField] Slider _redSlider1P;
    [SerializeField] Slider _redSlider2P;
    [SerializeField] Slider _spSlider1P;
    [SerializeField] Slider _spSlider2P;
    [SerializeField] Slider _upSlider1P;
    [SerializeField] Slider _upSlider2P;
    [Space]
    [Header("�R���{���o")]
    [SerializeField] TextMeshProUGUI _comboCounter1P;
    [SerializeField] TextMeshProUGUI _comboCounter2P;

    private CharacterState _characterState1P;
    private CharacterState _characterState2P;

    //1�t���[���ŐԃQ�[�W�̌����
    float redBarSpeed = 0.0025f;

    //�R���{���o�֘A
    int _conboCounterDisplayFrame = 90; //�R���{���o��������t���[��
    CancellationTokenSource _comboCounterCTS;

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
    }

    /// <summary>
    /// ���ꂼ��̃Q�[�W���X���C�_�[�ɔ��f������
    /// </summary>
    private void ApplyBar()
    {
        //SPBer
        if(_characterState1P != null && _spSlider1P != null)
        {
            _spSlider1P.value = _characterState1P.CurrentSP / _characterState1P.MaxSP;
        }
        if (_characterState2P != null && _spSlider2P != null)
        {
            _spSlider2P.value = _characterState2P.CurrentSP / _characterState2P.MaxSP;
        }

        //HPBer      
        if (_characterState1P != null && _hpSlider1P != null)
        {
            _hpSlider1P.value = _characterState1P.CurrentHP / _characterState1P.MaxHP;
        }
        if (_characterState2P != null && _hpSlider2P != null)
        {
            _hpSlider2P.value = _characterState2P.CurrentHP / _characterState2P.MaxHP;
        }

        //RedBar
        if(_characterState1P != null && _redSlider1P != null)
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
        if (_characterState2P != null && _redSlider2P != null)
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
        if (_characterState1P != null && _upSlider1P != null)
        {
            _upSlider1P.value = _characterState1P.CurrentUP / _characterState1P.MaxUP;
        }
        if(_characterState2P != null && _upSlider2P != null)
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

        if(_characterState1P.AnormalyStates.Contains(AnormalyState.Fatigue))
        {
            imageSP1P.color = new Color(0.75f, 0.75f, 0.75f);
        }
        else
        {
            imageSP1P.color = new Color(1, _spSlider1P.value, 0);
        }

        if (_characterState2P.AnormalyStates.Contains(AnormalyState.Fatigue))
        {
            imageSP2P.color = new Color(0.75f, 0.75f, 0.75f);
        }
        else
        {
            imageSP2P.color = new Color(1, _spSlider2P.value, 0);
        }
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
    
}
