using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public static class StageParameter
{
    public static float StageLength { get; } = 34.5f;

    public static float CurrentRightWallPosX { get; private set; }

    public static float CurrentLeftWallPosX { get; private set; }
    public static float GroundPosY { get; } = -3.9f;

    public static void SetCurrentWallPos(float rightWallPosX, float leftWallPosX)
    {
        CurrentLeftWallPosX = leftWallPosX;
        CurrentRightWallPosX = rightWallPosX;
    }
}
