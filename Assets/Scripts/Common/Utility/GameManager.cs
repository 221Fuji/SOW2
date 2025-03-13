using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class GameManager
{
    /// <summary>
    /// 非同期ロードする
    /// </summary>
    /// <param name="sceneName">シーン名</param>
    /// <param name="mode">シーンロードモード</param>
    /// <returns>ロード先シーンのコンポーネント</returns>
    public static async UniTask<TComponent> LoadAsync<TComponent>(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        where TComponent : Component
    {
        await SceneManager.LoadSceneAsync(sceneName, mode);

        Scene scene = SceneManager.GetSceneByName(sceneName);

        return GetFirstComponent<TComponent>(scene.GetRootGameObjects());
    }

    /// <summary>
    /// GameObject配列から指定のコンポーネントを一つ取得する
    /// </summary>
    /// <typeparam name="TComponent">取得対象コンポーネント</typeparam>
    /// <param name="gameObjects">GameObject配列</param>
    /// <returns>対象コンポーネント</returns>
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
