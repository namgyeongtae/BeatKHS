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

    private void ConvertFile(string kshPath)
    {
        string[] lines = File.ReadAllLines(kshPath);
        float bpm = 0;
        float currentTime = 0;
        List<NoteData> notes = new List<NoteData>();
        List<string> currentMeasureLines = new List<string>();

        // 메타데이터 파싱
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
                // 이전 마디가 있다면 처리
                if (currentMeasureLines.Count > 0)
                {
                    ProcessMeasure(currentMeasureLines, ref currentTime, bpm, notes);
                    currentMeasureLines.Clear();
                }
                continue;
            }

            // 마디 내용 수집
            if (!string.IsNullOrEmpty(line) && line.Contains("|"))
            {
                currentMeasureLines.Add(line);
            }
        }

        // 마지막 마디 처리
        if (currentMeasureLines.Count > 0)
        {
            ProcessMeasure(currentMeasureLines, ref currentTime, bpm, notes);
        }

        // JSON 생성 및 저장
        SaveToJson(kshPath, notes);
    }

    private void ProcessMeasure(List<string> measureLines, ref float currentTime, float bpm, List<NoteData> notes)
    {
        if (measureLines.Count == 0) return;

        float measureDuration = (60f / bpm) * 4f; // 한 마디의 전체 시간 (4박자)
        float stepDuration = measureDuration / measureLines.Count; // 현재 마디의 라인 개수로 나눔
        
        bool[] isInLongNote = new bool[4];
        float measureStartTime = currentTime; // 마디 시작 시간 저장

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
                if (noteType == '1') // ShortNote
                {
                    notes.Add(new NoteData
                    {
                        time = currentLineTime * 1000f,
                        type = "ShortNote",
                        duration = 0f,
                        lane = lane
                    });
                }
                else if (noteType == '2' && !isInLongNote[lane]) // LongNote 시작점
                {
                    // 현재 마디에서 남은 2의 개수 계산
                    float duration = FindLongNoteDuration(measureLines, i, lane, stepDuration);
                    notes.Add(new NoteData
                    {
                        time = currentLineTime * 1000f,
                        type = "LongNote",
                        duration = duration * 1000f,
                        lane = lane
                    });
                    isInLongNote[lane] = true;
                }
                else if (noteType != '2' && isInLongNote[lane])
                {
                    isInLongNote[lane] = false;
                }
            }
        }

        // 마디가 끝나면 전체 마디 시간만큼 증가
        currentTime += measureDuration;
    }

    private float FindLongNoteDuration(List<string> measureLines, int startIndex, int lane, float stepDuration)
    {
        float duration = 0f;
        for (int i = startIndex + 1; i < measureLines.Count; i++)
        {
            string line = measureLines[i];
            if (line[lane] == '2')
            {
                duration += stepDuration;
            }
            else
            {
                break;
            }
        }
        return duration;
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
