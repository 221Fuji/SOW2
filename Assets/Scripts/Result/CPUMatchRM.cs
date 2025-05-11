using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUMatchRM : ResultManager
{
    private CPUCharacter _cpu1P;
    private CPUCharacter _cpu2P;


    public override void InitializeRM(int winnerNum, PlayerData pd1, PlayerData pd2)
    {
        base.InitializeRM(winnerNum, pd1, pd2);

        //１ｐ側の入力だけでシーン移れるようにしてほしい
        //２ｐ側の矢印を消す等の演出面も改善可能ならお願いします

        /*
         * ここに必要であればいろいろ追加して
         */
    }

    /// <summary>
    /// CPU戦終了時に呼ばれる
    /// </summary>
    public void SetCPUData(CPUCharacter cpu1P, CPUCharacter cpu2P)
    {
        _cpu1P = cpu1P;
        _cpu2P = cpu2P;
    }

    protected override async void GoCharaSelect()
    {
        try
        {
            var characterSelectManager =
                await GameManager.LoadAsync<CPUMatchCSM>("CharacterSelectScene");
            characterSelectManager.Initialize(GameManager.Player1Device);
        }
        catch
        {
            return;
        }
    }

    protected override async void GoFighting()
    {
        try
        {
            var fightingManager =
                await GameManager.LoadAsync<CPUMatchFM>("FightingScene");
            fightingManager.InitializeCPUMatch(_cpu1P, _cpu2P);
        }
        catch
        {
            return;
        }
    }
}
