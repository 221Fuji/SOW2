using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMSMovingCtrl : UIMovingCtrl
{
    [SerializeField] UIMSModeNameTx _modeNameMainTx;
    [SerializeField] UIMSDiscriptionTx _discriptionTx;
    [SerializeField] UIMSYazirusi _yazirusi;
    [SerializeField] UIMSFloorTop _floorTop;
    [SerializeField] UIMSFloorUnder _floorUnder;

    public override void ForcusUp()
    {
        base.ForcusUp();
    }

    public override void ForcusDown()
    {
        base.ForcusDown();
    }

    public UIMSYazirusi ReturnYazirusiOb()
    {
        return _yazirusi;
    }
    public UIMSFloorTop ReturnFloorTop()
    {
        return _floorTop;
    }

    public UIMSFloorUnder ReturnFloorUnder()
    {
        return _floorUnder;
    }

    public UIMSModeNameTx ReturnModeNameMainTx()
    {
        return _modeNameMainTx;
    }

    public UIMSDiscriptionTx ReturnDiscriptionTx()
    {
        return _discriptionTx;
    }
}
