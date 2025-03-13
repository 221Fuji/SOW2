using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    [Header("���O(�p�ꖼ)")]
    [SerializeField] private string _characterName;
    [Header("�L������Prefab")]
    [SerializeField] private CharacterActions _characterPrefab;

    public string CharacterName { get { return _characterName; } }
    public CharacterActions CharacterPrefab { get { return _characterPrefab; } }
}
