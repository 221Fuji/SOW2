using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UICSSkillBox : UIPersonalAct
{
    [SerializeField] private TextMeshProUGUI _skillName;
    [SerializeField] private TextMeshProUGUI _operationGuide;
    [SerializeField] private GameObject _skillButtonField;
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private TextMeshProUGUI _skillDiscription;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private float _moveSpeed = 0.2f;
    private bool _forcusDirection = false;
    private Vector2[] _moveTargetPos = { new Vector2(10, -60), new Vector2(-20, 120) };

    public void SetData(CmdListBox listSource)
    {
        _skillName.text = listSource.SkillName;
        _operationGuide.text = listSource.OperationGuide;
        UIField skillField = _skillButtonField.GetComponent<UIField>();
        skillField.ChangeActionType(listSource.SkillOpEnum);
        skillField.ChangedIcon();
        _videoPlayer.clip = listSource.SkillVideo;
        _skillDiscription.text = listSource.SkillDiscription;
    }

    public override void FocusedAction(GameObject ob)
    {
        SeparateOnComplete();
        Vector2 startPosition = new Vector2(0, 0);
        if (_forcusDirection) startPosition = _moveTargetPos[1];
        else startPosition = _moveTargetPos[0];

        ob.TryGetComponent<UICSSkillListCtrl>(out var uicsSkillClass);
        GameObject backObj = uicsSkillClass?.SkillboxBack;

        if (backObj == null)
        {
            Debug.Log("backObj is null");
            return;
        }
        transform.GetComponent<RectTransform>().anchoredPosition = startPosition;
        gameObject.SetActive(true);
        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;
        var sequence = DOTween.Sequence();
        transform.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, 0), _moveSpeed).ToUniTask(cancellationToken: token);

    }

    public override void SeparateAction(GameObject ob)
    {
        CancellationToken token = _cts.Token;
        UICSSkillListCtrl ctrl = ob.GetComponent<UICSSkillListCtrl>();
        bool direction = SearchDirection(ctrl);
        Vector2 endPosition = new Vector2(-1, -1);
        if (direction) endPosition = _moveTargetPos[0];
        else endPosition = _moveTargetPos[1];

        transform.GetComponent<RectTransform>().DOAnchorPos(endPosition, _moveSpeed)
            .OnComplete(SeparateOnComplete).ToUniTask(cancellationToken: token);
    }

    public override bool MovingException(GameObject ob)
    {
        UICSSkillListCtrl ctrl = ob.GetComponent<UICSSkillListCtrl>();
        bool direction = SearchDirection(ctrl);
        _forcusDirection = direction;
        return false;
    }

    private bool SearchDirection(UICSSkillListCtrl ctrl)
    {
        Vector2 checkPos = new Vector2(0, 0);
        if (ctrl.Casted.x == -1 && ctrl.Casted.y == -1)
        {
            checkPos = ctrl.Forcus;
        }
        else
        {
            checkPos = ctrl.Casted;
        }
        float checkPosY = checkPos.y;
        float searchPosY = ctrl.Search.y;
        int maxIndex = ctrl.ReturnArrayLength() - 1;

        if ((searchPosY < checkPosY || (checkPosY == 0 && searchPosY == maxIndex)) && !(checkPosY == maxIndex && searchPosY == 0))
        {
            return false;
        }
        else if (searchPosY > checkPosY || (checkPosY == maxIndex && searchPosY == 0))
        {
            return true;
        }

        throw new System.Exception("world is broken");
    }



    public void SeparateOnComplete()
    {
        gameObject.SetActive(false);
        _cts?.Cancel();
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>è„ì¸óÕÇÕtrue,â∫ì¸óÕÇÕfalse</returns>

    private void OnDestroy()
    {
        _cts.Cancel();
    }
}
