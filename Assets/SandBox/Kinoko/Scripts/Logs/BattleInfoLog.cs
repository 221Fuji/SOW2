using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BattleInfoLog
{
    private string _path = "KinokoLogs";
    //ÉoÉgÉãÇÃIDÇçÏê¨ÇµÅAë„ì¸Ç∑ÇÈ
    private string _battleId = "";
    private string _p1Chara = "";
    private string _p2Chara = "";

    private List<string> _p1Log = new List<string>();
    private List<string> _p2Log = new List<string>();

    public BattleInfoLog()
    {
        _battleId = Guid.NewGuid().ToString();
    }

    public void AddLog(int playerNum, string data, string charaName)
    {
        if (playerNum == 1)
        {
            _p1Log.Add(data);
            _p1Chara = charaName;
        }
        else if (playerNum == 2)
        {
            _p2Log.Add(data);
            _p2Chara = charaName;
        }
    }

    public void ArrangeForFile()
    {
        if(!Directory.Exists(_path)) Directory.CreateDirectory(_path);

        Directory.CreateDirectory(_path + @"\" + _battleId);

        using (var fs = new StreamWriter(_path +@"\"+ _battleId + @"\" + _p1Chara + "1" + ".txt", false, System.Text.Encoding.GetEncoding("UTF-8")))
        {
            fs.Write(_p1Log[0]);
        }

        using (var fs = new StreamWriter(_path +@"\" + _battleId + @"\" + _p2Chara + "2" + ".txt",false, System.Text.Encoding.GetEncoding("UTF-8")))
        {
            fs.Write(_p2Log[0]);
        }
    }
}
