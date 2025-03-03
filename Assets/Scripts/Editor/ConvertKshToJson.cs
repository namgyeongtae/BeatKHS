using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class ConvertKshToJson : EditorWindow
{
    [MenuItem("Tools/Convert KSH to JSON")]
    public static void ShowWindow()
    {
        GetWindow<ConvertKshToJson>("KSH to JSON Converter");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Convert KSH File"))
        {
            string kshPath = EditorUtility.OpenFilePanel("Select KSH file", "", "ksh");
            if (!string.IsNullOrEmpty(kshPath))
            {
                ConvertFile(kshPath);
            }
        }
    }

    private class LongNoteInfo
    {
        public float startTime;
        public int lane;
        public int startMeasureIndex;
        public int startLineIndex;
    }

    private void ConvertFile(string kshPath)
    {
        string[] lines = File.ReadAllLines(kshPath);
        float bpm = 0;
        float currentTime = 0;
        List<NoteData> notes = new List<NoteData>();
        List<string> currentMeasureLines = new List<string>();
        List<List<string>> allMeasures = new List<List<string>>();
        Dictionary<int, LongNoteInfo> activeLongNotes = new Dictionary<int, LongNoteInfo>();

        // 메타데이터 파싱 및 마디 수집
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            
            if (line.StartsWith("t=")) // BPM
            {
                bpm = float.Parse(line.Substring(2));
                continue;
            }

            if (line == "--")
            {
                if (currentMeasureLines.Count > 0)
                {
                    allMeasures.Add(new List<string>(currentMeasureLines));
                    currentMeasureLines.Clear();
                }
                continue;
            }

            if (!string.IsNullOrEmpty(line) && line.Contains("|"))
            {
                currentMeasureLines.Add(line);
            }
        }

        if (currentMeasureLines.Count > 0)
        {
            allMeasures.Add(new List<string>(currentMeasureLines));
        }

        // 마디별 처리
        for (int measureIndex = 0; measureIndex < allMeasures.Count; measureIndex++)
        {
            ProcessMeasure(allMeasures[measureIndex], ref currentTime, bpm, notes, activeLongNotes, measureIndex, allMeasures);
        }

        SaveToJson(kshPath, notes);
    }

    private void ProcessMeasure(List<string> measureLines, ref float currentTime, float bpm, List<NoteData> notes, 
        Dictionary<int, LongNoteInfo> activeLongNotes, int currentMeasureIndex, List<List<string>> allMeasures)
    {
        if (measureLines.Count == 0) return;

        float measureDuration = (60f / bpm) * 4f;
        float stepDuration = measureDuration / measureLines.Count;
        float measureStartTime = currentTime;

        for (int i = 0; i < measureLines.Count; i++)
        {
            string line = measureLines[i];
            string[] parts = line.Split('|');
            if (parts.Length < 2) continue;

            float currentLineTime = measureStartTime + (stepDuration * i);
            string btNotes = parts[0];
            
            for (int lane = 0; lane < 4; lane++)
            {
                char noteType = btNotes[lane];
                int laneKey = lane;

                if (noteType == '1')
                {
                    notes.Add(new NoteData
                    {
                        time = currentLineTime * 1000f,
                        type = "ShortNote",
                        duration = 0f,
                        lane = lane
                    });
                }
                else if (noteType == '2')
                {
                    if (!activeLongNotes.ContainsKey(laneKey))
                    {
                        // 롱노트 시작
                        activeLongNotes[laneKey] = new LongNoteInfo
                        {
                            startTime = currentLineTime,
                            lane = lane,
                            startMeasureIndex = currentMeasureIndex,
                            startLineIndex = i
                        };
                    }
                }
                else if (noteType != '2' && activeLongNotes.ContainsKey(laneKey))
                {
                    // 롱노트 종료
                    var longNote = activeLongNotes[laneKey];
                    float duration = currentLineTime - longNote.startTime;
                    notes.Add(new NoteData
                    {
                        time = longNote.startTime * 1000f,
                        type = "LongNote",
                        duration = duration * 1000f,
                        lane = lane
                    });
                    activeLongNotes.Remove(laneKey);
                }
            }
        }

        // 마디의 마지막에서 아직 진행 중인 롱노트 확인
        if (currentMeasureIndex == allMeasures.Count - 1)
        {
            // 마지막 마디에서 끝나지 않은 롱노트들 강제 종료
            foreach (var longNote in activeLongNotes.Values)
            {
                float duration = (measureStartTime + measureDuration) - longNote.startTime;
                notes.Add(new NoteData
                {
                    time = longNote.startTime * 1000f,
                    type = "LongNote",
                    duration = duration * 1000f,
                    lane = longNote.lane
                });
            }
            activeLongNotes.Clear();
        }

        currentTime += measureDuration;
    }

    private void SaveToJson(string kshPath, List<NoteData> notes)
    {
        string jsonPath = Path.ChangeExtension(kshPath, "json");
        NoteDataContainer container = new NoteDataContainer { notes = notes };
        string json = JsonUtility.ToJson(container, true);
        File.WriteAllText(jsonPath, json);
        Debug.Log($"Converted JSON saved to: {jsonPath}");
    }
}
