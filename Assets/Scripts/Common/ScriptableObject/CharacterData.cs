using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    [Header("�p�ꖼ")]
    [SerializeField] private string _characterNameE;
    [Header("���{��")]
    [SerializeField] private string _characterNameJ;
    [Header("�L������Prefab")]
    [SerializeField] private CharacterActions _characterPrefab;
    [Header("VersusScene�ł�Image")]
    [SerializeField] private Image _versusEyeForcusImage;
    [SerializeField] private Image _versusStandingImage;
    [Header("FightingScene�ł�Image")]
    [SerializeField] private Image _fightingFaceUpImage;
    [SerializeField] private Image _fightingStandingImage;


    public string CharacterNameE { get { return _characterNameE; } }
    public string CharacterNameJ { get { return _characterNameJ; } }
    public CharacterActions CharacterPrefab { get { return _characterPrefab; } }
    public Image VersusEyeForcusImage { get {  return _versusEyeForcusImage; } }
    public Image VersusStandingImage { get { return _versusStandingImage; } }
    public Image FightingFaceUpImage { get { return _fightingFaceUpImage; } }
    public Image FightingStandingImage { get { return _fightingStandingImage; } }
}
