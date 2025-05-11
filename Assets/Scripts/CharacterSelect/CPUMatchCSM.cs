using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// CPUMatchのキャラセレ
/// </summary>
public class CPUMatchCSM : CharacterSelectManager
{
    public override void Initialize(InputDevice device)
    {
        base.Initialize(device);
        _csMovingCtrl1P.OnSelected = On1PSelected;
        _csMovingCtrl1P.SetCharaData(1);
    }

    private void On1PSelected(bool isSelected)
    {
        Debug.Log($"1P選択状況変更>>{isSelected}");

        _oir1P.RemoveDelegate();
        if (isSelected)
        {
            _oir1P.Cancel += _csMovingCtrl1P.Cancel;
            SetDelegate(_player1Input.GetComponent<OtherInputReceiver>(), _csMovingCtrl2P);
        }
        else
        {
            SetDelegate(_player1Input.GetComponent<OtherInputReceiver>(), _csMovingCtrl1P);
        }
    }

    /// <summary>
    /// CPU戦は自動でplayer1になる。引数2は考慮されない。
    /// </summary>
    protected override void SwitchDelegate(UIMovingCtrl movingCtrl, int playerNum)
    {
        _oir1P.RemoveDelegate();
        SetDelegate(_oir1P, movingCtrl);
    }

    protected override async void GoFighting()
    {
        _goFightingCTS = new CancellationTokenSource();
        CancellationToken token = _goFightingCTS.Token;

        try
        {
            await UniTask.WaitUntil(() =>
            {
                return _csMovingCtrl1P.Selected && _csMovingCtrl2P.Selected;
            }, cancellationToken: token);

            CharacterData chara1P = _csMovingCtrl1P.CharacterData;
            CharacterData chara2P = _csMovingCtrl2P.CharacterData;

            await UniTask.WaitForSeconds(0.8f, cancellationToken: token);
            //FightingSceneに移行
            var vm = await GameManager.LoadAsync<VersusManager>("VersusScene");

            CPUCharacter cpu1P = new CPUCharacter(_csMovingCtrl1P.CharacterData, CPUCharacter.CPULevel.Player);
            CPUCharacter cpu2P = new CPUCharacter(_csMovingCtrl2P.CharacterData, CPUCharacter.CPULevel.Easy);
            vm.InitializeCPUMode(cpu1P, cpu2P);
        }
        catch { }
    }
}
