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
            Debug.LogError("PreparePlayer for player " + stateContext.NetworkRunner.LocalPlayer.AsIndex);



            Debug.LogError("PreparePlayer for player " + runner.LocalPlayer.AsIndex);

            if (runner.IsServer)
            {
                var player = runner.Spawn
                (
                    playerPrefab,
                    FieldManager.Instance.spawnRoot.position + Vector3.up
                );
                runner.SetPlayerObject(runner.LocalPlayer, player.GetComponent<NetworkObject>());
                // player.SetCamera(false);
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

        }

        public override void OnStateExit()
        {

        }
    }
}
