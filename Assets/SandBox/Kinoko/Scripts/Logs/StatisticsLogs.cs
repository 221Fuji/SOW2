using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StatisticsLogs
{
    private string _path = "StatisticsLog.txt";
    public void WritingTxt(int _writedTxt)
    {
        //��������true���ǋL
        using (var fs = new StreamWriter(_path, true, System.Text.Encoding.GetEncoding("UTF-8")))
        {
            fs.Write(_writedTxt + ",");
        }
    }

    /// <summary>
    /// (������:�������ق��̃f�[�^,������:�������ق��̃f�[�^)
    /// </summary>
    public int ArrangeResult(CharacterData winnersData,CharacterData loserData)
    {
        return (winnersData.MyNumber * 2 + loserData.MyNumber);
    }
}
