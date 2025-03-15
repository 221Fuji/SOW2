using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMSMovingCtrl : UIMovingCtrl
{
    [SerializeField] UIMSYazirusi _yazirusi;

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
}
