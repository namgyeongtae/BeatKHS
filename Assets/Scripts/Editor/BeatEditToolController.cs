using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BeatEditToolController
{
    public enum NoteType
    {
        None,
        ShortNote,
        LongNote
    }
    public enum EditMode
    {
        Normal,
        Edit
    }

    public struct Note
    {
        public int lane;
        public float beat;
        public NoteType type;
        public float length;

        public Note(int lane, float beat, NoteType type, float length = 0)
        {
            this.lane = lane;
            this.beat = beat;
            this.type = type;
            this.length = length;
        }
    }

    // Properties
    public EditMode CurrentMode { get; set; } = EditMode.Normal;
    public NoteType SelectedNoteType { get; set; } = NoteType.None;
    public bool IsDraggingLongNote { get; set; }
    public Vector2 LongNoteStartPos { get; set; }
    public float DragStartBeat { get; set; } = -1;
    public List<Note> Notes { get; private set; } = new List<Note>();

    // Audio playback properties
    public bool IsPlaying { get; set; }
    public bool IsPaused { get; set; }
    public float CurrentPlayTime { get; set; }
    public double StartTime { get; set; }
    public double PauseTime { get; set; }

    public void AddNote(int lane, float beat, NoteType type, float length = 0)
    {
        Notes.Add(new Note(lane, beat, type, length));
    }

    public void RemoveNote(Note note)
    {
        Notes.Remove(note);
    }

    public void StartLongNoteDrag(Vector2 startPos, float beat)
    {
        IsDraggingLongNote = true;
        LongNoteStartPos = startPos;
        DragStartBeat = beat;
    }

    public void EndLongNoteDrag()
    {
        IsDraggingLongNote = false;
        DragStartBeat = -1;
    }

    public void ClearNotes()
    {
        Notes.Clear();
    }

    public void StartPlayback()
    {
        IsPlaying = true;
        IsPaused = false;
        CurrentPlayTime = 0f;
        StartTime = EditorApplication.timeSinceStartup;
    }

    public void PausePlayback()
    {
        if (!IsPlaying) return;
        IsPaused = true;
        PauseTime = EditorApplication.timeSinceStartup;
    }

    public void ResumePlayback()
    {
        if (!IsPlaying || !IsPaused) return;
        IsPaused = false;
        StartTime += EditorApplication.timeSinceStartup - PauseTime;
    }

    public void StopPlayback()
    {
        IsPlaying = false;
        IsPaused = false;
        CurrentPlayTime = 0f;
    }

    public void UpdatePlayTime()
    {
        if (IsPlaying && !IsPaused)
        {
            CurrentPlayTime = (float)(EditorApplication.timeSinceStartup - StartTime);
        }
    }

    public bool CanPlaceNoteAt(int lane, float beat, NoteType type, float length = 0)
    {
        foreach (var note in Notes)
        {
            // 같은 레인에 있는 노트들 체크
            if (note.lane == lane)
            {
                if (type == NoteType.ShortNote)
                {
                    // 숏노트 배치 시: 다른 노트와 정확히 같은 비트에 있는지 체크
                    if (Mathf.Approximately(note.beat, beat))
                        return false;
                    
                    // 롱노트 내부에 있는지 체크
                    if (note.type == NoteType.LongNote)
                    {
                        if (beat >= note.beat && beat <= note.beat + note.length)
                            return false;
                    }
                }
                else if (type == NoteType.LongNote)
                {
                    // 롱노트 배치 시: 다른 노트와의 겹침 체크
                    float endBeat = beat + length;
                    
                    // 숏노트와의 겹침 체크
                    if (note.type == NoteType.ShortNote)
                    {
                        if (note.beat >= beat && note.beat <= endBeat)
                            return false;
                    }
                    // 롱노트와의 겹침 체크
                    else if (note.type == NoteType.LongNote)
                    {
                        float noteEndBeat = note.beat + note.length;
                        if (!(endBeat < note.beat || beat > noteEndBeat))
                            return false;
                    }
                }
            }
        }
        return true;
    }
} 