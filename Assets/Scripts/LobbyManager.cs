using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Cysharp.Threading.Tasks;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private NetworkRunner networkRunner;
    [SerializeField]
    private NetworkManager networkManager;
    [SerializeField]
    private LobbyUI lobbyUI;

    private void Awake()
    {
        StartGame(GameMode.AutoHostOrClient);
    }
    
    public async UniTask StartGame(GameMode mode)
    {
        networkRunner.ProvideInput = true;

        Debug.LogError("StartGame");

        // Start or join (depends on gamemode) a session with a specific name
        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = CreateSceneInfo(),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
        
        Debug.LogError("EndtGame");

        Debug.LogError(networkRunner.LocalPlayer);
        

        lobbyUI.Interactable = true;

        while (true)
        {
            var playerObject = networkRunner.GetPlayerObject(networkRunner.LocalPlayer);
            Debug.LogError($"Player object: {playerObject != null}");
            Debug.LogError($"Is Connected: {networkRunner.IsConnectedToServer}");
        ;
            await UniTask.Yield();
        }


        NetworkSceneInfo CreateSceneInfo()
        {
            var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            var sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid)
            {
                sceneInfo.AddSceneRef(scene);
            }
            return sceneInfo;
        }
    }
}
