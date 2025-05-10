using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUMatchRM : ResultManager
{
    private CPUCharacter _cpu1P;
    private CPUCharacter _cpu2P;

    public void InitializeCPU(CPUCharacter cpu1P, CPUCharacter cpu2P)
    {
        _cpu1P = cpu1P;
        _cpu2P = cpu2P;

        /*
         * 
         *‚±‚±‚©‚ç‚PP“ü—Í‚¾‚¯‚Å‘I‘ðŽˆ‚ð‘I‚×‚é‚æ‚¤‚É‚µ‚Ä‚Ù‚µ‚¢
         * 
         * 
        */
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
