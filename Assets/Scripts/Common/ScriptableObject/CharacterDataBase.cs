using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterDataBase : ScriptableObject
{
    [SerializeField] private List<CharacterData> _characterDataList;
    [SerializeField] private List<CharacterData> _devedCpuList;

    public List<CharacterData> CharacterDataList { get { return _characterDataList; } }
    public List<CharacterData> DevedCpuList { get { return _devedCpuList; } }
    /// <summary>
    /// キャラクターの英語名からCharacterdataを取得する
    /// </summary>
    /// <param name="characterName">英語名</param>
    public CharacterData GetCharacterDataByName(string characterName)
    {
        CharacterData resultCharacter = null;

        foreach(CharacterData cd in _characterDataList)
        {
            if(cd.CharacterNameE == characterName)
            {
                resultCharacter = cd;
                break;
            }
        }

        if(resultCharacter == null)
        {
            Debug.LogError($"{characterName}という名前のキャラクターが存在しません");
        }

        return resultCharacter;
    }
}
