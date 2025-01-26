using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace  CliffLeeCL
{
    public class PreparePlayer : State<GameCore>
    {
        public override void OnStateEnter()
        {
            LoadGameScene()
                .ContinueWith(SpawnPlayers)
                .ContinueWith(() => stateContext.SwitchState("MatchProcess"))
                .Forget();
        }

        private async UniTask LoadGameScene()
        { 
            var op = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
            while (!op.isDone)
            {
                await UniTask.Yield();
            }
        }

        private void SpawnPlayers() 
        { 
            var playerPrefab = GameConfig.Instance.playerConfig.GetPlayerPrefab(PlayerConfig.PlayerType.Normal);
            
            Debug.LogError("PreparePlayer for player " + stateContext.NetworkRunner.LocalPlayer.AsIndex);
            var player = GameObject.Instantiate<BasePlayer>(playerPrefab);
            player.SetTeam(Team.Blue);
            var spawnRoot = player.TeamType == Team.Red ? FieldManager.Instance.redSpawnRoot : FieldManager.Instance.blueSpawnRoot;
            player.transform.position = spawnRoot.position + Vector3.up;
            //player.SetCamera(false);
            player.StartController();
            
            if (player is NormalPlayer)
            {
                (player as NormalPlayer).SetMain();
            }
            var inputCtrlType = stateContext.IsUseNetwork ? InputCtrlConfig.InputCtrlType.Net : InputCtrlConfig.InputCtrlType.Local;
            var inputCtrlPrefab = GameConfig.Instance.inputCtrlConfig.GetInputCtrlPrefab(inputCtrlType);
            var inputCtrl = GameObject.Instantiate<BaseInputController>(inputCtrlPrefab);
            if (inputCtrlPrefab is NetworkInputController networkInputController)
            {
                stateContext.NetworkRunner.AddCallbacks(networkInputController);
            }

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
