using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Policies;
using UnityEngine;

/// <summary>
/// CPUäwèKóp
/// </summary>
public class CPULearningManager : MonoBehaviour
{
    [SerializeField] private bool _learningMode;
    [SerializeField] private GameObject _prefab1P;
    [SerializeField] private CharacterData _characterData1P;
    [SerializeField] private GameObject _prefab2P;
    [SerializeField] private CharacterData _characterData2P;
    [SerializeField] private CPUMatchFM _cpuMatchManager;

    private static bool _startedLearning = false;

    private void Awake()
    {
        if (!_learningMode) return;

        Screen.SetResolution(1280, 720, false);
        _cpuMatchManager.SetLearningMode(_learningMode);

        if (!_startedLearning)
        {
            GameObject chara1P = Instantiate(_prefab1P);
            GameObject chara2P = Instantiate(_prefab2P);
            _startedLearning = true;

            chara1P.GetComponent<BehaviorParameters>().TeamId = 0;
            chara2P.GetComponent<BehaviorParameters>().TeamId = 1;

            _cpuMatchManager.StartLearnig(chara1P, _characterData1P, chara2P, _characterData2P);
        }
    }
}
