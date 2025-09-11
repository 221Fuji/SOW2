using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;

public class ConversationManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image leftCharacterImage;
    [SerializeField] private Image rightCharacterImage;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Characters")]
    [SerializeField] private Sprite ryuNormal;
    [SerializeField] private Sprite ryuSerious;
    [SerializeField] private Sprite kenSmile;

    private ConversationData currentConversation;
    private int currentLineIndex = 0;
    private CancellationTokenSource cts;

    private void Start()
    {
        LoadConversation("StoryConversation/test");
        PlayConversationAsync().Forget();
    }

    private void OnDestroy()
    {
        cts?.Cancel();
    }

    public void LoadConversation(string resourcePath)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);
        currentConversation = JsonUtility.FromJson<ConversationData>(jsonFile.text);
        currentLineIndex = 0;
        cts = new CancellationTokenSource();
    }

    private async UniTask PlayConversationAsync()
    {
        while (currentLineIndex < currentConversation.Lines.Count)
        {
            await ShowLineAsync(currentConversation.Lines[currentLineIndex], cts.Token);
            currentLineIndex++;

            // プレイヤー入力待ち
            await UniTask.WaitUntil(
                () => Input.GetKeyDown(KeyCode.Space),
                cancellationToken: cts.Token
            );
        }

        EndConversation();
    }

    private async UniTask ShowLineAsync(LineData line, CancellationToken token)
    {
        speakerNameText.text = line.Speaker;

        // キャラの立ち絵切り替え（DOTweenでフェード）
        if (line.Side == "left")
        {
            leftCharacterImage.sprite = GetCharacterSprite(line);
            await leftCharacterImage.DOFade(1f, 0.3f).WithCancellation(token);
            await rightCharacterImage.DOFade(0.3f, 0.3f).WithCancellation(token);
        }
        else
        {
            rightCharacterImage.sprite = GetCharacterSprite(line);
            await rightCharacterImage.DOFade(1f, 0.3f).WithCancellation(token);
            await leftCharacterImage.DOFade(0.3f, 0.3f).WithCancellation(token);
        }

        // テキストをタイプライター風に表示
        dialogueText.text = "";
        foreach (char c in line.Text)
        {
            dialogueText.text += c;
            await UniTask.Delay(30, cancellationToken: token); // 30ms間隔
        }
    }

    private Sprite GetCharacterSprite(LineData line)
    {
        if (line.Speaker == "Ryu")
        {
            if (line.Expression == "serious") return ryuSerious;
            return ryuNormal;
        }
        if (line.Speaker == "Ken")
        {
            if (line.Expression == "smile") return kenSmile;
        }
        return null;
    }

    private void EndConversation()
    {
        Debug.Log("会話終了 → バトル開始へ");
        // ここでシーン遷移やバトル開始処理を呼ぶ
    }
}
