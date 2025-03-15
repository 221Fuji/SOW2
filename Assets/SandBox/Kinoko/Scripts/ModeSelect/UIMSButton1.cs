using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class UIMSButton1 : UIPersonalAct
{
    public override async void ClickedAction()
    {
        var characterSelectManager = 
            await GameManager.LoadAsync<CharacterSelectManager>("CharacterSelectScene");
        characterSelectManager.Initialize(GameManager.Player1Device);
    }
    public override void FocusedAction(GameObject _ob)
    {
        _ob.TryGetComponent<UIMSMovingCtrl>(out var movingCtrlClass);
        GameObject yazirusiOb = movingCtrlClass?.ReturnYazirusiOb().transform.gameObject;
        yazirusiOb.SetActive(true);
        movingCtrlClass?.ReturnYazirusiOb().InstanceYazirusi(this.transform.gameObject);
        //yazirusiOb.transform.DOMoveX(yazirusiOb.transform.position.x - 15,1).SetLoops(-1,LoopType.Yoyo).SetEase(Ease.Linear);
    }
    public override void SeparateAction(GameObject _ob)
    {
        _ob.TryGetComponent<UIMSMovingCtrl>(out var movingCtrlClass);
        //movingCtrlClass?.ReturnYazirusiOb().transform.gameObject.transform.DOKill();
        movingCtrlClass?.ReturnYazirusiOb().transform.gameObject.SetActive(false);
    }
}
