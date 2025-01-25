using System;
using System.Collections.Generic;

namespace CliffLeeCL
{
    public class ScoreManager : SingletonMono<ScoreManager>
    {
        public enum Team
        {
            Red,
            Blue
        }

        private int[] scoreList = new int[Enum.GetValues(typeof(Team)).Length]; 
        
        public void AddScore(Team team, int amount)
        {
            scoreList[(int)team] += amount;
            EventManager.Instance.OnTeamScored(this, team);
        }
        
        public int GetScore(Team team)
        {
            return scoreList[(int)team];
        }

        private void ResetScore()
        {
            for (int i = 0; i < scoreList.Length; i++)
            {
                scoreList[i] = 0;
            }
        }
    }
}
