using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceIdentification : MonoBehaviour
{

    [SerializeField] private InputAction _inputAction = default;
    // PlayerInputがアタッチされているプレイヤーオブジェクト
    //デバッグ用
    [SerializeField] private PlayerInput _playerPrefab = default;
    // 最大参加人数
    [SerializeField] private int _maxPlayerCount;

    private InputDevice[] _joinedDevices = new InputDevice[2];
    private int _currentPlayerCount = 0;
    private GameObject[] _characters = new GameObject[2];

    private void Awake()
    {
        _inputAction.Enable();
        _inputAction.performed += OnJoin;
    }

    private void OnJoin(InputAction.CallbackContext context)
    {
        // Join要求元のデバイスが既に参加済みのとき、処理を終了
        foreach (var device in _joinedDevices)
        {
            if (context.control.device == device)
            {
                return;
            }
        }

        // PlayerInputを所持した仮想のプレイヤーをインスタンス化
        // ※Join要求元のデバイス情報を紐づけてインスタンスを生成する
        PlayerInput character = PlayerInput.Instantiate(
            prefab: _playerPrefab.gameObject,
            playerIndex: _currentPlayerCount,
            pairWithDevice: context.control.device
            );

        //characterの情報を保存
        _characters[_currentPlayerCount] = character.gameObject;

        // Joinしたデバイス情報を保存
        _joinedDevices[_currentPlayerCount] = context.control.device;

        _currentPlayerCount++;

        //デバッグ用
        if(_currentPlayerCount == _maxPlayerCount)
        {
            //敵設定
            _characters[0].GetComponent<CharacterActions>().SetEnemy(_characters[1]);
            _characters[1].GetComponent<CharacterActions>().SetEnemy(_characters[0]);
        }
    }
}
