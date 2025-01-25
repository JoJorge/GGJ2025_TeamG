using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Multiplayer.Center.Common;
using Fusion;
using Fusion.Sockets;
using Cysharp.Threading.Tasks;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField]
    private int minPlayerCount = 4;

    [SerializeField]
    private NetworkRunner networkRunner;

    [SerializeField]
    private NetworkPrefabRef _playerPrefab;
    public readonly Dictionary<PlayerRef, BasePlayer> Players = new();

    private const string DEFAULT_LOBBY_NAME = "Bubble";
    private Team? teamInput;

    public async UniTask WaitForAllPlayers()
    {
        await UniTask.WaitWhile(() => networkRunner.ActivePlayers.Count() < minPlayerCount);
    }

    public void RegisterPlayer(PlayerRef playerRef, BasePlayer player)
    {
        Players[playerRef] = player;
    }


    public async UniTask ConnectToServer()
    {
        networkRunner.ProvideInput = true;

        // Start or join (depends on gamemode) a session with a specific name
        await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "TestRoom",
            SceneManager = networkRunner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    NetworkSceneInfo CreateSceneInfo()
    {
        var scene = SceneRef.FromIndex(SceneManager.GetSceneByName("Game").buildIndex);
        //var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene);
        }
        return sceneInfo;
    }


    public void OnConnectedToServer(NetworkRunner runner)
    {
        // runner.JoinSessionLobby(SessionLobby.ClientServer, DEFAULT_LOBBY_NAME);
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.LogError($"Player {player.PlayerId} joins the game");
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
    }


    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
