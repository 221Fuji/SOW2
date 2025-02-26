using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private InputAction _inputAction;
    [SerializeField] private PlayerInput _playerPrefab = default;
    private bool _join = false;

    private void Awake()
    {
        Application.targetFrameRate = 60;
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

        PlayerInput character = PlayerInput.Instantiate(
            prefab: _playerPrefab.gameObject,
            playerIndex: 0,
            pairWithDevice: context.control.device
            );

        //ModeSelectScene‚ÉˆÚ“®
        ModeSelectManager modeSelectmanager =
            await GameManager.LoadAsync<ModeSelectManager>("ModeSelectScene");
        modeSelectmanager.InitializeMSM(character);
    }
}
