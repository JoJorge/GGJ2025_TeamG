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

            Debug.LogError("PreparePlayer for player " + runner.LocalPlayer.AsIndex);

            if (runner.IsServer)
            {
                var player = runner.Spawn
                (
                    playerPrefab
                );
                runner.SetPlayerObject(runner.LocalPlayer, player.GetComponent<NetworkObject>());
                player.SetCamera(false);
                Debug.LogError($"Player {runner.LocalPlayer.PlayerId} joins the game");

                var inputCtrlPrefab = GameConfig.Instance.inputCtrlConfig.GetInputCtrlPrefab(InputCtrlConfig.InputCtrlType.Net);

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
