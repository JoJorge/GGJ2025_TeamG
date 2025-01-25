using UnityEngine;

namespace  CliffLeeCL
{
    public class PreparePlayer : State<GameCore>
    {
        public override void OnStateEnter()
        {
            var playerPrefab = GameConfig.Instance.playerConfig.GetPlayerPrefab(PlayerConfig.PlayerType.Normal);
            var player = GameObject.Instantiate<BasePlayer>(playerPrefab);
            player.SetCamera(false);
            var inputCtrlPrefab = GameConfig.Instance.inputCtrlConfig.GetInputCtrlPrefab(InputCtrlConfig.InputCtrlType.Local);
            var inputCtrl = GameObject.Instantiate<BaseInputController>(inputCtrlPrefab);
            inputCtrl.SetPlayer(player);
        }

        public override void UpdateState()
        {
            
        }

        public override void OnStateExit()
        {
            
        }
    }
}
