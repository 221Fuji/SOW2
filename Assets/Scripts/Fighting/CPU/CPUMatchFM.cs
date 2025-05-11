using System;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.InputSystem;

public class CPUMatchFM : FightingManager
{
    private CPUCharacter _cpu1P;
    private CPUCharacter _cpu2P;

    public bool IsLearningMode { get; private set; }

    public void SetLearningMode(bool value)
    {
        IsLearningMode = value;
    }

    public void InitializeCPUMatch(CPUCharacter chara1, CPUCharacter chara2)
    {
        _cpu1P = chara1;
        _cpu2P = chara2;

        GameObject prefab1P = InstantiateCharacterPrefab(chara1);
        GameObject prefab2P = InstantiateCharacterPrefab(chara2);

        //���E���h�P�J�n
        RoundData firstRound = new RoundData(2, 2, 1);
        PlayerData playerData1P = new PlayerData(
            chara1.CharacterData,
            prefab1P.GetComponent<CharacterActions>(),
            prefab1P.GetComponent<CharacterState>(),
            1
            );
        PlayerData playerData2P = new PlayerData(
            chara2.CharacterData,
            prefab2P.GetComponent<CharacterActions>(),
            prefab2P.GetComponent<CharacterState>(),
            2
            );
        StartRound(firstRound, playerData1P, playerData2P);
    }

    private GameObject InstantiateCharacterPrefab(CPUCharacter chara)
    {
        GameObject prefab;

        //���x�����ƂɃC���X�^���X����v���n�u��ς���
        //�C���ǉ��\��
        switch(chara.Level)
        {
            case CPUCharacter.CPULevel.Player:
                PlayerInput player1 = PlayerInput.Instantiate(
                    prefab: chara.CharacterData.CharacterPrefab.gameObject,
                    playerIndex: 1,
                    pairWithDevice: GameManager.Player1Device
                );
                prefab = player1.gameObject;
                break;
            case CPUCharacter.CPULevel.Easy:
                prefab = Instantiate(chara.CharacterData.CPUEasy.gameObject);
                break;
            default:
                prefab = Instantiate(chara.CharacterData.CharacterPrefab.gameObject);
                break;
        }

        return prefab;
    }

    public void StartLearnig(GameObject prefab1P, CharacterData cd1P, GameObject prefab2P, CharacterData cd2P)
    {
        //���E���h�P�J�n
        
        RoundData firstRound = new RoundData(2, 2, 1);
        PlayerData playerData1P = new PlayerData(
            cd1P,
            prefab1P.GetComponent<CharacterActions>(),
            prefab1P.GetComponent<CharacterState>(),
            1
            );
        PlayerData playerData2P = new PlayerData(
            cd2P,
            prefab2P.GetComponent<CharacterActions>(),
            prefab2P.GetComponent<CharacterState>(),
            2
            );
        StartRound(firstRound, playerData1P, playerData2P);
        
    }

    protected override void StartRound(RoundData roundData, PlayerData playerData1P, PlayerData playerData2P)
    {
        base.StartRound(roundData, playerData1P, playerData2P);
        
        if(_playerData1P.CharacterActions.TryGetComponent<CPUAgent>(out var agent1P))
        {
            agent1P.SetCAandCS();
        }
        if (_playerData2P.CharacterActions.TryGetComponent<CPUAgent>(out var agent2P))
        {
            agent2P.SetCAandCS();
        }
    }

    private void SetCPUData(CPUCharacter cpu1P, CPUCharacter cpu2P)
    {
        _cpu1P = cpu1P;
        _cpu2P = cpu2P;
    }

    protected async override void GoFighting()
    {
        CPUMatchFM cpuMatchManager =
            await GameManager.LoadAsync<CPUMatchFM>("FightingScene");
        cpuMatchManager.StartRound(CurrentRoundData, _playerData1P, _playerData2P);
        cpuMatchManager.SetCPUData(_cpu1P, _cpu2P);
    }

    protected async override void GameSet(int winnerNum)
    {
        if(!IsLearningMode)
        {
            //�����͏�������
            //CPUResultManager�ɍs���悤�ɂ���
            base.GameSet(winnerNum);
            return;
        }

        //�w�K�ĊJ
        RoundData firstRound = new RoundData(2, 2, 1);
        CPUMatchFM cpuMatchManager =
            await GameManager.LoadAsync<CPUMatchFM>("FightingScene");
        cpuMatchManager.StartRound(firstRound, _playerData1P, _playerData2P);
    }

    protected override async void GoResult(int winnerNum)
    {
        //ResultSelectScene�Ɉړ�
        var resultManager =
            await GameManager.LoadAsync<CPUMatchRM>("ResultScene");
        resultManager.InitializeRM(winnerNum, _playerData1P, _playerData2P);
        resultManager.SetCPUData(_cpu1P, _cpu2P);
    }
}

/// <summary>
/// CPUMatch�̃L�����N�^�[���
/// Player��CPU�����ʂł���
/// </summary>
public struct CPUCharacter
{
    public CharacterData CharacterData { get; private set; }
    public CPULevel Level { get; private set; }

    public CPUCharacter(CharacterData charaData, CPULevel level)
    {
        CharacterData = charaData;
        Level = level;
    }

    public enum CPULevel
    {
        Player,
        Easy,
        Normal,
        Hard,
        VeryHard
    }
}
