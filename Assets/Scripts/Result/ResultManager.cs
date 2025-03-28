using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultManager : ModeManager
{
    [Header("第一演出")]
    [SerializeField] private GameObject _firstPerformance;
    [SerializeField] private RectTransform _standImagePos;
    [SerializeField] private RectTransform _topImage;
    [SerializeField] private RectTransform _bottomImage;
    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private TextMeshProUGUI _playerNumText;
    [Header("第二演出")]
    [SerializeField] private GameObject _secondPerformance;
    [SerializeField] private TextMeshProUGUI _charaNameText1;
    [SerializeField] private TextMeshProUGUI _charaNameText2;
    [Header("第三演出")]
    [SerializeField] private GameObject _thirdPerformance;
    [SerializeField] private TextMeshProUGUI _finalCharaNameText;
    [SerializeField] private RectTransform _finalStandImagePos;

    private int _winnerNum;
    private PlayerData _winnerData;
    private CharacterData _winnerCharacterData;
    private CancellationTokenSource _resultPerformanceCTS;

    public bool IsCompletedPerformance
    {
        get { return _resultPerformanceCTS != null; }
    }

    //デバッグ用
    [SerializeField] CharacterData _debugCD;
    private void Awake()
    {
        _winnerCharacterData = _debugCD;
        WinPerformance();
    }

    public void InitializeRM(int winnerNum, PlayerData pd1, PlayerData pd2)
    {
        Initialize(GameManager.Player1Device);
        InstantiatePlayer2Input(GameManager.Player2Device);

        _winnerNum = winnerNum;

        _winnerData = winnerNum == 1 ? pd1 : pd2;

        WinPerformance();
    }

    private async void WinPerformance()
    {
        _resultPerformanceCTS = new CancellationTokenSource();
        CancellationToken token = _resultPerformanceCTS.Token;

        try
        {
            await FirstPerformance(token);
            await SecondPerformance(token);
            await ThirdPerformance(token);
        }
        finally
        {
            _resultPerformanceCTS = null;
        }
    }

    /// <summary>
    /// 第一演出
    /// </summary>
    private async UniTask FirstPerformance(CancellationToken token)
    {
        _firstPerformance.SetActive(true);
        _secondPerformance.SetActive(false);
        _thirdPerformance.SetActive(false);
        //WinnerTextの文字間隔アニメーション
        DOTween.To(() => _winnerText.characterSpacing, x => _winnerText.characterSpacing = x, 30, 5)
        .SetEase(Ease.OutExpo).ToUniTask(cancellationToken: token).Forget();
        _playerNumText.text = "Player" + _winnerData.PlayerNum.ToString();
        //立ち絵
        StandImagePerformance(token).Forget();
        //白幕
        await WhiteVail(token);
    }
    /// <summary>
    /// 第二演出
    /// </summary>
    private async UniTask SecondPerformance(CancellationToken token)
    {
        _firstPerformance.SetActive(false);
        _secondPerformance.SetActive(true);
        //名前切り替わり
        for (int i = 0; i < _winnerCharacterData.CharacterNameE.Length; i++)
        {
            Debug.Log(_winnerCharacterData.CharacterNameE[i].ToString());
            _charaNameText1.text = _winnerCharacterData.CharacterNameE[i].ToString(); // 1文字のみ表示
            _charaNameText2.text = _winnerCharacterData.CharacterNameE[i].ToString(); // 2文字のみ表示
            int interval = 500 / _winnerCharacterData.CharacterNameE.Length; // 次の文字へ (ループする)
            await UniTask.Delay(interval, cancellationToken: token); // 指定時間待機
        }
    }
    /// <summary>
    /// 第三演出
    /// </summary>
    private async UniTask ThirdPerformance(CancellationToken token)
    {
        _secondPerformance.SetActive(false);
        _thirdPerformance.SetActive(true);
        //立ち絵
        Image standImage = Instantiate(_winnerCharacterData.ResultStandImage);
        standImage.transform.SetParent(_finalStandImagePos, false);
        standImage.transform.localScale = new Vector2(5, 5);
        await standImage.transform.DOScale(new Vector2(1.25f, 1.25f), 0.5f)
            .SetEase(Ease.OutExpo).ToUniTask(cancellationToken: token);
    }


    private async UniTask WhiteVail(CancellationToken token)
    {
        _topImage.localPosition = new Vector2(0, 135);
        _bottomImage.localPosition = new Vector2(0, -135);
        _topImage.DOLocalMoveY(350f, 0.75f).SetEase(Ease.OutExpo).ToUniTask().Forget();
        await _bottomImage.DOLocalMoveY(-350f, 1f).SetEase(Ease.OutExpo).ToUniTask(cancellationToken: token);
    }

    private async UniTask StandImagePerformance(CancellationToken token)
    {
        RectTransform standImage = Instantiate(_winnerCharacterData.ResultWhiteStandImage).rectTransform;
        standImage.SetParent(_standImagePos, false);
        standImage.DOLocalMoveX(300, 2.5f).SetEase(Ease.OutExpo).ToUniTask().Forget();

        await UniTask.Delay(200, cancellationToken: token);

        RectTransform standImageChild = standImage.GetChild(0) as RectTransform;
        Image chidImage = standImageChild.GetComponent<Image>();

        for (int i = 0; i < 1; i++)
        {
            chidImage.color = new Color(1, 1, 1, 1);
            await UniTask.Delay(100, cancellationToken: token);
            chidImage.color = new Color(1, 1, 1, 0);
            await UniTask.Delay(100, cancellationToken: token);
        }
        chidImage.color = new Color(1, 1, 1, 1);
        standImageChild.SetParent(_standImagePos, true);
    }
}
