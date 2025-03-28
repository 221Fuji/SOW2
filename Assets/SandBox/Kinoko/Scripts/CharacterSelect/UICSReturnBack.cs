using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UICSReturnBack : UIPersonalAct
{
    [SerializeField] private GameObject _flame;
    public UnityAction ClickedActionEvent{get; set;}

    public override bool MovingException(GameObject ob)
    {
        UICSMovingCtrl ctrl = ob.GetComponent<UICSMovingCtrl>();
        Vector2 checkPos = new Vector2(-1,-1);
        if(ctrl.Casted.x == -1 && ctrl.Casted.y == -1){
            checkPos = ctrl.Forcus;
        }
        else
        {
            checkPos = ctrl.Casted;
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

    public override void ClickedAction(GameObject ob)
    {
        ClickedActionEvent?.Invoke();
    }
}
