using UnityEngine;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private InputAction _inputAction;
    private bool _join = false;

    [SerializeField] private bool _soloPlayerDebug;
    [SerializeField] private CharacterDataBase _characterDataBase;

    public static bool SoloPlayDebug { get; private set; } = false;

    private void Awake()
    {
        if(_soloPlayerDebug)
        {
            SoloPlayDebug = true;
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

        if(!_soloPlayerDebug)
        {
            // ModeSelectScene‚ÉˆÚ“®
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
