using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ‘Îí’†‚Ì”wŒiî•ñ‚ª‚Ü‚Æ‚ß‚ç‚ê‚Ä‚¢‚é
/// </summary>
[CreateAssetMenu]
public class StageData : ScriptableObject
{
    [SerializeField] private GameObject _stage;
    [SerializeField] private GameObject _backgroundGround;

    /// <summary>
    /// backgroundManager‚Ì‰º‚É”wŒi‚ğ¶¬‚·‚é
    /// </summary>
    public ParallaxBackground[] GeneratePBG(Transform backgroundManager)
    {
        // _stage‚ÌPrefab‚ğ¶¬
        GameObject stageInstance = Instantiate(_stage, backgroundManager);

        // qƒIƒuƒWƒFƒNƒg‚©‚çParallaxBackground‚ğ‘S•”æ“¾
        ParallaxBackground[] result = stageInstance.GetComponentsInChildren<ParallaxBackground>(true);

        return result;
    }

    /// <summary>
    /// ”wŒi‚Ì’n–Ê‚ğ¶¬‚·‚é
    /// </summary>
    public void GenerateBGG()
    {
        Instantiate(_backgroundGround);
    }
}
