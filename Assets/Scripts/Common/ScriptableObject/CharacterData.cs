using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    [Header("キャラ識別番号")]
    [SerializeField] private int _characterNum;
    [Header("英語名")]
    [SerializeField] private string _characterNameE;
    [Header("日本名")]
    [SerializeField] private string _characterNameJ;
    [Header("キャラのPrefab")]
    [SerializeField] private CharacterActions _characterPrefab;
    [SerializeField] private CPUAgent _cpuEasy;
    [Header("VersusSceneでのImage")]
    [SerializeField] private Image _versusEyeForcusImage;
    [SerializeField] private Image _versusStandingImage;
    [SerializeField] private Image _versusGrayImage;
    [Header("FightingSceneでのImage")]
    [SerializeField] private Image _fightingFaceUpImage;
    [SerializeField] private Image _fightingStandingImage;
    [Header("CharacterSelectでのPrefab")]
    [SerializeField] private GameObject _windowFace;
    [SerializeField] private GameObject _csFigure;
    [SerializeField] private GameObject _csDot;
    [Header("ResultSceneでのImage")]
    [SerializeField] private Image _resultWhiteStandImage;
    [SerializeField] private Image _resultStandImage;
    [Header("ログ取る用の個体値")]
    [SerializeField] private int _myNumber;
    [Header("コマンドリスト用の要素リスト(1つの技につき1つ,0から表示)")]
    [SerializeField] private List<CmdListBox> _cmdListBoxes;


    public int CharacterNum { get { return _characterNum; } }
    public string CharacterNameE { get { return _characterNameE; } }
    public string CharacterNameJ { get { return _characterNameJ; } }
    public CharacterActions CharacterPrefab { get { return _characterPrefab; } }
    public CPUAgent CPUEasy { get { return _cpuEasy; } } 
    public Image VersusEyeForcusImage { get {  return _versusEyeForcusImage; } }
    public Image VersusStandingImage { get { return _versusStandingImage; } }
    public Image VersusGrayImage { get { return _versusGrayImage; } }
    public Image FightingFaceUpImage { get { return _fightingFaceUpImage; } }
    public Image FightingStandingImage { get { return _fightingStandingImage; } }
    public GameObject WindowFace {get {return _windowFace;}}
    public GameObject CSFigure {get {return _csFigure;}}
    public GameObject CSDot{get {return _csDot;}}
    public Image ResultWhiteStandImage { get { return _resultWhiteStandImage; } }
    public Image ResultStandImage { get { return _resultStandImage; } }
    public int MyNumber { get { return _myNumber; } }
    public List<CmdListBox> CmdListBoxes { get { return _cmdListBoxes; } }
}
