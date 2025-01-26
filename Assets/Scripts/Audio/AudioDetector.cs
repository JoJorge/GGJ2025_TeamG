using System.Linq;
using UnityEngine;

public class AudioDetector 
{
    public int SampleWindow { get; set; } = 128;

    private AudioClip microphoneClip;
    
    public void StartRecording(int index)
    {
        if (index >= Microphone.devices.Length)
        {
            return;
        }
        
        var deviceName = Microphone.devices[index];
        if (!Microphone.IsRecording(deviceName))
        {
            microphoneClip = Microphone.Start(deviceName, true, 10, AudioSettings.outputSampleRate);
            Debug.Log($"[AudioDetector] Start recording {deviceName}");
        }
    }
    
    public void StopRecording(int index)
    {
        var deviceName = Microphone.devices[index];
        if (Microphone.IsRecording(deviceName))
        {
            Microphone.End(Microphone.devices[index]);
            Debug.Log($"[AudioDetector] Stop recording {deviceName}");
        }
    }
    
    public float GetMicrophoneLoudness(int index)
    {
        if (microphoneClip == null)
        {
            return 0;
        }
        return GetLoudness(Microphone.GetPosition(Microphone.devices[index]), microphoneClip);
    }

    public float GetLoudness(int clipPosition, AudioClip clip)
    {
        var startPosition = clipPosition - SampleWindow;
        var waveData = new float[SampleWindow];
        
        startPosition = Mathf.Clamp(startPosition, 0, clip.samples - SampleWindow);
        clip.GetData(waveData, startPosition);
        return waveData.Sum(Mathf.Abs) / SampleWindow;
    }
}
