using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CliffLeeCL;

public class MusicPlayer : MonoBehaviour {
    /// <summary>
    /// The variable is used to access this class.
    /// </summary>
    public static MusicPlayer instance;
    public AudioSource audioSource;
    public List<Song> songs = new List<Song>();
    public double lastPlayedDspTime;
    public float songDelayTime;
    public bool isSongPlayed;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Song song = new Song("TheLightDemoVersion", 140, true);
        //float beatPeriod = 60.0f / song.beatsPerMinute;
        /*
        song.notes.Add(new Note(Color.red, Color.black, beatPeriod * 2.0f, 0));
        
        song.notes.Add(new Note(Color.red, Color.black, beatPeriod * 4.0f, 0));
        song.notes.Add(new Note(Color.green, Color.black, beatPeriod * 6.0f, 1));
        song.notes.Add(new Note(Color.green, Color.black, beatPeriod * 8.0f, 1));
        song.notes.Add(new Note(Color.blue, Color.black, beatPeriod * 10.0f, 2));
        song.notes.Add(new Note(Color.blue, Color.black, beatPeriod * 12.0f, 2));
        song.notes.Add(new Note(Color.yellow, Color.black, beatPeriod * 14.0f, 0));
        song.notes.Add(new Note(Color.yellow, Color.black, beatPeriod * 16.0f, 0));
        song.notes.Add(new Note(Color.magenta, Color.black, beatPeriod * 18.0f, 1));
        song.notes.Add(new Note(Color.magenta, Color.black, beatPeriod * 20.0f, 1));
        song.notes.Add(new Note(Color.cyan, Color.black, beatPeriod * 22.0f, 2));
        song.notes.Add(new Note(Color.cyan, Color.black, beatPeriod * 24.0f, 2));
        */
        songs.Add(song);

        //EventManager.instance.onGameStart += OnGameStart;
        //EventManager.instance.onGameRestart += OnGameRestart;
    }

    void OnDisable()
    {
        //EventManager.instance.onGameStart -= OnGameStart;
    }

    void OnGameStart()
    {
        lastPlayedDspTime = PlaySong(songs[0], songDelayTime);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastPlayedDspTime = PlaySong(songs[0], songDelayTime);
        }

        if (isSongPlayed && !audioSource.isPlaying)
        {
            isSongPlayed = false;
            //EventManager.instance.OnSongEnded();
        }
    }

    public double PlaySong(Song song, float delayTime)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + song.songName);

        song.duration = clip.length;
        audioSource.clip = clip;
        audioSource.PlayScheduled(AudioSettings.dspTime + delayTime);
        isSongPlayed = true;
        //EventManager.instance.OnSongPlayed(song);

        return AudioSettings.dspTime + delayTime;
    }

    public double StopSong()
    {
        audioSource.Stop();
        isSongPlayed = false;

        return AudioSettings.dspTime;
    }

    public void OnGameRestart()
    {
        lastPlayedDspTime = PlaySong(songs[0], songDelayTime);
    }
}
