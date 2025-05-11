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
    /// �L�����N�^�[�̉p�ꖼ����Characterdata���擾����
    /// </summary>
    /// <param name="characterName">�p�ꖼ</param>
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
            Debug.LogError($"{characterName}�Ƃ������O�̃L�����N�^�[�����݂��܂���");
        }

        return resultCharacter;
    }
}
