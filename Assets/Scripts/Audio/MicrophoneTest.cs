using UnityEngine;

public class MicrophoneTest : MonoBehaviour
{
    public int microphoneIndex = 0;
    public float loudnessScalar = 10;
    
    private AudioDetector audioDetector = new();
    
    void Start()
    {
        audioDetector.StartRecording(microphoneIndex);    
    }

    void Update()
    {
        var loudness = audioDetector.GetMicrophoneLoudness(microphoneIndex) * loudnessScalar;
        transform.localScale = new Vector3(loudness, loudness, loudness);
        Debug.Log($"{Microphone.devices[0]} loudness: {loudness}");
    }
}
