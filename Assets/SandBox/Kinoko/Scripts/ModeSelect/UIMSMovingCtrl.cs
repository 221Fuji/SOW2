using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMSMovingCtrl : UIMovingCtrl
{
    [SerializeField] GameObject _modeNameMainTx;
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

    public GameObject ReturnModeNameMainTx()
    {
        return _modeNameMainTx;
    }
}
