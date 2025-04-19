using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalMatchManager : FightingManager
{
    protected async override void GoFighting()
    {
        LocalMatchManager localMatchManager =
            await GameManager.LoadAsync<LocalMatchManager>("FightingScene");
        localMatchManager.StartRound(CurrentRoundData, _playerData1P, _playerData2P);
    }
}
