
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class UIMSButton1 : UIPersonalAct
{
    [SerializeField] private string _modeName = "";
    [SerializeField] private string _discription = "";
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private List<Tween> _tweens = new List<Tween>();
    
    public override async void ClickedAction()
    {
        var characterSelectManager = 
            await GameManager.LoadAsync<CharacterSelectManager>("CharacterSelectScene");
        characterSelectManager.Initialize(GameManager.Player1Device);
    }
    public override async void FocusedAction(GameObject _ob)
    {
        _ob.TryGetComponent<UIMSMovingCtrl>(out var movingCtrlClass);

        var yazirusi = movingCtrlClass?.ReturnYazirusiOb();
        var floorTop = movingCtrlClass?.ReturnFloorTop();
        var floorUnder = movingCtrlClass?.ReturnFloorUnder();
        var modeNameTxt = movingCtrlClass?.ReturnModeNameMainTx();
        var descriptionTx = movingCtrlClass?.ReturnDiscriptionTx();
        GameObject yazirusiOb = yazirusi?.transform.gameObject;
        GameObject floorTopOb = floorTop?.transform.gameObject;
        GameObject floorUnderOb = floorUnder?.transform.gameObject;

        yazirusiOb.SetActive(true);


        modeNameTxt.StringToText(_modeName);
        descriptionTx.StringToText(_discription);
        yazirusi?.InstanceObject(transform.gameObject);
        floorTop?.InstanceObject(transform.gameObject);
        floorUnder?.InstanceObject(transform.gameObject);

        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;

        modeNameTxt.ResetPosition();
        var titleSequence = DOTween.Sequence();
        _tweens.Add(titleSequence);
        await titleSequence.Append(modeNameTxt.MovingDirection()).ToUniTask(cancellationToken: token);
        await modeNameTxt.FlashText(_modeName,token);
        try
        {
            await UniTask.WaitForSeconds(0.3f,cancellationToken: token);
        }
        catch{}
        var childTitleSequence = DOTween.Sequence();
        _tweens.Add(childTitleSequence);
        try
        {
            await childTitleSequence.Append(modeNameTxt.MovingDirectionChild()).ToUniTask(cancellationToken: token);
        }
        catch{}

        try
        {
            await UniTask.WaitForSeconds(0.5f,cancellationToken:token);
            floorTopOb.SetActive(true);
            floorUnderOb.SetActive(true);
            var sequence = DOTween.Sequence();
            _tweens.Add(sequence);
            await sequence.Append(floorTop.FadeIn())
            .Join(floorUnder.FadeIn())
            .Join(floorTop.Moving())
            .Join(floorUnder.Moving()).ToUniTask(cancellationToken: token);   
        }
        catch(System.Exception e)
        {
            
        }

    }
    public override void SeparateAction(GameObject _ob)
    {
        _cts.Cancel();
        _ob.TryGetComponent<UIMSMovingCtrl>(out var movingCtrlClass);


        GameObject yazirusiOb = movingCtrlClass?.ReturnYazirusiOb().transform.gameObject;
        GameObject floorTopOb = movingCtrlClass?.ReturnFloorTop().transform.gameObject;
        GameObject floorUnderOb = movingCtrlClass?.ReturnFloorUnder().transform.gameObject;

        floorTopOb.GetComponent<UnityEngine.UI.Image>().color = new Color(1,1,1,0f);
        floorUnderOb.GetComponent<UnityEngine.UI.Image>().color = new Color(1,1,1,0f);
        yazirusiOb.transform.gameObject.SetActive(false);
        floorTopOb.transform.gameObject.SetActive(false);
        floorUnderOb.transform.gameObject.SetActive(false);
    }
}
