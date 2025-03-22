using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMSReturnBack : UIPersonalAct
{
    [SerializeField] private GameObject _flame;
    public override bool MovingException(GameObject ob)
    {
        UIMSMovingCtrl ctrl = ob.GetComponent<UIMSMovingCtrl>();
        Vector2 checkPos = new Vector2(-1,-1);
        if(ctrl._casted.x == -1 && ctrl._casted.y == -1){
            checkPos = ctrl._forcus;
        }
        else
        {
            checkPos = ctrl._casted;
        }

        if((int)checkPos.y == ctrl.ReturnArrayLength() - 1)
        {
            return true;
        }
        return false;

    }

    public override void FocusedAction(GameObject _ob)
    {
        _flame.SetActive(true);
    }

    public override void SeparateAction(GameObject _ob)
    {
        _flame.SetActive(false);
    }
}
