using CliffLeeCL;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class NetworkInputController : BaseInputController, INetworkRunnerCallbacks
{
    public int microphoneIndex = 0;
    public float loudnessScalar = 10;
    public float loudnessThreshold = 0.5f;
    private InputSystem_Actions inputActions;    
    private InputSystem_Actions InputActions
    {
        get
        {
            return inputActions ??= new InputSystem_Actions();
        }
    }

    private AudioDetector audioDetector = new();

    private void OnEnable()
    {
        Debug.LogError("NetworkInputController.OnEnable");
        InputActions.Enable();
        EventManager.Instance.onMatchStart += OnMatchStart;
    }

    private void OnDisable()
    {
        InputActions.Disable();
        EventManager.Instance.onMatchStart -= OnMatchStart;
    }

    private void OnMatchStart()
    {
        audioDetector.StartRecording(microphoneIndex);
    }

    #region 

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        InputActions.Enable();

        var data = new NetworkInputData();
        data.Movement = InputActions.Player.Move.ReadValue<Vector2>();
        data.Turn = InputActions.Player.Look.ReadValue<Vector2>();
        data.Buttons.Set(InputButtons.Jump, InputActions.Player.Jump.triggered);
        data.Buttons.Set(InputButtons.ShootAttack, InputActions.Player.Attack.triggered);

        var loudness = audioDetector.GetMicrophoneLoudness(microphoneIndex) * loudnessScalar;
        data.Buttons.Set(InputButtons.ShootBubble, loudness > loudnessThreshold);
        data.Loudness = loudness;
        Debug.LogError("Input loadness = " + data.Loudness);
        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }
    #endregion
}
