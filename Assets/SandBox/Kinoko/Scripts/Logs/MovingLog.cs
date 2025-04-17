using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovingLog : MonoBehaviour
{
    [SerializeField] private bool _isDebugRec;
    private bool _recFlag;
    //入力を保存する配列
    //"継続フレーム数,移動,ガード,ジャンプ,通常,必殺1,必殺2,超必
    private List<string> _recInputArray = new List<string>();
    private BattleInfoLog _battleInfoLog;
    public BattleInfoLog BattleInfoLog { get { return _battleInfoLog; } }

    //入力関係の変数
    private float _walkvalue = 0f;
    private float _guard = 0f;
    private bool _jump;
    private bool _normalMove;
    private bool _specialMove1;
    private bool _specialMove2;
    private bool _ultimate;

    public void Awake()
    {
        ManagedState();
    }

    public void RegisterLogs()
    {
        ManagedState();
        string str = ArrayToTxt();
        int pNum = GetComponent<CharacterActions>().PlayerNum;
        string charaName = GetComponent<CharacterActions>().name;
        BattleInfoLog.AddLog(pNum, str, charaName);
    }

    public void SetBattleInfoLog(BattleInfoLog bil)
    {
        _battleInfoLog = bil;
    }

    public void ManagedState()
    {
        if (!_isDebugRec) return;
        if (!_recFlag)
        {
            _recFlag = true;
            return;
        }
        else
        {
            _recFlag = false;
            
        }
    }

    void FixedUpdate()
    {
        if (!_isDebugRec) return;
        if (_recInputArray.Count == 0)
        {
            _recInputArray.Add(Time.frameCount.ToString());
            return;
        }

        string justBefore = "";
        if (_recInputArray.Count > 1) justBefore = _recInputArray[_recInputArray.Count - 1];

        string now = "";
        now += _walkvalue.ToString() + ",";
        now += _guard.ToString() + ",";
        now += _jump.ToString() + ",";
        now += _normalMove.ToString() + ",";
        now += _specialMove1.ToString() + ",";
        now += _specialMove2.ToString() + ",";
        now += _ultimate.ToString();

        int continueFlame = 0;
        if(justBefore != "")
        {
            string[] justBefores = justBefore.Split(',');
            int beforeFlame = int.Parse(justBefores[0]);
            string pure = "";
            for (int i = 1; i < justBefores.Length; i++)
            {
                if (i == justBefores.Length - 1)
                {
                    pure += justBefores[i];
                }
                else
                {
                    pure += justBefores[i] + ",";
                }
            }

            if(now == pure)
            {
                continueFlame += (beforeFlame + 1);
            }
        }

        string registered = continueFlame.ToString() + "," + now;
        if (continueFlame == 0)
        {
            _recInputArray.Add(registered);
        }
        else _recInputArray[_recInputArray.Count - 1] = registered;

        _jump = false;
        _normalMove = false;
        _specialMove1 = false;
        _specialMove2 = false;
        _ultimate = false;
    }

    public string ArrayToTxt()
    {
        //まず配列をstring型にする
        string toString = "";
        foreach(string str in _recInputArray)
        {
            toString += str + Environment.NewLine;
        }

        return toString;
        string motherName = gameObject.name;

        /*
        //テキストデータとして出力
        try
        {
            using (var fs = new StreamWriter(motherName + ".txt", false, System.Text.Encoding.GetEncoding("UTF-8")))
            {
                fs.Write(toString);
            }
        }
        catch { }
        */
    }

    public void OnFourDirections(InputValue value)
    {
        if(value.Get<Vector2>().x > 0)
        {
            _walkvalue = 1f;
        }
        else if(value.Get<Vector2>().x < 0)
        {
            _walkvalue = -1f;
        }
        else
        {
            _walkvalue = 0f;
        }
    }

    public void OnJump(InputValue value)
    {
        _jump = true;
    }

    public void OnNormalMove()
    {
        _normalMove = true;
    }

    public void OnSpecialMove1()
    {
        _specialMove1 = true;
    }

    public void OnSpecialMove2()
    {
        _specialMove2 = true;
    }

    public void OnUltimate()
    {
        _ultimate = true;
    }

    public void OnGuard(InputValue inputValue)
    {
        _guard = inputValue.Get<float>();
    }
}
