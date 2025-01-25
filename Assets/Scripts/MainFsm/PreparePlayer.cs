using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Fusion;
using UnityEngine;

namespace CliffLeeCL
{
    public class PreparePlayer : State<GameCore>
    {
        private NetworkManager networkManager;
        private NetworkRunner networkRunner;

        public override void OnStateEnter()
        {
            networkRunner = stateContext.NetworkRunner;
            networkManager = stateContext.NetworkManager;

            if (networkRunner.IsServer)
            {
                Debug.LogError("I am server: " + stateContext.NetworkRunner.LocalPlayer);
                InitServer();
            }
            else 
            {
                Debug.LogError("I am player: " + stateContext.NetworkRunner.LocalPlayer);
                InitClient();
            }
            EventManager.Instance.OnMatchStart();
        }

        public override void UpdateState()
        {
        }

        private async UniTask InitServer() 
        {             
            var playerPrefab = GameConfig.Instance.playerConfig.GetPlayerPrefab(PlayerConfig.PlayerType.Net);

            foreach (var playerRef in networkRunner.ActivePlayers)
            {
                Debug.LogError("Server spawn player " + playerRef.AsIndex);

                var player = networkRunner.Spawn
                (
                    playerPrefab,
                    FieldManager.Instance.spawnRoot.position + Vector3.up,
                    inputAuthority: playerRef
                );
                stateContext.NetworkManager.RegisterPlayer(playerRef, player);

                networkRunner.SetPlayerObject(playerRef, player.GetComponent<NetworkObject>());

                player.SetCamera(playerRef.Equals(networkRunner.LocalPlayer));
                if (playerRef.Equals(stateContext.NetworkRunner.LocalPlayer))
                {
                    player.StartController();
                    SpawnController(player);
                }
                
                player.SetTeam(Team.Blue);
                if (player is NormalPlayer normalPlayer)
                {
                    normalPlayer.SetMain();
                }
            }
        }

        private async UniTask InitClient()
        {
            Debug.LogError($"Wait for player object to spawn {networkRunner.ActivePlayers.Count()}");

            await UniTask.WaitUntil(() => networkRunner.ActivePlayers.All(player => networkRunner.GetPlayerObject(player) != null));

            foreach (var playerRef in stateContext.NetworkRunner.ActivePlayers)
            {
                Debug.LogError($"Client fetch player" + playerRef.AsIndex);
                var player = networkRunner.GetPlayerObject(playerRef)?.GetComponent<BasePlayer>();

                networkManager.RegisterPlayer(playerRef, player);
                player.SetCamera(playerRef.Equals(stateContext.NetworkRunner.LocalPlayer));
                player.GetComponent<NetworkObject>().AssignInputAuthority(playerRef);
                if (playerRef.Equals(stateContext.NetworkRunner.LocalPlayer))
                {
                    player.StartController();
                    SpawnController(player);
                }
            }
        }


        private void SpawnController(BasePlayer player)
        {

            var inputCtrlType = stateContext.IsUseNetwork ? InputCtrlConfig.InputCtrlType.Net : InputCtrlConfig.InputCtrlType.Local;
            var inputCtrlPrefab = GameConfig.Instance.inputCtrlConfig.GetInputCtrlPrefab(inputCtrlType);

            var inputCtrl = GameObject.Instantiate<BaseInputController>(inputCtrlPrefab);
            if (inputCtrlPrefab is NetworkInputController networkInputController)
            {
                stateContext.NetworkRunner.AddCallbacks(networkInputController);
            }

            inputCtrl.SetPlayer(player);
        }

        public override void OnStateExit()
        {
        }
    }
}
