using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace  CliffLeeCL
{
    public class WaitPlayer : State<GameCore>
    {
        private bool isFinished;

        public override void OnStateEnter()
        {
            WaitUntilAllPlayerJoins();
        }

        public override void UpdateState()
        {
            if (isFinished)
            {
                stateContext.SwitchState("PreparePlayer");
            }
        }
        private async UniTask WaitUntilAllPlayerJoins()
        {
            var networkRunner = stateContext.NetworkRunner;
            await UniTask.WaitWhile(() => networkRunner.ActivePlayers.Count() < 2);

            isFinished = true;
        }


        public override void OnStateExit()
        {
        }
    }
}
