using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class UIMSButton : UIPersonalAct
{

    [SerializeField] private string _modeName = "";
    [SerializeField] private string _discription = "";
    protected CancellationTokenSource _cts = new CancellationTokenSource();
    private List<Tween> _tweens = new List<Tween>();

    //å≈óLèàóù
    public UnityAction<GameObject> ClickedActionEvent { get; set; }

    private Vector2 _defaultRect;


    public override async void FocusedAction(GameObject ob)
    {
        ob.TryGetComponent<UIMSMovingCtrl>(out var movingCtrlClass);

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

        _defaultRect = transform.parent.GetComponent<RectTransform>().localScale;
        transform.parent.GetComponent<RectTransform>().DOScale(new Vector2(1.1f,1.1f),0.5f).SetEase(Ease.OutBack).ToUniTask(cancellationToken:token).Forget();


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
        catch
        {}
    }
    public override void SeparateAction(GameObject ob)
    {
        transform.parent.GetComponent<RectTransform>().localScale = _defaultRect;
        _cts.Cancel();
        ob.TryGetComponent<UIMSMovingCtrl>(out var movingCtrlClass);
        movingCtrlClass?.ReturnKettei().ResetAnim();

        GameObject yazirusiOb = movingCtrlClass?.ReturnYazirusiOb().transform.gameObject;
        GameObject floorTopOb = movingCtrlClass?.ReturnFloorTop().transform.gameObject;
        GameObject floorUnderOb = movingCtrlClass?.ReturnFloorUnder().transform.gameObject;

        floorTopOb.GetComponent<UnityEngine.UI.Image>().color = new Color(1,1,1,0f);
        floorUnderOb.GetComponent<UnityEngine.UI.Image>().color = new Color(1,1,1,0f);
        yazirusiOb.transform.gameObject.SetActive(false);
        floorTopOb.transform.gameObject.SetActive(false);
        floorUnderOb.transform.gameObject.SetActive(false);
    }

    public override void ClickedAction(GameObject ob)
    {
        ClickedActionEvent?.Invoke(ob);
    }
}
