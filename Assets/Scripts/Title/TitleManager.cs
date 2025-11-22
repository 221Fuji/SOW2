using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private InputAction _inputAction;
    [SerializeField] private Animator _opening;
    private bool _join = false;

    [SerializeField] private bool _soloPlayerDebug;
    [SerializeField] private CharacterDataBase _characterDataBase;

    public static bool SoloPlayDebug { get; private set; } = false;

    private async void Awake()
    {
        if(_soloPlayerDebug)
        {
            SoloPlayDebug = true;
        }

        //即移動しないように
        try
        {
            await UniTask.DelayFrame(10);
        }
        catch
        {
            return;
        }

        Application.targetFrameRate = 60;
        GameManager.Player1Device = null;
        GameManager.Player2Device = null;
        _inputAction.Enable();
        _inputAction.performed += OnJoin;
    }

    private void OnDestroy()
    {
        _inputAction.performed -= OnJoin;
    }

    private async void OnJoin(InputAction.CallbackContext context)
    {
        if (_join) return;

        _join = true;
        GameManager.Player1Device = context.control.device;

        //オープニング演出
        SoundManager.I.SystemSEPlayer.PlaySE(0);
        _opening.SetTrigger("OpeningTrigger");
        await UniTask.WaitUntil(() =>
        {
            var startStateInfo = _opening.GetCurrentAnimatorStateInfo(0);
            return startStateInfo.IsName("Opening") && startStateInfo.normalizedTime >= 1f;
        });

        if (!_soloPlayerDebug)
        {
            // ModeSelectSceneに移動
            ModeSelectManager modeSelectmanager =
                await GameManager.LoadAsync<ModeSelectManager>("ModeSelectScene");

            modeSelectmanager.Initialize(GameManager.Player1Device);
        }
        else
        {
            VersusManager versusManager =
                await GameManager.LoadAsync<VersusManager>("VersusScene");

            versusManager.VersusPerformance(
                _characterDataBase.GetCharacterDataByName("Lancer"),
                _characterDataBase.GetCharacterDataByName("Cloud")
                );
        }

    }
}
