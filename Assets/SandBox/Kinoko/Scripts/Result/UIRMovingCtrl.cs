using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class UIRMovingCtrl : UIMovingCtrl
{
    [SerializeField] private UIRMovingCtrl _rivalMCtrl;
    [SerializeField] private UIRAraw _arawSymbol;
    [SerializeField] private int _playerNum;
    [SerializeField] private UIRBackMeter _backMeter;
    public UIRAraw ArawSymbol { get { return _arawSymbol; }}
    public bool Selected { get; private set; }
    public GameObject SelectedButton { get; private set; }
    public bool RivalSelected { get { return (bool)_rivalMCtrl?.Selected; } }
    public GameObject RivalSelevtedButton { get { return _rivalMCtrl?.SelectedButton; } }
    public int PlayerNum { get { return _playerNum;} }
    public UIRBackMeter BackMeter { get { return _backMeter; }}

    private bool isActive;

    protected override void Awake()
    {

    }

    public void ActivateThis()
    {
        isActive = true;
        ArawSymbol.gameObject.SetActive(true);
        DesignatedForcus(_startPos);
    }

    public override void DesignatedForcus(Vector2 arrayPos)
    {
        if (!isActive) return;
        if (Selected) return;
        Debug.Log("DEsignated" + Forcus +"ArrayPos>>" + arrayPos);
        base.DesignatedForcus(arrayPos);
    }

    public override void ForcusUp()
    {
        if (!isActive) return;
        if (Selected) return;
        base.ForcusUp();
    }

    public override void ForcusDown()
    {
        if (!isActive) return;
        if (Selected) return;
        base.ForcusDown();
    }

    public override void ForcusLeft()
    {
        if (!isActive) return;
        if (Selected) return;
        base.ForcusLeft();
    }

    public override void ForcusRight()
    {
        if (!isActive) return;
        if (Selected) return;
        base.ForcusRight();
    }

    public override void OnClick()
    {
        if (!isActive) return;
        if (Selected) return;
        Selected = true;
        try
        {
            base.OnClick();
        }
        catch
        { }
        SelectedButton = _outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y].gameObject;
    }

    public void PrioritySetForCPU()
    {
        foreach(Making making in _outMap)
        {
            foreach(UIPersonalAct item in making.ReturnList())
            {
                if(item is UIRButton button)
                {
                    button.ChangePriority(0);
                }
            }
        }
    }

    public void Cancell()
    {
        if (!isActive) return;
        if (RivalSelected && Selected) return;
        if(_outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y] is UIRButton button) button.CancelledAction(gameObject);
        Selected = false;
    }
}
