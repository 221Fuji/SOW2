using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading;
using Cysharp.Threading.Tasks;

public class VersusManager : MonoBehaviour
{
    [Header("��ꉉ�o")]
    [SerializeField] private Transform _firstParent;
    [SerializeField] private Image _panel;
    [SerializeField] private TextMeshProUGUI _charaName1P;
    [SerializeField] private TextMeshProUGUI _charaName2P;
    [SerializeField] private Transform _eyeForcusMask1P;
    [SerializeField] private Transform _eyeForcusMask2P;

    [Header("��j���o")]
    [SerializeField] private Transform _secondParent;
    [SerializeField] private Image _vsImage;
    [SerializeField] private TextMeshProUGUI _versusText;
    [SerializeField] private Transform _gray1P;
    [SerializeField] private Transform _gray2P;
    [SerializeField] private Transform _stand1P;
    [SerializeField] private Transform _stand2P;

    private CancellationTokenSource _performanceCTS;

    public async void VersusPerformance(CharacterData chara1p, CharacterData chara2p)
    {
        //��ꉉ�o�J�n
        _firstParent.gameObject.SetActive(true);
        _secondParent.gameObject.SetActive(false);

        //eyeForcusImage����
        Image eyeForcus1P = Instantiate(chara1p.VersusEyeForcusImage);
        eyeForcus1P.transform.SetParent(_eyeForcusMask1P, false);
        Image eyeForcus2P = Instantiate(chara2p.VersusEyeForcusImage);
        eyeForcus2P.transform.SetParent(_eyeForcusMask2P, false);
        eyeForcus2P.transform.localPosition *= new Vector2(-1, 1);
        eyeForcus2P.transform.localScale *= new Vector2(-1, 1);

        _performanceCTS = new CancellationTokenSource();
        CancellationToken token = _performanceCTS.Token;

        //���]����
        _panel.color = new Color(1, 1, 1, 1);
        _panel.DOFade(0, 1f).ToUniTask(cancellationToken: token).Forget();

        //�Y�[���A�E�g
        await ZoomOut(_firstParent, token);

        //�e�L�X�g�ړ�
        _charaName1P.text = chara1p.CharacterNameE;
        _charaName2P.text = chara2p.CharacterNameE;
        _charaName1P.transform.localPosition = new Vector2(960, 135);
        _charaName2P.transform.localPosition = new Vector2(-960, -135);
        _charaName1P.transform.DOLocalMoveX(400, 3).SetEase(Ease.OutExpo).ToUniTask(cancellationToken: token).Forget();
        await _charaName2P.transform.DOLocalMoveX(-400, 3).SetEase(Ease.OutExpo).ToUniTask(cancellationToken: token);
        _charaName1P.transform.DOLocalMoveX(-400, 0.25f).SetEase(Ease.InExpo).ToUniTask(cancellationToken: token).Forget();
        _charaName2P.transform.DOLocalMoveX(400, 0.25f).SetEase(Ease.InExpo).ToUniTask(cancellationToken: token).Forget();

        //�Y�[���C��
        await _firstParent.DOScale(new Vector2(8f, 8f), 0.25f).SetEase(Ease.InOutExpo).ToUniTask(cancellationToken: token);


        //��񉉏o�J�n
        _firstParent.gameObject.SetActive(false);
        _secondParent.gameObject.SetActive(true);
        _vsImage.transform.localScale = new Vector2(8, 8);
        _vsImage.color = new Color(1, 1, 1, 0);

        //�����G����
        Image stand1P = Instantiate(chara1p.VersusStandingImage);
        stand1P.transform.SetParent(_stand1P, false);
        Image stand2P = Instantiate(chara2p.VersusStandingImage);
        stand2P.transform.SetParent(_stand2P, false);

        //�����w�i����
        Image gray1P = Instantiate(chara1p.VersusGrayImage);
        gray1P.transform.SetParent(_gray1P, false);
        Image gray2P = Instantiate(chara2p.VersusGrayImage);
        gray2P.transform.SetParent(_gray2P, false);

        //�Y�[���A�E�g
        await ZoomOut(_secondParent, token);

        //VS���o
        _vsImage.DOFade(1, 0.25f).ToUniTask(cancellationToken: token).Forget();
        _vsImage.transform.DOScale(new Vector2(4, 4), 2f).SetEase(Ease.OutExpo).ToUniTask(cancellationToken: token).Forget();

        //versus���o
        await DOTween.To(
            () => _versusText.characterSpacing,  // ���݂̒l
            value => _versusText.characterSpacing = value, // �X�V����
            100, 2.5f).SetEase(Ease.InOutQuad).ToUniTask(cancellationToken: token);

        //�Y�[���C��
        _secondParent.DOScale(new Vector2(8f, 8f), 0.5f).SetEase(Ease.OutExpo).ToUniTask(cancellationToken: token).Forget();

        //�Ó]
        _panel.color = new Color(0, 0, 0, 0);
        await _panel.DOFade(1, 0.25f).ToUniTask(cancellationToken: token);

        //�V�[���ړ�
        var fm = await GameManager.LoadAsync<FightingManager>("FightingScene");
        fm.InitializeFM(GameManager.Player1Device, chara1p, GameManager.Player2Device, chara2p);
    }

    private async UniTask ZoomOut(Transform parent, CancellationToken token)
    {
        parent.localScale = new Vector2(2f, 2f);
        await parent.DOScale(new Vector2(1.05f, 1.05f), 0.25f).SetEase(Ease.Linear).ToUniTask(cancellationToken: token);
        parent.DOScale(new Vector2(1f, 1f), 2f).SetEase(Ease.Linear).ToUniTask(cancellationToken: token).Forget();
    }

    private void OnDestroy()
    {
        if(_performanceCTS == null) return;
        _performanceCTS.Cancel();
        _performanceCTS = null;
    }
}
