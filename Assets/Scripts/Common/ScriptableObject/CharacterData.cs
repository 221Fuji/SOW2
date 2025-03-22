using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    [Header("‰pŒê–¼")]
    [SerializeField] private string _characterNameE;
    [Header("“ú–{–¼")]
    [SerializeField] private string _characterNameJ;
    [Header("ƒLƒƒƒ‰‚ÌPrefab")]
    [SerializeField] private CharacterActions _characterPrefab;
    [Header("VersusScene‚Å‚ÌImage")]
    [SerializeField] private Image _versusEyeForcusImage;
    [SerializeField] private Image _versusStandingImage;
    [SerializeField] private Image _versusGrayImage;
    [Header("FightingScene‚Å‚ÌImage")]
    [SerializeField] private Image _fightingFaceUpImage;
    [SerializeField] private Image _fightingStandingImage;
    [Header("CharacterSelect‚Å‚ÌPrefab")]
    [SerializeField] private GameObject _windowFace;


    public string CharacterNameE { get { return _characterNameE; } }
    public string CharacterNameJ { get { return _characterNameJ; } }
    public CharacterActions CharacterPrefab { get { return _characterPrefab; } }
    public Image VersusEyeForcusImage { get {  return _versusEyeForcusImage; } }
    public Image VersusStandingImage { get { return _versusStandingImage; } }
    public Image VersusGrayImage { get { return _versusGrayImage; } }
    public Image FightingFaceUpImage { get { return _fightingFaceUpImage; } }
    public Image FightingStandingImage { get { return _fightingStandingImage; } }
    public GameObject WindowFace {get {return _windowFace;}}
}
