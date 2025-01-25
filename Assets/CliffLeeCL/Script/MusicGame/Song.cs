using System.Collections.Generic;
using UnityEngine;

public class Song{
    public List<Note> notes;
    public string songName;
    public float duration;
    public int beatsPerMinute;

    public Song()
    {
        notes = new List<Note>();
        songName = "NoName";
        duration = 0.0f;
        beatsPerMinute = 0;
    }

    // Copy constructor
    public Song(Song songToBeCopied)
    {
        for(int i = 0; i < songToBeCopied.notes.Count; i++)
            notes.Add(new Note(songToBeCopied.notes[i]));

        songName = songToBeCopied.songName;
        duration = songToBeCopied.duration;
        beatsPerMinute = songToBeCopied.beatsPerMinute;
    }

    public Song(string inSongName, int inBeatsPerMinute, bool isLoadFromJSON = true)
    {
        Song loadedSong = new Song();

        notes = new List<Note>();
        if (isLoadFromJSON)
        {
            loadedSong = JSONParser.LoadFromJSON<Song>(inSongName);
        }

        notes = loadedSong.notes;
        duration = loadedSong.duration;
        songName = inSongName;
        beatsPerMinute = inBeatsPerMinute;
    }

    public Song(string inSongName, float inDuration, int inBeatsPerMinute)
    {
        notes = new List<Note>();
        songName = inSongName;
        duration = inDuration;
        beatsPerMinute = inBeatsPerMinute;
    }
}
