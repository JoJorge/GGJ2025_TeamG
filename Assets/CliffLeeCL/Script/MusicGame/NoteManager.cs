using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CliffLeeCL;
public class NoteManager : MonoBehaviour {
    public GameObject note;
    private GameObject outerNote;
    private GameObject innerNote;
    public Transform startPos;//x+-2
    public float horizontalDist;
    public float verticalDist;
    public Transform redRegionCenter;
    public Vector2 noteVelocity;
    
    //private float current_Time;
    private Song song;
    private bool[] noteIsPlayed;
    private float[] noteSpawnTime;
    //offset is the time  from startPos to first red region
    double gameStartTime;
    
	// Use this for initialization
	void Start () {
        //EventManager.instance.onSongPlayed += OnSongPlayed;
        gameStartTime = AudioSettings.dspTime;
        //horizontalDist = 1.75f;
	}
	
	// Update is called once per frame
	void Update() {
        if (song!=null)
        {

            for (int i = 0; i < noteIsPlayed.Length;i++)
            {
                if (noteIsPlayed[i]) continue;
                else
                {
                    float diffTime = (float)(AudioSettings.dspTime+MusicPlayer.instance.songDelayTime - MusicPlayer.instance.lastPlayedDspTime) - noteSpawnTime[i];
                    
                    if (diffTime >= 0)
                    {
                        noteIsPlayed[i] = true;
                        SpawnNote(song.notes[i]);
                    }
                     
                }
                
            }
        }
        
	}
    public void SpawnNote(Note reference_note)
    {
        Vector3 spawn_Offset = new Vector3((reference_note.trackIndex - 1) * horizontalDist, 0, 0);
        GameObject currentNote = (GameObject)Instantiate(note, startPos.position + spawn_Offset, Quaternion.identity);
        //currentNote.GetComponent<NoteObject>().InitNote(reference_note,noteVelocity);
        
    }
    

    public void OnSongPlayed(Song playedSong)
    {

        song = playedSong;
        
        noteIsPlayed = new bool[song.notes.Count];
        noteSpawnTime = new float[song.notes.Count];
        noteVelocity = -new Vector2(0, verticalDist * song.beatsPerMinute / 60);
        for(int i=0;i<noteIsPlayed.Length;i++)
        {
            noteIsPlayed[i] = false;
            noteSpawnTime[i] = song.notes[i].playTime - get_Period_before_spawn(song.notes[i].targetColor)+MusicPlayer.instance.songDelayTime ;
            //noteSpawnTime[i] = song.notes[i].playTime - get_Period_before_spawn(song.notes[i].targetColor) ;
            /*
            Debug.Log("PlayTime:" + song.notes[i].playTime.ToString("0.00"));
            Debug.Log("period before spawn:" + get_Period_before_spawn(song.notes[i].targetColor).ToString("0.00"));
            Debug.Log("spawnTime:"+noteSpawnTime[i].ToString("0.00"));
            */
        }

    }
    void OnDisable(){
        //EventManager.instance.onSongPlayed -= OnSongPlayed;
     }

    //input note's first target color to get the spawn time period before play time.
    float get_Period_before_spawn(Color targetColor)
    {
        float period_Start_red = (startPos.position.y - redRegionCenter.position.y) / -noteVelocity.y;
        float period_between_judge = 60.0f / (float)song.beatsPerMinute;
        
        if (targetColor.r > 0)
        {
            return period_Start_red;
        }
        else if (targetColor.g > 0)
        {
            return period_Start_red + period_between_judge;
        }
        else if (targetColor.b > 0)
        {
            return period_Start_red + 2 * period_between_judge;
        }
        else
        {
            return 0;
        }
        
        
    }

    
}
