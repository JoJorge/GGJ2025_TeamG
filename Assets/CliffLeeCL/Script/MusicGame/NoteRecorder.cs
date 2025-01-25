using System;
using System.Collections.Generic;
using UnityEngine;
using CliffLeeCL;

public class NoteRecorder : MonoBehaviour {
    public bool canRecord = false;

    private Song song;
    private List<Note> recordedNotes = new List<Note>();
    private bool isRecording = false;


	// Use this for initialization
	void Start () {
        //EventManager.instance.onSongPlayed += OnSongPlayed;
        //EventManager.instance.onSongEnded += OnSongEnded;
    }

    void OnDisable()
    {
        //EventManager.instance.onSongPlayed -= OnSongPlayed;
        //EventManager.instance.onSongEnded -= OnSongEnded;
    }

    void OnSongPlayed(Song playedSong)
    {
        song = playedSong;
        isRecording = true;
    }

    void OnSongEnded()
    {
        song.notes = recordedNotes;
        QuantizeSong(song);
        CombineNotesForSong(song);
        JSONParser.SaveToJSON(song, song.songName + " " + DateTime.Now.ToString("MM-dd HH_mm"));
    }

    // Update is called once per frame
    void Update () {
        if (isRecording)
        {
            RecordRedRow();
            RecordGreenRow();
            RecordBlueRow();
        }
	}

    void QuantizeSong(Song song)
    {
        float beatPeriod = 60.0f / song.beatsPerMinute;

        for(int i = 0; i < song.notes.Count; i++)
        {
            int noteBeatIndex = (int)(song.notes[i].playTime / beatPeriod);
            float beatTimeBeforeNote = noteBeatIndex * beatPeriod;
            float beatTimeAfterNote = (noteBeatIndex + 1) * beatPeriod;
            float beatTimeMiddle = (beatTimeBeforeNote + beatTimeAfterNote) / 2.0f;
            float offsetBefore = Mathf.Abs(song.notes[i].playTime - beatTimeBeforeNote);
            float offsetAfter = Mathf.Abs(song.notes[i].playTime - beatTimeAfterNote);
            float offsetMiddle = Mathf.Abs(song.notes[i].playTime - beatTimeMiddle);

            if (offsetBefore < offsetMiddle && offsetBefore < offsetAfter)
                song.notes[i].playTime = beatTimeBeforeNote;

            if (offsetMiddle < offsetBefore && offsetMiddle < offsetAfter)
                song.notes[i].playTime = beatTimeMiddle;

            if (offsetAfter < offsetBefore && offsetAfter < offsetMiddle)
                song.notes[i].playTime = beatTimeAfterNote;

        }
    }

    void CombineNotesForSong(Song song)
    {
        List<bool> dirtyNotes = new List<bool>();
        List<Note> combinedNotes = new List<Note>();
        float beatPeriod = 60.0f / song.beatsPerMinute;

        for (int i = 0; i < song.notes.Count; i++)
            dirtyNotes.Add(false);

        for (int i = 0; i < song.notes.Count; i++)
        {
            if (dirtyNotes[i]) continue;

            if (song.notes[i].targetColor == Color.red)
            {
                List<Note> sameTrackNotes = new List<Note>();
                List<int> sameTrackNotesIndex = new List<int>();
                int noteBeatIndex = (int)(song.notes[i].playTime / beatPeriod);

                // Extract same track's nodes
                for (int j = i + 1; j < song.notes.Count; j++)
                {
                    if (dirtyNotes[j]) continue;

                    if ((song.notes[i].trackIndex == song.notes[j].trackIndex))
                    {
                        sameTrackNotes.Add(new Note(song.notes[j]));
                        sameTrackNotesIndex.Add(j);
                    }
                }

                // Combine if the target is green and equal to 1 interval or blue and equal to 2 interval
                for(int s = 0; s < sameTrackNotes.Count; s++)
                {
                    int nextNoteBeatIndex = (int)(sameTrackNotes[s].playTime / beatPeriod);

                    if(sameTrackNotes[s].targetColor == Color.green && (nextNoteBeatIndex - noteBeatIndex == 1))
                    {
                        combinedNotes.Add(new Note(Color.yellow, Color.black, song.notes[i].playTime, song.notes[i].trackIndex));
                        dirtyNotes[i] = true;
                        dirtyNotes[sameTrackNotesIndex[s]] = true;
                        break;
                    }

                    if (sameTrackNotes[s].targetColor == Color.blue && (nextNoteBeatIndex - noteBeatIndex == 2))
                    {
                        combinedNotes.Add(new Note(Color.magenta, Color.black, song.notes[i].playTime, song.notes[i].trackIndex));
                        dirtyNotes[i] = true;
                        dirtyNotes[sameTrackNotesIndex[s]] = true;
                        break;
                    }
                }

                // No color to combine.
                if (!dirtyNotes[i])
                {
                    combinedNotes.Add(new Note(song.notes[i]));
                    dirtyNotes[i] = true;
                }
            }
            else if (song.notes[i].targetColor == Color.green)
            {
                List<Note> sameTrackNotes = new List<Note>();
                List<int> sameTrackNotesIndex = new List<int>();
                int noteBeatIndex = (int)(song.notes[i].playTime / beatPeriod);

                // Extract same track's nodes
                for (int j = i + 1; j < song.notes.Count; j++)
                {
                    if (dirtyNotes[j]) continue;

                    if ((song.notes[i].trackIndex == song.notes[j].trackIndex))
                    {
                        sameTrackNotes.Add(new Note(song.notes[j]));
                        sameTrackNotesIndex.Add(j);
                    }
                }

                // Combine if the target is blue and equal to 1 interval
                for (int s = 0; s < sameTrackNotes.Count; s++)
                {
                    int nextNoteBeatIndex = (int)(sameTrackNotes[s].playTime / beatPeriod);

                    if (sameTrackNotes[s].targetColor == Color.blue && (nextNoteBeatIndex - noteBeatIndex == 1))
                    {
                        combinedNotes.Add(new Note(Color.cyan, Color.black, song.notes[i].playTime, song.notes[i].trackIndex));
                        dirtyNotes[i] = true;
                        dirtyNotes[sameTrackNotesIndex[s]] = true;
                        break;
                    }
                }

                // No color to combine.
                if (!dirtyNotes[i])
                {
                    combinedNotes.Add(new Note(song.notes[i]));
                    dirtyNotes[i] = true;
                }
            }
            else if (song.notes[i].targetColor == Color.blue)
            {
                // No color to combine.
                if (!dirtyNotes[i])
                {
                    combinedNotes.Add(new Note(song.notes[i]));
                    dirtyNotes[i] = true;
                }
            }
        }

        song.notes = combinedNotes;
    }

    void RecordRedRow()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            recordedNotes.Add(new Note(Color.red, Color.black, (float)(AudioSettings.dspTime - MusicPlayer.instance.lastPlayedDspTime), 0));

        if (Input.GetKeyDown(KeyCode.W))
            recordedNotes.Add(new Note(Color.red, Color.black, (float)(AudioSettings.dspTime - MusicPlayer.instance.lastPlayedDspTime), 1));

        if (Input.GetKeyDown(KeyCode.E))
            recordedNotes.Add(new Note(Color.red, Color.black, (float)(AudioSettings.dspTime - MusicPlayer.instance.lastPlayedDspTime), 2));
    }

    void RecordGreenRow()
    {
        if (Input.GetKeyDown(KeyCode.A))
            recordedNotes.Add(new Note(Color.green, Color.black, (float)(AudioSettings.dspTime - MusicPlayer.instance.lastPlayedDspTime), 0));

        if (Input.GetKeyDown(KeyCode.S))
            recordedNotes.Add(new Note(Color.green, Color.black, (float)(AudioSettings.dspTime - MusicPlayer.instance.lastPlayedDspTime), 1));

        if (Input.GetKeyDown(KeyCode.D))
            recordedNotes.Add(new Note(Color.green, Color.black, (float)(AudioSettings.dspTime - MusicPlayer.instance.lastPlayedDspTime), 2));
    }

    void RecordBlueRow()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            recordedNotes.Add(new Note(Color.blue, Color.black, (float)(AudioSettings.dspTime - MusicPlayer.instance.lastPlayedDspTime), 0));

        if (Input.GetKeyDown(KeyCode.X))
            recordedNotes.Add(new Note(Color.blue, Color.black, (float)(AudioSettings.dspTime - MusicPlayer.instance.lastPlayedDspTime), 1));

        if (Input.GetKeyDown(KeyCode.C))
            recordedNotes.Add(new Note(Color.blue, Color.black, (float)(AudioSettings.dspTime - MusicPlayer.instance.lastPlayedDspTime), 2));

    }
}
