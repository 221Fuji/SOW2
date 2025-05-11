using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class UICSMovingCtrl : UIMovingCtrl
{
    [SerializeField] private CharacterDataBase _database;
    [SerializeField] private TextMeshProUGUI _characterNameJField;
    [SerializeField] private TextMeshProUGUI _characterNameEField;
    [SerializeField] private GameObject _1PFrameTop;
    [SerializeField] private GameObject _2PFrameTop;
    [SerializeField] private GameObject _MixFrameTop;
    [SerializeField] private GameObject _1PFrameFloor;
    [SerializeField] private GameObject _2PFrameFloor;
    [SerializeField] private GameObject _MixFrameFloor;
    [SerializeField] private GameObject _guideField;
    [SerializeField] private UICSDotObjectField _dotObjectField;
    [SerializeField] private UICSFigureBox _figureBox;
    [SerializeField] private UICSStreamText _streamTxtLarge;
    [SerializeField] private UICSStreamText _streamTxtMedium;
    [SerializeField] private UICSStreamText _streamTxtSmall;
    [SerializeField] private UICSReadyTxt _readyTxt;
    [SerializeField] private UICSMovingCtrl _rivalMovingCtrl;
    [SerializeField] private UICSSkillListCtrl _skillListCtrl;
    [SerializeField] private int _playerNum = 0;

    [SerializeField] private GameObject _controllerGuide;
    [SerializeField] private GameObject _keyBoardGuide;

    private List<CharacterData> _characterDataList;
    public int PlayerNum{get {return _playerNum;}}
    public CharacterDataBase DataBase{get {return _database;}}
    public TextMeshProUGUI CharacterNameJField{get {return _characterNameJField;}}
    public TextMeshProUGUI CharacterNameEField{get {return _characterNameEField;}}
    public UICSFigureBox FigureBox{get {return _figureBox;}}
    public UICSDotObjectField DotObjectField{get {return _dotObjectField;}}
    public UICSReadyTxt ReadyTxt{get {return _readyTxt;}}
    public CharacterData CharacterData{get; private set;}
    public UnityAction<bool> OnSelected { get; set; }
    public UnityAction<UIMovingCtrl,int> SwitchDelegate { get; set; }
    public UnityAction<int> SwitchAdmin { get; set; }
    public List<CharacterData> CharacterDataList { get { return _characterDataList; } }

    private bool _selected; // キャラクターを選択したか
    private CancellationTokenSource _cts;

    public bool Selected //選択状況を切り替えた時イベントを発火させる
    {
        get { return _selected; }

        private set
        {
            OnSelected?.Invoke(value);
            _selected = value;
        }
    }

    /// <summary>
    /// 対応してるキャラクターデータベースに切り替える
    /// </summary>
    /// <param name="mode">0:Local,1:CPU</param>
    public void SetCharaData(int mode)
    {
        if(mode == 0)
        {
            _characterDataList = _database.CharacterDataList;
        }
        else if(mode == 1)
        {
            _characterDataList = _database.DevedCpuList;
        }

        //このplayerが1じゃなかったらっていう処理(1Pをホストとして位置づけ)結構良くないかも。修正？
        if (PlayerNum != 1) return;
        foreach (Making make in _outMap)
        {
            foreach (UIPersonalAct target in make.ReturnList())
            {
                try
                {
                    if (target is UICSCharaWindow window)
                    {
                        if (!window.Characterdata) window.SetCharacterData(this.transform.gameObject);
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
    }

    protected override void  Awake()
    {
        base.Awake();

        if (PlayerNum != 1) return;
        _cts = new CancellationTokenSource();
        _streamTxtLarge.StreamingText(_cts.Token).Forget();
        _streamTxtMedium.StreamingText(_cts.Token).Forget();
        _streamTxtSmall.StreamingText(_cts.Token).Forget();
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
    }

    public override void DesignatedForcus(Vector2 arrayPos)
    {
        if(CheckAvailable()) return;
        base.DesignatedForcus(arrayPos);
    }

    public override void ForcusDown()
    {
        if(CheckAvailable()) return;
        base.ForcusDown();
    }

    public override void ForcusLeft()
    {
        if(CheckAvailable()) return;
        base.ForcusLeft();
    }

    public override void ForcusRight()
    {
        if(CheckAvailable()) return;
        base.ForcusRight();
    }
    public override void ForcusUp()
    {
        if(CheckAvailable()) return;
        base.ForcusUp();
    }

    public override void OnClick()
    {
        if(CheckAvailable()) return;

        base.OnClick();
        if(_outMap[(int)Forcus.x].ReturnList()[(int)Forcus.y] is UICSCharaWindow window)
        {

            CharacterData = window.Characterdata;
            Debug.Log(CharacterData.CharacterNameJ);
            if(!CharacterData)
            {
                throw new Exception("キャラクターが選択されていません");
            }
            Selected = true;
        }
    }

    public void Cancel()
    {
        if(CheckAvailable() && _rivalMovingCtrl.Selected) return;
        Selected = false;
        //キャラ確定後にもとに戻したい処理があったらここに書く！(※両者選択後は呼ばれない)
        _readyTxt.ResetUI();
    }

    /// <summary>
    /// キャラ詳細を開く時の処理
    /// </summary>
    public override void SwitchtoOtherCtrler()
    {
        if (CheckAvailable()) return;
        SwitchDelegate.Invoke(_skillListCtrl,_playerNum);
        SwitchAdmin.Invoke((int)Forcus.x);
    }

    public CharacterOutFrames ReturnCharacterOutFrames()
    {
        return new CharacterOutFrames(new GameObject[]
        {_1PFrameTop,_2PFrameTop,_MixFrameTop,_1PFrameFloor,_2PFrameFloor,_MixFrameFloor});
    }

    private bool CheckAvailable()
    {
        if(Selected) return true;
        return false;
    }


    public struct CharacterOutFrames
    {
        public GameObject _1PFrameTop{get; private set;}
        public GameObject _2PFrameTop{get; private set;}
        public GameObject _MixFrameTop{get; private set;}
        public GameObject _1PFrameFloor{get; private set;}
        public GameObject _2PFrameFloor{get; private set;}
        public GameObject _MixFrameFloor{get; private set;}
        public CharacterOutFrames(GameObject[] objects)
        {
            _1PFrameTop = objects[0];
            _2PFrameTop = objects[1];
            _MixFrameTop = objects[2];
            _1PFrameFloor = objects[3];
            _2PFrameFloor = objects[4];
            _MixFrameFloor = objects[5];
        }
    }

}
