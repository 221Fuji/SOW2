using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StatisticsLogs
{
    private string _path = "StatisticsLog.txt";
    public void WritingTxt(int _writedTxt)
    {
        //第二引数はtrueが追記
        using (var fs = new StreamWriter(_path, true, System.Text.Encoding.GetEncoding("UTF-8")))
        {
            fs.Write(_writedTxt + ",");
        }
    }

    /// <summary>
    /// (第一引数:勝ったほうのデータ,第二引数:負けたほうのデータ)
    /// </summary>
    public int ArrangeResult(CharacterData winnersData,CharacterData loserData)
    {
        return (winnersData.MyNumber * 2 + loserData.MyNumber);
    }
}
