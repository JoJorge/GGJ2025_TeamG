using UnityEngine;
using System;

[Serializable]
public class Note{
    public Color targetColor;
    public Color currentColor;
    public float playTime;
    public int trackIndex;

    public Note()
    {
        targetColor = Color.white;
        currentColor = Color.black;
        playTime = 0.0f;
        trackIndex = 0;
    }

    // Copy contructor
    public Note(Note noteToBeCopied)
    {
        targetColor = noteToBeCopied.targetColor;
        currentColor = noteToBeCopied.currentColor;
        playTime = noteToBeCopied.playTime;
        trackIndex = noteToBeCopied.trackIndex;
    }

    public Note(Color inTargetColor, Color inCurrentColor, float inPlayTime, int inTrackIndex)
    {
        targetColor = inTargetColor;
        currentColor = inCurrentColor;
        playTime = inPlayTime;
        trackIndex = inTrackIndex;
    }
}
