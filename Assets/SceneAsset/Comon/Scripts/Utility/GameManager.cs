using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class GameManager
{
    /// <summary>
    /// �񓯊����[�h����
    /// </summary>
    /// <param name="sceneName">�V�[����</param>
    /// <param name="mode">�V�[�����[�h���[�h</param>
    /// <returns>���[�h��V�[���̃R���|�[�l���g</returns>
    public static async UniTask<TComponent> LoadAsync<TComponent>(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        where TComponent : Component
    {
        await SceneManager.LoadSceneAsync(sceneName, mode);

        Scene scene = SceneManager.GetSceneByName(sceneName);

        return GetFirstComponent<TComponent>(scene.GetRootGameObjects());
    }

    /// <summary>
    /// GameObject�z�񂩂�w��̃R���|�[�l���g����擾����
    /// </summary>
    /// <typeparam name="TComponent">�擾�ΏۃR���|�[�l���g</typeparam>
    /// <param name="gameObjects">GameObject�z��</param>
    /// <returns>�ΏۃR���|�[�l���g</returns>
    private static TComponent GetFirstComponent<TComponent>(GameObject[] gameObjects)
        where TComponent : Component
    {
        TComponent target = null;
        foreach (GameObject go in gameObjects)
        {
            target = go.GetComponent<TComponent>();
            if (target != null) break;
        }
        return target;
    }

    public static InputDevice Player1Device { get; set; }
    public static InputDevice Player2Device { get; set; }

    public static string GetControlSchemeFromDevice(PlayerInput playerInput, InputDevice device)
    {
        foreach (var scheme in playerInput.actions.controlSchemes)
        {
            if (scheme.SupportsDevice(device))
            {
                return scheme.name;
            }
        }
        return "Unknown";
    }
}
