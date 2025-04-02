using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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


    public override void DesignatedForcus(Vector2 arrayPos)
    {
        if (Selected) return;
        base.DesignatedForcus(arrayPos);
    }

    public override void ForcusUp()
    {
        if (Selected) return;
        base.ForcusUp();
    }

    public override void ForcusDown()
    {
        if (Selected) return;
        base.ForcusDown();
    }

    public override void ForcusLeft()
    {
        if (Selected) return;
        base.ForcusLeft();
    }

    public override void ForcusRight()
    {
        if (Selected) return;
        base.ForcusRight();
    }

    public override void OnClick()
    {
        if (Selected) return;
        Selected = true;
        try
        {
            base.OnClick();
        }
        catch(Exception e)
        {

        }
        SelectedButton = _outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y].gameObject;
    }

    public void Cancell()
    {
        if (RivalSelected && Selected) return;
        if(_outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y] is UIRButton button) button.CancelledAction(gameObject);
        Selected = false;
    }
}
