using System;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.InputSystem;

public class CPUMatchManager : FightingManager
{
    public bool IsLearningMode { get; private set; }

    public void SetLearningMode(bool value)
    {
        IsLearningMode = value;
    }

    public void InitializeCPUMatch(CPUCharacter chara1, CPUCharacter chara2)
    {
        GameObject prefab1P = InstantiateCharacterPrefab(chara1);
        GameObject prefab2P = InstantiateCharacterPrefab(chara2);

        //ラウンド１開始
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

        if (chara.Level == 0)
        {
            PlayerInput player1 = PlayerInput.Instantiate(
            prefab: chara.CharacterData.CharacterPrefab.gameObject,
            playerIndex: 1,
            pairWithDevice: GameManager.Player1Device
            );
            prefab = player1.gameObject;
        }
        else
        {
            prefab = Instantiate(chara.CharacterData.CharacterPrefab.gameObject);
            prefab.AddComponent<CPUAgent>();
            var behaviorParameters = prefab.GetComponent<BehaviorParameters>();
            behaviorParameters.BehaviorName = $"{chara.CharacterData.CharacterNameE}_{chara.Level}";

            if(IsLearningMode)
            {
                behaviorParameters.BehaviorType = BehaviorType.Default;
                behaviorParameters.Model = null;
            }
            else
            {
                behaviorParameters.BehaviorType = BehaviorType.InferenceOnly;

                //ここにモデルを読み込む処理を書く
            }

            if (prefab.TryGetComponent<PlayerInput>(out var pi))
            {
                Destroy(pi);
            }
        }

        return prefab;
    }

    public void StartLearnig(GameObject prefab1P, CharacterData cd1P, GameObject prefab2P, CharacterData cd2P)
    {
        //ラウンド１開始
        
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

    protected async override void GoFighting()
    {
        CPUMatchManager cpuMatchManager =
            await GameManager.LoadAsync<CPUMatchManager>("FightingScene");
        cpuMatchManager.StartRound(CurrentRoundData, _playerData1P, _playerData2P);
    }

    protected async override void GameSet(int winnerNum)
    {
        if(!IsLearningMode)
        {
            //ここは書き直す
            //CPUResultManagerに行くようにする
            base.GameSet(winnerNum);
            return;
        }

        /*
        //学習用の処理
        if(winnerNum == 1)
        {
            _playerData1P.CharacterActions.GetComponent<CPUAgent>().OnGameSet(true);
            _playerData2P.CharacterActions.GetComponent<CPUAgent>().OnGameSet(false);
        }
        else
        {
            _playerData1P.CharacterActions.GetComponent<CPUAgent>().OnGameSet(false);
            _playerData2P.CharacterActions.GetComponent<CPUAgent>().OnGameSet(true);
        }
        */

        //学習再開
        RoundData firstRound = new RoundData(2, 2, 1);
        CPUMatchManager cpuMatchManager =
            await GameManager.LoadAsync<CPUMatchManager>("FightingScene");
        cpuMatchManager.StartRound(firstRound, _playerData1P, _playerData2P);
    }

    /// <summary>
    /// CPUMatchのキャラクター情報
    /// PlayerかCPUか判別できる
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
}
