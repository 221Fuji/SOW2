using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultManager : ModeManager
{
    public void InitializeRM(int winnerNum)
    {
        Initialize(GameManager.Player1Device);
        InstantiatePlayer2Input(GameManager.Player2Device);
        WinPerformance(winnerNum);
    }

    private void WinPerformance(int winnerNum)
    {

    }
}
