using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMSReturnBack : UIPersonalAct
{
    [SerializeField] private GameObject _flame;
    public override bool MovingException(UIMovingCtrl _ctrl)
    {
        Vector2 checkPos = new Vector2(-1,-1);
        if(_ctrl._casted.x == -1 && _ctrl._casted.y == -1){
            checkPos = _ctrl._forcus;
        }
        else
        {
            checkPos = _ctrl._casted;
        }

        if((int)checkPos.y == _ctrl.ReturnArrayLength() - 1)
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
