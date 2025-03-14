using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading;
using Cysharp.Threading.Tasks;

public class VersusManager : MonoBehaviour
{
    [SerializeField] private Image _panel;

    [SerializeField] private TextMeshPro _charaName1P;
    [SerializeField] private TextMeshPro _charaName2P;

    [SerializeField] private GameObject _eyeForcus1P;
    [SerializeField] private GameObject _eyeForcus2P;

    [SerializeField] private Transform _allParent;

    private CancellationTokenSource _performanceCTS;

    public async void VersusPerformance(CharacterData chara1p, CharacterData chara2p)
    {
        _performanceCTS = new CancellationTokenSource();
        CancellationToken token = _performanceCTS.Token;

        //ñæì]âèú
        await _panel.DOFade(0, 0.25f);

        //à√ì]
        _panel.color = Color.black;
        await _panel.DOFade(1, 0.25f).ToUniTask(cancellationToken: token);
    }
}
