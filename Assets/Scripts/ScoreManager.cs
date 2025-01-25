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

        private List<int> scoreList = new(); 
        
        public void AddScore(Team team, int amount)
        {
            scoreList[(int)team] += amount;
        }
        
        public int GetScore(Team team)
        {
            return scoreList[(int)team];
        }

        private void Start()
        {
            foreach (var team in Enum.GetValues(typeof(Team)))
            {
                scoreList.Add(0); 
            }
            EventManager.Instance.onMatchStart += OnMatchStart;
        }

        private void OnDisable()
        {
            EventManager.Instance.onMatchStart -= OnMatchStart;
        }

        private void OnMatchStart()
        {
            for (int i = 0; i < scoreList.Count; i++)
            {
                scoreList[i] = 0;
            }
        }
    }
}
