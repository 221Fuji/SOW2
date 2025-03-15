using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.Linq;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CharacterSelectManager : ModeManager
{

    [SerializeField] private CharacterSelectController _csc1P;
    [SerializeField] private CharacterSelectController _csc2P;

    private CancellationTokenSource _goFightingCTS;

    //�f�o�b�O
    [SerializeField] private CharacterData _lancer;
    [SerializeField] private CharacterData _succubus;

    public override void Initialize(InputDevice device)
    {
        base.Initialize(device);

        SetDelegate(_player1Input.GetComponent<OtherInputReceiver>(), _csc1P);

        InputSystem.onEvent += OnInput2P;

        GoFighting();
    }

    //2P���̃f�o�C�X���m
    private void OnInput2P(InputEventPtr eventPtr, InputDevice device)
    {
        // �L�[�{�[�h�ƃp�b�h����
        if (!(device is Keyboard) && !(device is Gamepad)) return;

        if (_player1Input.devices.Contains(device)||
            _player2Input != null) return;

        InstantiatePlayer2Input(device);
        Debug.Log("2P���̃f�o�C�X��o�^" + _player2Input.devices);
        SetDelegate(_player2Input.GetComponent<OtherInputReceiver>(), _csc2P);
    }

    //PlayerInput�̃f���Q�[�g�ݒ�
    private void SetDelegate(OtherInputReceiver oir, CharacterSelectController csc)
    {
        oir.Accept = csc.Accept;
        oir.Cancel = csc.Cancel;
    }

    private async void GoFighting()
    {
        _goFightingCTS = new CancellationTokenSource();
        CancellationToken token = _goFightingCTS.Token;

        await UniTask.WaitUntil(() =>
        {
            return _csc1P.Selected && _csc2P.Selected;
        }, cancellationToken : token);

        //FightingScene�Ɉڍs
        var vm = await GameManager.LoadAsync<VersusManager>("VersusScene");
        vm.VersusPerformance(_lancer, _succubus);
    }
}
