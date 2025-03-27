using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMSMovingCtrl : UIMovingCtrl
{
    [SerializeField] private UIMSModeNameTx _modeNameMainTx;
    [SerializeField] private UIMSDiscriptionTx _discriptionTx;
    [SerializeField] private UIMSYazirusi _yazirusi;
    [SerializeField] private UIMSFloorTop _floorTop;
    [SerializeField] private UIMSFloorUnder _floorUnder;
    [SerializeField] private UIMSKettei _kettei;

    public void Cancell()
    {
        if(_lockPushed) return;
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

    public UIMSKettei ReturnKettei()
    {
        return _kettei;
    }
}
