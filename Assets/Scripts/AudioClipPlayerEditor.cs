using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AudioClipPlayerEditor : EditorWindow
{
    private AudioClip audioClip; // 오디오 클립 저장
    private AudioSource audioSource; // 재생용 AudioSource
    private float currentTime; // 현재 시간(ms)
    private bool isPlaying = false; // 오디오 재생 여부
    private const int LaneCount = 4; // 레인 개수
    private NoteDataContainer noteDataContainer = new NoteDataContainer(); // 노트 데이터 컨테이너
    private Rect timelineRect; // 타임라인 그래프 영역
    private float timelineLength = 500f; // 타임라인 길이 (픽셀 단위)
    private float laneHeight = 100f; // 각 레인의 높이
    private float playbackSpeed = 1.0f; // 재생 배속

    private Vector2 scrollPosition = Vector2.zero;
    private Vector2 timelineScrollPosition = Vector2.zero;
    private float zoomLevel = 1.0f;
    private const float MIN_ZOOM = 1.0f;
    private const float MAX_ZOOM = 30.0f;
    private const float ZOOM_SENSITIVITY = 0.1f;

    private char[] InputLane = { 'D', 'F', 'J', 'K' };

    private enum EditMode
    {
        TimelineControl,
        NoteEdit
    }

    private EditMode currentEditMode = EditMode.TimelineControl;
    private NoteData selectedNote = null;

    private Dictionary<int, float> longNoteStartTimes = new Dictionary<int, float>();
    private Dictionary<int, bool> isLongNoteActive = new Dictionary<int, bool>();
    private const float LONG_NOTE_THRESHOLD = 200f; // 롱노트 판정 기준시간

    [MenuItem("Tools/Audio Clip Player")]
    public static void ShowWindow()
    {
        GetWindow<AudioClipPlayerEditor>("Audio Clip Player");
    }

    private void OnEnable()
    {
        if (audioSource == null)
        {
            GameObject audioObject = new GameObject("EditorAudioSource");
            audioSource = audioObject.AddComponent<AudioSource>();
            audioObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    private void OnDisable()
    {
        if (audioSource != null)
        {
            DestroyImmediate(audioSource.gameObject);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Audio Clip Player", EditorStyles.boldLabel);

        // 편집 모드 선택 드롭다운 추가
        currentEditMode = (EditMode)EditorGUILayout.EnumPopup("Edit Mode", currentEditMode);

        // 오디오 클립 드롭다운 슬롯
        audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", audioClip, typeof(AudioClip), false);

        DrawPlayControlButtons();
        DrawSaveAndLoadButton();
        // 재생 배속 설정
        DrawSpeedSelector();
        DrawPlayTime();
        if (isPlaying && audioSource != null)
        {
            currentTime = audioSource.time * 1000; // 초를 ms로 변환
            Repaint();
        }

        GUILayout.Space(10);

        DrawTimeline();
        DrawLaneSelector();
        DrawNoteDataView();

        InputLaneHandler();
    }

    private void DrawPlayTime()
    {
        GUILayout.Label($"Current Time: {currentTime}ms", EditorStyles.helpBox);
    }
    private void DrawPlayControlButtons()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Play"))
        {
            PlayAudioClip();
        }
        if (GUILayout.Button("Stop"))
        {
            StopAudioClip();
        }
        if (GUILayout.Button("Clear"))
        {
            ClearTimeline();
        }
        if (GUILayout.Button("Pause"))
        {
            PauseAudioClip();
        }
        GUILayout.EndHorizontal();
    }
    private void DrawSpeedSelector()
    {
        GUILayout.Label("Playback Speed", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("0.25x")) SetPlaybackSpeed(0.25f);
        if (GUILayout.Button("0.5x")) SetPlaybackSpeed(0.5f);
        if (GUILayout.Button("0.75x")) SetPlaybackSpeed(0.75f);
        if (GUILayout.Button("1.0x")) SetPlaybackSpeed(1.0f);
        if (GUILayout.Button("1.25x")) SetPlaybackSpeed(1.25f);
        if (GUILayout.Button("1.5x")) SetPlaybackSpeed(1.5f);
        if (GUILayout.Button("1.75x")) SetPlaybackSpeed(1.75f);
        if (GUILayout.Button("2.0x")) SetPlaybackSpeed(2.0f);
        GUILayout.EndHorizontal();

        GUILayout.Label($"Current Speed: {playbackSpeed}x", EditorStyles.helpBox);
    }
    private void DrawLaneSelector()
    {
        GUILayout.Label("Lanes", EditorStyles.boldLabel);
        for (int i = 0; i < LaneCount; i++)
        {
            if (GUILayout.Button($"Lane {i + 1} : {InputLane[i]}"))
            {
                return;
            }
        }
    }
    private void DrawNoteDataView()
    {
        GUILayout.Label("Notes", EditorStyles.boldLabel);
        
        // 스크롤뷰의 고정 높이 설정
        float scrollViewHeight =300f; // 원하는 높이로 조정 가능
        
        // 스크롤뷰를 위한 영역 확보
        Rect scrollViewRect = GUILayoutUtility.GetRect(0, scrollViewHeight, GUILayout.ExpandWidth(true));
        
        // 스크롤뷰 시작
        scrollPosition = GUI.BeginScrollView(
            scrollViewRect,
            scrollPosition,
            new Rect(0, 0, scrollViewRect.width - 20, noteDataContainer.notes.Count * 20f) // 20f는 각 노트 항목의 높이
        );

        // 노트 데이터 표시
        float yPosition = 0;
        foreach (var note in noteDataContainer.notes)
        {
            GUI.Label(new Rect(5, yPosition, scrollViewRect.width - 25, 20), 
                $"Time: {note.time}ms, Lane: {note.lane}, Type: {note.type}, Duration: {note.duration}");
            yPosition += 20;
        }

        GUI.EndScrollView();
    }
    private void DrawSaveAndLoadButton()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Notes"))
        {
            SaveNotesToJson();
        }
        if (GUILayout.Button("Load Notes"))
        {
            LoadNotesFromJson();
        }
        GUILayout.EndHorizontal();
    }
    private void DrawTimeline()
    {
        GUILayout.Space(10);
        GUILayout.Label("Timeline", EditorStyles.boldLabel);

        // 스크롤뷰를 위한 고정된 영역 확보
        Rect containerRect = GUILayoutUtility.GetRect(timelineLength, laneHeight * LaneCount);
        
        // 실제 타임라인의 크기 계산 (줌 적용)
        float zoomedWidth = timelineLength * zoomLevel;
        
        // 스크롤뷰 시작
        timelineScrollPosition = GUI.BeginScrollView(
            containerRect,
            timelineScrollPosition,
            new Rect(0, 0, zoomedWidth, containerRect.height)
        );

        // 타임라인 배경
        timelineRect = new Rect(0, 0, zoomedWidth, containerRect.height);
        EditorGUI.DrawRect(timelineRect, Color.gray);

        if (audioClip != null)
        {
            float totalDuration = audioClip.length * 1000;
            float progress = currentTime / totalDuration;

            // 편집 모드에 따라 다른 처리
            switch (currentEditMode)
            {
                case EditMode.TimelineControl:
                    HandleTimelineClick(totalDuration);
                    break;
                case EditMode.NoteEdit:
                    HandleNoteEditing(totalDuration);
                    break;
            }

            // 재생 위치 표시 (빨간 선)
            float redBarX = progress * zoomedWidth;
            EditorGUI.DrawRect(new Rect(redBarX, 0, 2, timelineRect.height), Color.red);

            // 레인 그리기
            for (int i = 0; i < LaneCount; i++)
            {
                float laneY = i * laneHeight;

                // 레인 구분선
                if (i > 0)
                {
                    EditorGUI.DrawRect(new Rect(0, laneY, zoomedWidth, 1), Color.black);
                }

                // 노트 표시
                foreach (var note in noteDataContainer.notes)
                {
                    if (note.lane == i)
                    {
                        float noteX = (note.time / totalDuration) * zoomedWidth;
                        Color noteColor = (note == selectedNote) ? Color.green : Color.yellow;
                        
                        if (note.type == "LongNote")
                        {
                            float noteDuration = (note.duration / totalDuration) * zoomedWidth;
                            EditorGUI.DrawRect(new Rect(noteX, laneY + 2, noteDuration, laneHeight - 4), new Color(1f, 0.5f, 0f, 0.5f));
                        }
                        else
                        {
                            EditorGUI.DrawRect(new Rect(noteX, laneY + 2, 4, laneHeight - 4), noteColor);
                        }
                    }
                }
            }

            // 현재 누르고 있는 키에 대한 임시 롱노트 표시
            foreach (var lane in longNoteStartTimes.Keys)
            {
                if (isLongNoteActive[lane])
                {
                    float noteX = (longNoteStartTimes[lane] / totalDuration) * zoomedWidth;
                    float currentDuration = ((currentTime - longNoteStartTimes[lane]) / totalDuration) * zoomedWidth;
                    EditorGUI.DrawRect(new Rect(noteX, lane * laneHeight + 2, currentDuration, laneHeight - 4), new Color(1f, 0.5f, 0f, 0.3f));
                }
            }

            HandleZoom();
        }

        GUI.EndScrollView();
    }

    private void HandleZoom()
    {
        Event e = Event.current;
        if (e.type == EventType.ScrollWheel && timelineRect.Contains(e.mousePosition + timelineScrollPosition))
        {
            // 마우스 포인터 위치의 상대적 비율 저장
            float mouseRatio = (e.mousePosition.x + timelineScrollPosition.x) / (timelineLength * zoomLevel);

            // 줌 레벨 업데이트
            zoomLevel = Mathf.Clamp(zoomLevel - e.delta.y * ZOOM_SENSITIVITY, MIN_ZOOM, MAX_ZOOM);

            // 마우스 포인터 위치 유지를 위한 스크롤 위치 조정
            float newX = (mouseRatio * timelineLength * zoomLevel) - e.mousePosition.x;
            timelineScrollPosition.x = Mathf.Max(0, newX);

            e.Use();
            Repaint();
        }
    }

    private void HandleTimelineClick(float totalDuration)
    {
        Event e = Event.current;
        Vector2 adjustedMousePos = e.mousePosition + timelineScrollPosition;
        
        if (e.type == EventType.MouseDown && timelineRect.Contains(adjustedMousePos))
        {
            float clickRatio = adjustedMousePos.x / (timelineLength * zoomLevel);
            currentTime = clickRatio * totalDuration;
            
            if (audioSource != null)
            {
                audioSource.time = currentTime / 1000f;
                if (!isPlaying)
                {
                    PlayAudioClip();
                }
            }
            
            e.Use();
            Repaint();
        }
    }

    private void HandleNoteEditing(float totalDuration)
    {
        Event e = Event.current;
        Vector2 mousePosition = e.mousePosition;

        if (e.type == EventType.MouseDown && timelineRect.Contains(mousePosition))
        {
            // 클릭한 위치 근처의 노트 찾기
            selectedNote = FindNoteNearPosition(mousePosition, totalDuration);
            if (selectedNote != null)
            {
                e.Use();
            }
        }
        else if (e.type == EventType.MouseDrag && selectedNote != null)
        {
            // 드래그로 노트 위치 수정
            float clickPosition = (mousePosition.x - timelineRect.x) / timelineRect.width;
            selectedNote.time = Mathf.Clamp(clickPosition * totalDuration, 0, totalDuration);
            
            // 드래그 중일 때 해당 위치의 음악 재생
            /* if (audioSource != null)
            {
                audioSource.time = selectedNote.time / 1000f;
                if (!isPlaying)
                {
                    audioSource.Play();
                    isPlaying = true;
                }
            } */
            
            e.Use();
            Repaint();
        }
        else if (e.type == EventType.MouseUp)
        {
            if (selectedNote != null)
            {
                // 음악 정지
                /* if (audioSource != null)
                {
                    audioSource.Stop();
                    isPlaying = false;
                } */
                selectedNote = null;
                e.Use();
            }
        }
    }

    private NoteData FindNoteNearPosition(Vector2 mousePosition, float totalDuration)
    {
        float clickTime = ((mousePosition.x - timelineRect.x) / timelineRect.width) * totalDuration;
        float closestDistance = 50f; // 클릭 허용 범위 (밀리초)
        NoteData closestNote = null;

        foreach (var note in noteDataContainer.notes)
        {
            float distance = Mathf.Abs(note.time - clickTime);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNote = note;
            }
        }

        return closestNote;
    }

    private void PlayAudioClip()
    {
        if (audioClip != null && audioSource != null)
        {
            audioSource.clip = audioClip;
            audioSource.pitch = playbackSpeed; // 재생 배속 적용
            audioSource.Play();
            isPlaying = true;
        }
        else
        {
            Debug.LogWarning("Audio Clip 또는 AudioSource가 설정되지 않았습니다.");
        }
    }

    private void StopAudioClip()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            isPlaying = false;
        }
    }

    private void PauseAudioClip()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
            isPlaying = false;
        }
    }

    private void ClearTimeline()
    {
        // 모든 노트 제거
        noteDataContainer.notes.Clear();

        // 음악 재생 초기화
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0;
            isPlaying = false;
        }

        // 타임라인 초기화
        currentTime = 0;
        Debug.Log("타임라인과 노트가 초기화되었습니다.");
    }

    private void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
        if (audioSource != null && isPlaying)
        {
            audioSource.pitch = playbackSpeed; // 실시간으로 재생 배속 변경
        }
        Debug.Log($"재생 속도가 {playbackSpeed}x로 설정되었습니다.");
    }

    private void AddNoteToLane(float time, int lane, string noteType = "ShortNote", float duration = 0)
    {
        noteDataContainer.notes.Add(new NoteData
        {
            time = time,
            lane = lane,
            duration = duration,
            type = noteType
        });
        Debug.Log($"노트 추가: Lane {lane + 1}, Time {currentTime}ms, Type {noteType}");
    }

    private void SaveNotesToJson()
    {
        string path = EditorUtility.SaveFilePanel("Save Notes", "", "NoteData.json", "json");
        if (!string.IsNullOrEmpty(path))
        {
            string json = JsonUtility.ToJson(noteDataContainer, true);
            File.WriteAllText(path, json);
            Debug.Log("노트 데이터가 저장되었습니다: " + path);
        }
    }

    private void LoadNotesFromJson()
    {
        string path = EditorUtility.OpenFilePanel("Load Notes", "", "json");
        if (!string.IsNullOrEmpty(path))
        {
            string json = File.ReadAllText(path);
            noteDataContainer = JsonUtility.FromJson<NoteDataContainer>(json);
            Debug.Log("노트 데이터가 로드되었습니다: " + path);
        }

        // 이 함수에서는 노트 데이터를 로드하고, 타임라인을 업데이트하는 로직을 구현해야 합니다.
        // 타임라인 업데이트는 현재 시간을 초기화하고, 노트 데이터를 타임라인에 표시하는 로직을 구현해야 합니다.
        // 이 부분은 현재 구현되어 있지 않으므로, 필요한 로직을 추가해야 합니다.
        List<NoteData> notes = noteDataContainer.notes;
        foreach (var note in notes)
        {
            AddNoteToLane(note.time, note.lane, note.type, note.duration);
        }
    }

    private void HandleLaneEvent(int laneIndex)
    {
        // 여기에 각 레인 버튼을 클릭했을 때 실행되는 로직을 구현
        // 예: 해당 레인의 오디오 재생 등
        if (laneIndex < LaneCount)  // LaneCount가 있다고 가정
        {
            // 레인 관련 로직 실행
            AddNoteToLane(currentTime, laneIndex);
            Repaint();  // GUI 업데이트
        }
    }

    private void InputLaneHandler()
    {
        if (!audioSource.isPlaying)
        {
            return;
        }

        Event e = Event.current;
        
        // 키를 누르고 있을 때의 처리
        if (e.type == EventType.KeyDown)
        {
            int? pressedLane = null;
            
            switch (e.keyCode)
            {
                case KeyCode.D:
                    pressedLane = 0;
                    break;
                case KeyCode.F:
                    pressedLane = 1;
                    break;
                case KeyCode.J:
                    pressedLane = 2;
                    break;
                case KeyCode.K:
                    pressedLane = 3;
                    break;
            }

            if (pressedLane.HasValue)
            {
                // 노트를 바로 추가하지 않고 시작 시간만 기록
                if (!isLongNoteActive.ContainsKey(pressedLane.Value))
                {
                    longNoteStartTimes[pressedLane.Value] = currentTime;
                    isLongNoteActive[pressedLane.Value] = false;  // 아직 롱노트로 변환되지 않음
                }
                e.Use();
            }
        }
        
        // 키를 누르고 있는 동안의 처리
        foreach (var lane in longNoteStartTimes.Keys.ToList())
        {
            float duration = currentTime - longNoteStartTimes[lane];
            if (duration >= LONG_NOTE_THRESHOLD && !isLongNoteActive[lane])
            {
                // 롱노트 판정 시간을 넘으면 롱노트 상태로 변경
                isLongNoteActive[lane] = true;
            }
            Repaint(); // 타임라인 실시간 업데이트를 위해 추가
        }
        
        // 키를 뗐을 때의 처리
        if (e.type == EventType.KeyUp)
        {
            int? releasedLane = null;
            
            switch (e.keyCode)
            {
                case KeyCode.D:
                    releasedLane = 0;
                    break;
                case KeyCode.F:
                    releasedLane = 1;
                    break;
                case KeyCode.J:
                    releasedLane = 2;
                    break;
                case KeyCode.K:
                    releasedLane = 3;
                    break;
            }

            if (releasedLane.HasValue && longNoteStartTimes.ContainsKey(releasedLane.Value))
            {
                float duration = currentTime - longNoteStartTimes[releasedLane.Value];
                
                if (duration < LONG_NOTE_THRESHOLD)
                {
                    // 짧게 눌렀을 경우 단노트 추가
                    AddNoteToLane(currentTime, releasedLane.Value, "ShortNote");
                }
                else
                {
                    // 롱노트인 경우 최종 duration과 함께 노트 데이터 추가
                    AddNoteToLane(longNoteStartTimes[releasedLane.Value], releasedLane.Value, "LongNote", duration);
                }
                
                // 상태 초기화
                isLongNoteActive.Remove(releasedLane.Value);
                longNoteStartTimes.Remove(releasedLane.Value);
                e.Use();
            }
        }
    }
}


