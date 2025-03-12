using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterDataBase : ScriptableObject
{
    [SerializeField] private List<CharacterData> _characterDataList;

    public List<CharacterData> CharacterDataList { get { return _characterDataList; } }
}
