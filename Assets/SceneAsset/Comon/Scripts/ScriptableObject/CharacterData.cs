using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    [Header("–¼‘O(‰pŒê–¼)")]
    [SerializeField] private string _characterName;
    [Header("ƒLƒƒƒ‰‚ÌPrefab")]
    [SerializeField] private CharacterActions _characterPrefab;

    public string CharacterName { get { return _characterName; } }
    public CharacterActions CharacterPrefab { get { return _characterPrefab; } }
}
