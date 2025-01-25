using TMPro;
using UnityEngine;

namespace CliffLeeCL
{
    /// <summary>
    /// The class is used to show round time in text format.
    /// </summary>
    public class RoundTime : MonoBehaviour
    {
        /// <summary>
        /// The text to show round time.
        /// </summary>
        public TextMeshProUGUI roundTimeText;

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            roundTimeText.text = TimeToString(GameManager.Instance.roundTime - GameManager.Instance.roundTimer.CurrentTime);
        }

        /// <summary>
        /// Turn time to string in (minutes : seconds) format.
        /// </summary>
        /// <param name="time">Time to be parsed.</param>
        /// <returns>A string of parsed time in (minutes : seconds) format.</returns>
        string TimeToString(float time)
        {
            var timeString = $"{(int)time / 60:0}:{(int)time % 60:00}"; ;
            return timeString;
        }
    }
}
