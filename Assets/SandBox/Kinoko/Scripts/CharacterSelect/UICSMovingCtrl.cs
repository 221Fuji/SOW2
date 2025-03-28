using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

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
    [SerializeField] private UICSDotObjectField _dotObjectField;
    [SerializeField] private UICSFigureBox _figureBox;
    [SerializeField] private UICSStreamText _streamTxtLarge;
    [SerializeField] private UICSStreamText _streamTxtMedium;
    [SerializeField] private UICSStreamText _streamTxtSmall;
    [SerializeField] private UICSReadyTxt _readyTxt;
    [SerializeField] private UICSMovingCtrl _rivalMovingCtrl;
    [SerializeField] private int _playerNum = 0;

    public int PlayerNum{get {return _playerNum;}}
    public CharacterDataBase DataBase{get {return _database;}}
    public TextMeshProUGUI CharacterNameJField{get {return _characterNameJField;}}
    public TextMeshProUGUI CharacterNameEField{get {return _characterNameEField;}}
    public UICSFigureBox FigureBox{get {return _figureBox;}}
    public UICSDotObjectField DotObjectField{get {return _dotObjectField;}}
    public UICSReadyTxt ReadyTxt{get {return _readyTxt;}}

    public bool Selected {get; private set;}
    public CharacterData CharacterData{get; private set;}

    protected override void  Awake()
    {
        foreach(Making make in _outMap)
        {
            foreach(UIPersonalAct target in make.ReturnList())
            {
                try
                {
                    if(target is UICSCharaWindow window)
                    {
                        if(!window.Characterdata)    window.SetCharacterData(this.transform.gameObject);    
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
        base.Awake();
        
        if(PlayerNum == 2) return;
        CancellationTokenSource cts = new CancellationTokenSource();
        _streamTxtLarge.StreamingText(cts.Token);
        _streamTxtMedium.StreamingText(cts.Token);
        _streamTxtSmall.StreamingText(cts.Token);
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
        
        //コントローラーの取得
        //自分のデバイスを取得
        InputDevice player1Device = GameManager.Player1Device;
        //デバイスが何か？
        if(player1Device is Keyboard)
        if(player1Device is Gamepad)
        if(player1Device is Joystick)

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

    public void Cancell()
    {
        if(CheckAvailable() && _rivalMovingCtrl.Selected) return;
        Selected = false;
        //キャラ確定後にもとに戻したい処理があったらここに書く！(※両者選択後は呼ばれない)
        //readyTxt.ResetUI();
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
