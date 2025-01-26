using UnityEngine;
using UnityEngine.SceneManagement;

namespace CliffLeeCL
{
    public class MatchOver : State<GameCore>
    {        
        private ResultUI resultUI;
        private bool isFinished;

        public override void OnStateEnter()
        {
            isFinished = false;
            var winner = JudgeWinner();
            SceneManager.UnloadScene(SceneManager.GetSceneByName("Game"));
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            resultUI = GameObject.Instantiate(GameConfig.Instance.uiConfig.ResultUI);
            resultUI.SetWinner(winner);
            resultUI.OnConfirm += () => isFinished = true;
        }

        private Team JudgeWinner()
        {
            var blueScore = ScoreManager.Instance.GetScore(ScoreManager.Team.Blue);
            var redScore = ScoreManager.Instance.GetScore(ScoreManager.Team.Red);
            if (blueScore > redScore)
                return Team.Blue;
            else if (blueScore < redScore)
                return Team.Red;
            else
                return Team.None;
        }

        public override void UpdateState()
        {
            if (isFinished)
            {
                stateContext.SwitchState("MainMenu");
            }
        }

        public override void OnStateExit()
        {
            GameObject.Destroy(resultUI.gameObject);
        }
    }
}
