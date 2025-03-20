

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIMSButton1 : UIMSButton
{
    public override async void ClickedAction(GameObject _ob)
    {
        _ob.TryGetComponent<UIMSMovingCtrl>(out var movingCtrlClass);
        var kettei = movingCtrlClass?.ReturnKettei();
        kettei.StartAnim();
        
        try
        {
            await UniTask.WaitForSeconds(0.7f,cancellationToken: _cts.Token);
            var characterSelectManager = 
                await GameManager.LoadAsync<CharacterSelectManager>("CharacterSelectScene");
            characterSelectManager.Initialize(GameManager.Player1Device);
        }
        catch
        {
        }

    }
}
