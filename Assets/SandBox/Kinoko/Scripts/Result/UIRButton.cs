using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class UIRButton : UIPersonalAct 
{
    [SerializeField] private int _priority;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private int _selectedPlayer = 0;
    public UnityAction ClickedActionEvent { get; set; }
    public override void FocusedAction(GameObject ob)
    {
        ob.TryGetComponent<UIRMovingCtrl>(out var movingCtrlClass);

        var araw = movingCtrlClass?.ArawSymbol;
        araw.InstantiateObj(gameObject, _cts.Token);
    }

    public override void SeparateAction(GameObject ob)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
    }

    public override void ClickedAction(GameObject ob)
    {
        if (!ob.TryGetComponent<UIRMovingCtrl>(out var movingCtrlClass)) return;
        //優先度0は無条件でシーン遷移

        //優先度1は他プレイヤーが選択していた場合、何を選択していてもシーンが遷移する
        if(_priority == 1)
        {
            if (!(bool)movingCtrlClass?.RivalSelected) throw new Exception("Didn't pass >> !RivalSelected");
        }
        //優先度2は両者プレイヤーが同じ選択をしていた場合のみシーン遷移
        else if(_priority == 2)
        {
            _selectedPlayer += (int)movingCtrlClass?.PlayerNum;
            if(movingCtrlClass?.RivalSelevtedButton.GetComponent<UIRButton>()._priority == 1)
            {
                movingCtrlClass?.RivalSelevtedButton.GetComponent<UIRButton>().DoClickedActionEvent();
                throw new Exception("DoAction");
            }

            if (_selectedPlayer != 3) throw new Exception("Didn't pass >> _selectedPlayer != 3");
        }

        ClickedActionEvent?.Invoke();
    }

    public void DoClickedActionEvent()
    {
        ClickedActionEvent?.Invoke();
    }
    public void CancelledAction(GameObject ob) 
    {
        if (!ob.TryGetComponent<UIRMovingCtrl>(out var movingCtrlClass)) return;
        if(_selectedPlayer <= 0) _selectedPlayer -= movingCtrlClass.PlayerNum;
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
    }

    
}
