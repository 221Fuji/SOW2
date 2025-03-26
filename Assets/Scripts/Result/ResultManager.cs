using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;

public class ResultManager : ModeManager
{
    [SerializeField] private RectTransform _topImage;
    [SerializeField] private RectTransform _bottomImage;
    [SerializeField] private TextMeshProUGUI _winnerText;
    private int _winnerNum;
    private PlayerData _player1Data;
    private PlayerData _player2Data;

    private void Awake()
    {
        WinPerformance();
    }

    public void InitializeRM(int winnerNum, PlayerData pd1, PlayerData pd2)
    {
        Initialize(GameManager.Player1Device);
        InstantiatePlayer2Input(GameManager.Player2Device);

        _winnerNum = winnerNum;
        _player1Data = pd1;
        _player2Data = pd2;

        WinPerformance();
    }

    private async void WinPerformance()
    {
        //白幕
        _topImage.localPosition = new Vector2(0, 135);
        _bottomImage.localPosition = new Vector2(0, -135);
        _topImage.DOLocalMoveY(350f, 0.75f).SetEase(Ease.OutExpo).ToUniTask().Forget();
        _bottomImage.DOLocalMoveY(-350f, 0.75f).SetEase(Ease.OutExpo).ToUniTask().Forget();

        //WinnerTextの文字間隔アニメーション
        DOTween.To(() => _winnerText.characterSpacing, x => _winnerText.characterSpacing = x, 30, 5)
        .SetEase(Ease.OutExpo).ToUniTask().Forget();

        _player1Data.CharacterData.


    }
}
