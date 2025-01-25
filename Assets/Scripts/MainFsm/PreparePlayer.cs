using Fusion;
using UnityEngine;

namespace CliffLeeCL
{
    public class PreparePlayer : State<GameCore>
    {
        public override void OnStateEnter()
        {
            var playerPrefab = GameConfig.Instance.playerConfig.GetPlayerPrefab(PlayerConfig.PlayerType.Net);
            var runner = stateContext.NetworkRunner;

            if (!runner.IsServer)
            {
                return;
            }

            foreach (var playerRef in runner.ActivePlayers)
            {
                Debug.LogError("Spawn player " + runner.LocalPlayer.AsIndex);

                var player = runner.Spawn
                (
                    playerPrefab,
                    FieldManager.Instance.spawnRoot.position + Vector3.up
                );
                stateContext.NetworkManager.RegisterPlayer(playerRef, player);

                runner.SetPlayerObject(runner.LocalPlayer, player.GetComponent<NetworkObject>());
                player.SetCamera(false);
                player.StartController();

                Debug.LogError($"Player {runner.LocalPlayer.PlayerId} joins the game");

                var inputCtrlType = stateContext.IsUseNetwork ? InputCtrlConfig.InputCtrlType.Net : InputCtrlConfig.InputCtrlType.Local;
                var inputCtrlPrefab = GameConfig.Instance.inputCtrlConfig.GetInputCtrlPrefab(inputCtrlType);

                var inputCtrl = GameObject.Instantiate<BaseInputController>(inputCtrlPrefab);
                if (inputCtrlPrefab is NetworkInputController networkInputController)
                {
                    runner.AddCallbacks(networkInputController);
                }

                inputCtrl.SetPlayer(player);

            }
        }

        public override void UpdateState()
        {
            if(stateContext.NetworkManager.Players.TryGetValue(stateContext.NetworkRunner.LocalPlayer, out var player))
                player.SetCamera(true);
        }

        public override void OnStateExit()
        {
        }
    }
}
