using System;
using CliffLeeCL;
using TMPro;
using UnityEngine;

public class StartTime : MonoBehaviour
{
    /// <summary>
    /// The text to show round time.
    /// </summary>
    public TextMeshProUGUI timeText;

    private void OnEnable()
    {
        EventManager.Instance.onMatchStart += OnMatchStart;
    }

    private void OnDisable()
    {
        EventManager.Instance.onMatchStart -= OnMatchStart;
    }
    
    private void OnMatchStart()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        timeText.text = $"{GameCore.Instance.matchStartTime:N0}";
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        if (GameCore.Instance.matchStartTimer != null)
        {
            timeText.text = $"{GameCore.Instance.matchStartTime - GameCore.Instance.matchStartTimer.CurrentTime:N0}";
        }
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
