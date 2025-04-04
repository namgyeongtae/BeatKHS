using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using static BeatEditToolController;
using System.IO;

public class BeatEditTool : EditorWindow
{
    private Vector2 scrollPosition;
    private float bpm = 120f;
    private float beatSnap = 4f; // 4분의 1박자
    
    // 수정된 상수들
    private const int LANE_COUNT = 4;
    private const float LANE_WIDTH = 40f; // 각 레인의 너비
    private const float LANE_SPACING = 10f; // 레인 사이 간격
    private const float SIDE_MARGIN = 40f; // 좌우 여백
    private const int MEASURES_PER_COLUMN = 4; // 한 줄에 표시할 마디 수
    private const float COLUMN_SPACING = 40f; // 컬럼 사이 간격
    private const float TOP_MARGIN = 25f; // 상단 여백
    private const float BOTTOM_MARGIN = 25f; // 하단 여백
    private const float CONTROLS_HEIGHT = 40f; // 컨트롤 영역 높이
    private const int MAX_BEATS_PER_MEASURE = 16;
    private const int MIN_BEATS_PER_MEASURE = 4;
    private float beatsPerMeasure = 4f;
    private float previewBeat = 0f;
    private AudioClip musicClip; // 재생할 음악
    private AudioClip keySound; // 키음 사운드
    private AudioSource audioSource; // 키음 재생용 AudioSource

    private BeatEditToolController controller;
    private HashSet<float> playedNoteBeats = new HashSet<float>();

    [MenuItem("Tools/Beat Editor")]
    public static void ShowWindow()
    {
        GetWindow<BeatEditTool>("Beat Editor");
    }

    private void OnEnable()
    {
        controller = new BeatEditToolController();
        
        // AudioSource 생성 및 설정
        GameObject tempGO = new GameObject("EditorAudioSource");
        audioSource = tempGO.AddComponent<AudioSource>();
        tempGO.hideFlags = HideFlags.HideAndDontSave;
        audioSource.playOnAwake = false;
    }

    private void OnDisable()
    {
        // AudioSource 정리
        if (audioSource != null)
        {
            DestroyImmediate(audioSource.gameObject);
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(GUILayout.Height(CONTROLS_HEIGHT));
        
        // 첫 번째 줄: 기존 컨트롤
        DrawBasicControls();

        // 두 번째 줄: 재생 컨트롤
        DrawPlaybackControls();

        // 세 번째 줄: 편집 모드 컨트롤
        DrawEditModeControls();
        
        EditorGUILayout.EndVertical();

        // 스크롤 뷰
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawBeatGrid();
        EditorGUILayout.EndScrollView();

        // 롱노트 드래그 처리
        HandleLongNoteDrag();

        // 재생 시간 업데이트
        if (controller.IsPlaying && !controller.IsPaused)
        {
            controller.UpdatePlayTime();
            Repaint();
        }
    }

    private void DrawBasicControls()
    {
        EditorGUILayout.BeginHorizontal();
        bpm = EditorGUILayout.FloatField("BPM", bpm);
        beatSnap = EditorGUILayout.FloatField("Beat Snap", beatSnap);
        beatsPerMeasure = EditorGUILayout.FloatField("Beats Per Measure", Mathf.Clamp(beatsPerMeasure, MIN_BEATS_PER_MEASURE, MAX_BEATS_PER_MEASURE));
        musicClip = (AudioClip)EditorGUILayout.ObjectField("Music", musicClip, typeof(AudioClip), false);
        keySound = (AudioClip)EditorGUILayout.ObjectField("Key Sound", keySound, typeof(AudioClip), false);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPlaybackControls()
    {
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = musicClip != null;
        
        if (GUILayout.Button(controller.IsPlaying && !controller.IsPaused ? "❚❚" : "▶", GUILayout.Width(40)))
        {
            if (!controller.IsPlaying)
                StartPlayback();
            else if (controller.IsPaused)
                ResumePlayback();
            else
                PausePlayback();
        }

        if (GUILayout.Button("■", GUILayout.Width(40)))
        {
            StopPlayback();
        }

        EditorGUILayout.LabelField($"Time: {controller.CurrentPlayTime:F2}s", GUILayout.Width(100));

        // Beats Per Measure 조절 UI
        EditorGUILayout.LabelField("Note Division:", GUILayout.Width(80));
        string[] divisionOptions = new string[] { "1/4", "1/8", "1/16", "1/32" };
        int[] divisionValues = new int[] { 4, 8, 16, 32 };
        int currentDivisionIndex = System.Array.IndexOf(divisionValues, (int)beatsPerMeasure);
        int newDivisionIndex = EditorGUILayout.Popup(currentDivisionIndex, divisionOptions, GUILayout.Width(60));
        if (currentDivisionIndex != newDivisionIndex)
        {
            beatsPerMeasure = divisionValues[newDivisionIndex];
        }
        
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawEditModeControls()
    {
        EditorGUILayout.BeginHorizontal();
        
        // 모드 선택 버튼
        if (GUILayout.Toggle(controller.CurrentMode == BeatEditToolController.EditMode.Normal, "Normal", EditorStyles.miniButtonLeft))
            controller.CurrentMode = BeatEditToolController.EditMode.Normal;
        if (GUILayout.Toggle(controller.CurrentMode == BeatEditToolController.EditMode.Edit, "Edit", EditorStyles.miniButtonRight))
            controller.CurrentMode = BeatEditToolController.EditMode.Edit;

        GUILayout.Space(10);

        

        GUI.enabled = controller.CurrentMode == BeatEditToolController.EditMode.Edit;
        
        // 노트 타입 선택 버튼
        if (GUILayout.Toggle(controller.SelectedNoteType == NoteType.ShortNote, "Short Note", EditorStyles.miniButtonLeft))
            controller.SelectedNoteType = NoteType.ShortNote;
        if (GUILayout.Toggle(controller.SelectedNoteType == NoteType.LongNote, "Long Note", EditorStyles.miniButtonRight))
            controller.SelectedNoteType = NoteType.LongNote;

        GUILayout.Space(20);

        // Clear 버튼 추가
        if (GUILayout.Button("Clear", GUILayout.Width(60)))
        {
            if (EditorUtility.DisplayDialog("Clear Notes", 
                "Are you sure you want to clear all notes?", 
                "Yes", "No"))
            {
                controller.ClearNotes();
                Repaint();
            }
        }

        // Save 버튼 추가
        if (GUILayout.Button("Save", GUILayout.Width(60)))
        {
            SaveData();
        }

        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
    }

    private int CalculateTotalMeasures()
    {
        if (musicClip == null) return 1;

        // 음악의 총 길이(초)와 BPM을 이용해 마디 수 계산
        float secondsPerMeasure = (60f / bpm) * 4; // 한 마디당 시간 (4비트 기준)
        int totalMeasures = Mathf.CeilToInt(musicClip.length / secondsPerMeasure);
        return Mathf.Max(totalMeasures, 1); // 최소 1마디는 보장
    }

    private void DrawBeatGrid()
    {
        if (musicClip == null) return;

        float totalMeasures = CalculateTotalMeasures();
        
        float columnWidth = (LANE_WIDTH * LANE_COUNT) + (LANE_SPACING * (LANE_COUNT - 1)) + (SIDE_MARGIN * 2);
        float gridHeight = position.height - CONTROLS_HEIGHT;
        float availableHeight = gridHeight - (TOP_MARGIN + BOTTOM_MARGIN);
        float measureHeight = availableHeight / MEASURES_PER_COLUMN;
        int totalColumns = Mathf.CeilToInt(totalMeasures / MEASURES_PER_COLUMN);

        // 전체 영역 확보
        Rect totalRect = GUILayoutUtility.GetRect(
            (columnWidth + COLUMN_SPACING) * totalColumns, 
            gridHeight
        );

        // 컬럼별로 그리기
        for (int column = 0; column < totalColumns; column++)
        {
            float columnX = totalRect.x + (column * (columnWidth + COLUMN_SPACING));
            Rect columnRect = new Rect(columnX, totalRect.y, columnWidth, gridHeight);
            
            // 컬럼 배경
            EditorGUI.DrawRect(columnRect, new Color(0.1f, 0.1f, 0.1f, 1f));

            // 레인 그리기
            for (int lane = 0; lane < LANE_COUNT; lane++)
            {
                float laneX = columnRect.x + SIDE_MARGIN + (lane * (LANE_WIDTH + LANE_SPACING));
                Rect laneRect = new Rect(laneX, columnRect.y + TOP_MARGIN, LANE_WIDTH, gridHeight - (TOP_MARGIN + BOTTOM_MARGIN));
                
                // 레인 배경
                EditorGUI.DrawRect(laneRect, new Color(0.15f, 0.15f, 0.15f, 1f));
                DrawRectBorder(laneRect, new Color(0.3f, 0.3f, 0.3f, 1f));

                // 레인 번호
                GUI.Label(new Rect(laneX + (LANE_WIDTH/2) - 10, columnRect.y + 5, 20, 20), 
                    lane.ToString(), new GUIStyle(GUI.skin.label) { normal = { textColor = Color.white } });
            }

             // 마디와 비트 라인 그리기
            for (int measure = 0; measure < MEASURES_PER_COLUMN; measure++)
            {
                float measureY = columnRect.y + gridHeight - BOTTOM_MARGIN - ((measure + 1) * measureHeight);
                
                // 마디 라인 (굵은 선)
                Rect measureLineRect = new Rect(
                    columnRect.x + SIDE_MARGIN, 
                    measureY, 
                    (LANE_WIDTH * LANE_COUNT) + (LANE_SPACING * (LANE_COUNT - 1)), 
                    2f);
                EditorGUI.DrawRect(measureLineRect, new Color(1f, 1f, 1f, 0.8f));

                // 비트 라인 (얇은 선) - beatsPerMeasure에 따라 동적으로 변경
                for (int beat = 1; beat < beatsPerMeasure; beat++)
                {
                    float beatY = measureY + (measureHeight * beat / beatsPerMeasure);
                    float lineAlpha = beat % 4 == 0 ? 0.6f : 0.3f; // 4분음표 위치는 좀 더 진하게
                    Rect beatLineRect = new Rect(
                        columnRect.x + SIDE_MARGIN, 
                        beatY, 
                        (LANE_WIDTH * LANE_COUNT) + (LANE_SPACING * (LANE_COUNT - 1)), 
                        1f);
                    EditorGUI.DrawRect(beatLineRect, new Color(1f, 1f, 1f, lineAlpha));
                }
            }
        }

        // 재생 위치 선 그리기
        if (controller.IsPlaying || controller.IsPaused)
        {
            DrawPlaybackLine(gridHeight, columnWidth, measureHeight);
        }

        // 노트 그리기
        foreach (var note in controller.Notes)
        {
            DrawNote(note, gridHeight);
        }

        // 드래그 중인 롱노트 미리보기 그리기
        if (controller.IsDraggingLongNote && Event.current.type == EventType.Repaint)
        {
            float startBeat = controller.DragStartBeat;
            int column = -1;
            int lane = -1;
            float currentBeat = CalculateBeatAtPosition(Event.current.mousePosition, out column, out lane);
            
            // 비트 스냅 적용
            currentBeat = Mathf.Round(currentBeat * beatSnap) / beatSnap;
            
            float length = Mathf.Abs(currentBeat - startBeat);

            // 미리보기 노트 그리기 (반투명하게)
            var previewNote = new BeatEditToolController.Note(
                lane,
                Mathf.Min(startBeat, currentBeat),
                NoteType.LongNote,
                length
            );
            
            previewBeat = currentBeat;
            // 반투명한 색상으로 미리보기 그리기
            DrawNote(previewNote, gridHeight, true);
        }

        // Handle input
        if (!controller.IsPlaying && controller.CurrentMode == BeatEditToolController.EditMode.Edit && 
            Event.current.type == EventType.MouseDown)
        {
            if (Event.current.button == 0)
            {
                HandleNotePlacement(Event.current.mousePosition, gridHeight);
                Event.current.Use();
            }
            else if (Event.current.button == 1)
            {
                HandleNoteRemoval(Event.current.mousePosition);
                Event.current.Use();
            }
        }
    }

    private void DrawNote(BeatEditToolController.Note note, float gridHeight, bool isPreview = false)
    {
        float columnWidth = (LANE_WIDTH * LANE_COUNT) + (LANE_SPACING * (LANE_COUNT - 1)) + (SIDE_MARGIN * 2);
        float measureHeight = (gridHeight - (TOP_MARGIN + BOTTOM_MARGIN)) / MEASURES_PER_COLUMN;

        // 항상 4비트 기준으로 위치 계산
        float measurePosition = note.beat / 4f;  // 4비트로 고정
        int noteColumn = Mathf.FloorToInt(measurePosition / MEASURES_PER_COLUMN);
        float measureInColumn = measurePosition % MEASURES_PER_COLUMN;

        float noteX = (noteColumn * (columnWidth + COLUMN_SPACING)) + 
                     SIDE_MARGIN + 
                     (note.lane * (LANE_WIDTH + LANE_SPACING));
        
        // 노트의 시작점 Y 위치 계산
        float startY = gridHeight - BOTTOM_MARGIN - (measureInColumn * measureHeight);

        Color noteColor = note.type == NoteType.ShortNote ? 
            new Color(1f, 1f, 0f, isPreview ? 0.5f : 0.8f) :  // 숏노트: 노란색
            new Color(0f, 1f, 1f, isPreview ? 0.5f : 0.8f);   // 롱노트: 청록색

        if (note.type == NoteType.ShortNote)
        {
            Rect noteRect = new Rect(noteX, startY - 5, LANE_WIDTH, 10);
            EditorGUI.DrawRect(noteRect, noteColor);
            if (!isPreview) DrawRectBorder(noteRect, Color.white);
        }
        else
        {
            // 롱노트 길이를 4비트 기준으로 계산
            float noteLength = note.length * (measureHeight / 4f);  // 4비트로 고정
            
            // 롱노트의 시작점에서 길이만큼 위로 그리기
            Rect noteRect = new Rect(noteX, startY - noteLength, LANE_WIDTH, noteLength);
            EditorGUI.DrawRect(noteRect, noteColor);
            if (!isPreview) DrawRectBorder(noteRect, Color.white);

            // 롱노트 시작과 끝 부분 강조
            if (!isPreview)
            {
                EditorGUI.DrawRect(new Rect(noteX, startY - 5, LANE_WIDTH, 10), Color.white);
                EditorGUI.DrawRect(new Rect(noteX, startY - noteLength - 5, LANE_WIDTH, 10), Color.white);
            }
        }
    }

    private void HandleNotePlacement(Vector2 mousePos, float gridHeight)
    {
        if (controller.CurrentMode != BeatEditToolController.EditMode.Edit) return;

        Debug.Log($"[Click] mousePosition: {mousePos}, scrollPosition: {scrollPosition}");
        
        int column = -1;        
        int lane = -1;
        float absoluteBeat = CalculateBeatAtPosition(mousePos, out column, out lane);

        // 선택된 노트 타입에 따라 처리
        switch (controller.SelectedNoteType)
        {
            case NoteType.ShortNote:
                if (controller.CanPlaceNoteAt(lane, absoluteBeat, NoteType.ShortNote))
                {
                    controller.AddNote(lane, absoluteBeat, NoteType.ShortNote);
                }
                break;

            case NoteType.LongNote:
                if (controller.CanPlaceNoteAt(lane, absoluteBeat, NoteType.LongNote))
                {
                    controller.StartLongNoteDrag(mousePos, absoluteBeat);
                }
                break;
        }

        Repaint();
    }

    private void HandleNoteRemoval(Vector2 mousePos)
    {
        if (controller.CurrentMode != BeatEditToolController.EditMode.Edit) return;

        int column = -1;
        int lane = -1;

        // 비트 위치 계산 - CalculateBeatAtPosition 사용
        float clickedBeat = CalculateBeatAtPosition(mousePos, out column, out lane);

        // 클릭된 위치 근처의 노트 찾기
        const float CLICK_THRESHOLD = 0.25f; // 클릭 인식 범위 (비트 단위)
        
        // 제거할 노트를 찾기 위한 리스트
        var notesToRemove = new List<BeatEditToolController.Note>();

        foreach (var note in controller.Notes)
        {
            if (note.lane == lane)
            {
                if (note.type == NoteType.ShortNote)
                {
                    // 숏노트는 단순히 비트 위치 비교
                    if (Mathf.Abs(note.beat - clickedBeat) < CLICK_THRESHOLD)
                    {
                        notesToRemove.Add(note);
                        break;
                    }
                }
                else if (note.type == NoteType.LongNote)
                {
                    // 롱노트는 시작점부터 끝점까지의 범위 체크
                    float noteEndBeat = note.beat + note.length;
                    if (clickedBeat >= note.beat - CLICK_THRESHOLD && 
                        clickedBeat <= noteEndBeat + CLICK_THRESHOLD)
                    {
                        notesToRemove.Add(note);
                        break;
                    }
                }
            }
        }

        // 찾은 노트들 제거
        if (notesToRemove.Count > 0)
        {
            foreach (var note in notesToRemove)
            {
                controller.RemoveNote(note);
            }
            Repaint();
        }
    }

    private float GetTotalWidth()
    {
        if (musicClip == null) return 0;

        float totalSeconds = musicClip.length;
        float secondsPerBeat = 60f / bpm;
        float totalBeats = totalSeconds / secondsPerBeat;
        float totalMeasures = totalBeats / beatsPerMeasure;
        
        float columnWidth = (LANE_WIDTH * LANE_COUNT) + (LANE_SPACING * (LANE_COUNT - 1)) + (SIDE_MARGIN * 2);
        int totalColumns = Mathf.CeilToInt(totalMeasures / MEASURES_PER_COLUMN);
        return (columnWidth + COLUMN_SPACING) * totalColumns;
    }

    private void DrawRectBorder(Rect rect, Color color)
    {
        // 상단
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), color);
        // 하단
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 1, rect.width, 1), color);
        // 좌측
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), color);
        // 우측
        EditorGUI.DrawRect(new Rect(rect.x + rect.width - 1, rect.y, 1, rect.height), color);
    }

    private void DrawPlaybackLine(float gridHeight, float columnWidth, float measureHeight)
    {
        // 마디당 시간을 bpm 기준으로 계산 (항상 4비트 기준)
        float secondsPerMeasure = (60f / bpm) * 4;  // 4는 고정된 마디당 비트 수
        float currentMeasure = controller.CurrentPlayTime / secondsPerMeasure;
        
        int currentColumn = Mathf.FloorToInt(currentMeasure / MEASURES_PER_COLUMN);
        float measureInColumn = currentMeasure % MEASURES_PER_COLUMN;
        
        float playLineX = (currentColumn * (columnWidth + COLUMN_SPACING)) + SIDE_MARGIN;
        float playLineY = gridHeight - BOTTOM_MARGIN - (measureInColumn * measureHeight);
        
        Rect playLineRect = new Rect(
            playLineX,
            playLineY,
            (LANE_WIDTH * LANE_COUNT) + (LANE_SPACING * (LANE_COUNT - 1)),
            2f
        );
        EditorGUI.DrawRect(playLineRect, new Color(1f, 0f, 0f, 0.8f));

        // 현재 재생 위치에 노트가 있는지 확인하고 키음 재생
        if (controller.IsPlaying && !controller.IsPaused)
        {
            float currentBeat = (controller.CurrentPlayTime / (60f / bpm)) * (4f / 4f);  // 4/4박자 기준
            
            foreach (var note in controller.Notes)
            {
                // 이미 재생된 노트는 건너뛰기
                if (playedNoteBeats.Contains(note.beat)) continue;

                if (Mathf.Abs(note.beat - currentBeat) < 0.1f)
                {
                    PlayKeySound();
                    playedNoteBeats.Add(note.beat);
                    // TestCode
                    Debug.Log($"NoteBeat - CurrentBeat = {note.beat - currentBeat}");
                    break;
                }
            }
        }
    }

    private void HandleLongNoteDrag()
    {
        if (!controller.IsDraggingLongNote) return;

        switch (Event.current.type)
        {
            case EventType.MouseDrag:
                Debug.Log($"[Drag] mousePosition: {Event.current.mousePosition}, scrollPosition: {scrollPosition}");
                Repaint();
                break;

            case EventType.MouseUp:
                Debug.Log($"[DragEnd] mousePosition: {Event.current.mousePosition}, scrollPosition: {scrollPosition}");
                
                float currentBeat = previewBeat;
                float startBeat = controller.DragStartBeat;
                
                // 비트 스냅 적용
                currentBeat = Mathf.Round(currentBeat * beatSnap) / beatSnap;
                float length = Mathf.Abs(currentBeat - startBeat);
                
                if (length >= 1f / beatSnap)
                {
                    int lane = CalculateLaneAtPosition(Event.current.mousePosition);
                    Debug.Log($"[LaneCalc] Input pos: {Event.current.mousePosition}, " +
                        $"With scroll: {Event.current.mousePosition.x + scrollPosition.x}, " +
                        $"Calculated Lane: {lane}");
                    
                    float minBeat = Mathf.Min(startBeat, currentBeat);
                    Debug.Log($"[BeatInfo] startBeat: {startBeat}, currentBeat: {currentBeat}, " +
                        $"length: {length}, minBeat: {minBeat}");
                    
                    if (controller.CanPlaceNoteAt(lane, minBeat, NoteType.LongNote, length))
                    {
                        controller.AddNote(lane, minBeat, NoteType.LongNote, length);
                        Debug.Log($"[NotePlaced] Lane: {lane}, Beat: {minBeat}, Length: {length}");
                    }
                }

                previewBeat = 0f;
                controller.EndLongNoteDrag();
                Repaint();
                Event.current.Use();
                break;
        }
    }

    private float CalculateBeatAtPosition(Vector2 mousePosition, out int column, out int lane)
    {
        float columnWidth = (LANE_WIDTH * LANE_COUNT) + (LANE_SPACING * (LANE_COUNT - 1)) + (SIDE_MARGIN * 2);
        float totalWidth = columnWidth + COLUMN_SPACING;
        
        // mousePosition은 이미 스크롤뷰 내에서의 상대 좌표이므로
        // 여기에 scrollPosition을 더하지 않고 바로 계산
        column = Mathf.FloorToInt(mousePosition.x / totalWidth);
        float columnX = mousePosition.x % totalWidth;
        
        // 레인 계산도 마찬가지로 수정
        lane = -1;
        if (columnX >= SIDE_MARGIN && columnX <= columnWidth - SIDE_MARGIN) {
            float laneWidth = LANE_WIDTH + LANE_SPACING;
            lane = Mathf.FloorToInt((columnX - SIDE_MARGIN) / laneWidth);
            if (lane >= LANE_COUNT) lane = -1;
        }
        
        // 3. 비트 위치 계산 (기존 코드)
        float measureHeight = (position.height - CONTROLS_HEIGHT - TOP_MARGIN - BOTTOM_MARGIN) / MEASURES_PER_COLUMN;
        float relativeY = mousePosition.y;
        float measureInColumn = ((position.height - CONTROLS_HEIGHT - relativeY - BOTTOM_MARGIN) / measureHeight);
        int currentMeasure = column * MEASURES_PER_COLUMN + Mathf.FloorToInt(measureInColumn);
        float beatInMeasure = (measureInColumn - Mathf.FloorToInt(measureInColumn)) * 4f;
        float absoluteBeat = currentMeasure * 4f + beatInMeasure;
        
        // 4. 최종 결과
        if (lane >= 0 && lane < LANE_COUNT) {
            return Mathf.Round(absoluteBeat * beatsPerMeasure / 4f) / (beatsPerMeasure / 4f);
        }
        
        return -1; // 유효하지 않은 위치
    }

    private int CalculateLaneAtPosition(Vector2 position)
    {
        float columnWidth = (LANE_WIDTH * LANE_COUNT) + (LANE_SPACING * (LANE_COUNT - 1)) + (SIDE_MARGIN * 2);
        float totalWidth = columnWidth + COLUMN_SPACING;
        
        float columnX = (position.x + scrollPosition.x) % (columnWidth + COLUMN_SPACING);
        Debug.Log($"[LaneCalc Detail] position.x: {position.x}, " +
            $"scrollPosition.x: {scrollPosition.x}, " +
            $"columnWidth: {columnWidth}, " +
            $"columnX: {columnX}");

        if (columnX < SIDE_MARGIN || columnX > columnWidth - SIDE_MARGIN) return -1;
        float laneWidth = LANE_WIDTH + LANE_SPACING;

        int lane = Mathf.FloorToInt((columnX - SIDE_MARGIN) / laneWidth);
        return lane;
    }

#region Play Clip
    private void PlayKeySound()
    {
        if (keySound != null && audioSource != null)
        {
            audioSource.clip = keySound;
            audioSource.Play();
        }
    }

    private void StartPlayback()
    {
        if (musicClip == null) return;
        
        playedNoteBeats.Clear();
        controller.IsPlaying = true;
        controller.IsPaused = false;
        controller.CurrentPlayTime = 0f;
        controller.StartTime = EditorApplication.timeSinceStartup;
        AudioUtility.PlayClip(musicClip);
    }

    private void PausePlayback()
    {
        if (!controller.IsPlaying) return;
        
        controller.IsPaused = true;
        controller.PauseTime = EditorApplication.timeSinceStartup;
        AudioUtility.StopAllClips();
    }

    private void ResumePlayback()
    {
        if (!controller.IsPlaying || !controller.IsPaused) return;
        
        controller.IsPaused = false;
        controller.StartTime += EditorApplication.timeSinceStartup - controller.PauseTime;
        AudioUtility.PlayClip(musicClip, controller.CurrentPlayTime);
    }

    private void StopPlayback()
    {
        controller.IsPlaying = false;
        controller.IsPaused = false;
        controller.CurrentPlayTime = 0f;
        playedNoteBeats.Clear();
        AudioUtility.StopAllClips();
        scrollPosition.x = 0;
        Repaint();
    }
#endregion

    public bool CanPlaceNoteAt(int lane, float beat, NoteType type, float length = 0)
    {
        foreach (var note in controller.Notes)
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

    public void SaveData()
    {
        if (controller.Notes.Count == 0) return;

        List<string> beatDataLines = new List<string>();
        float totalBeats = CalculateTotalMeasures() * 4; // 4비트 기준 전체 비트 수
        int beatsPerMeasure = 16; // 한 마디당 16비트

        // 마디별로 처리
        for (int measureIndex = 0; measureIndex < Mathf.CeilToInt(totalBeats / 4); measureIndex++)
        {
            beatDataLines.Add("--"); // 마디 시작

            // 마디 내의 각 비트 처리
            for (int beatIndex = 0; beatIndex < beatsPerMeasure; beatIndex++)
            {
                float currentBeat = (measureIndex * 4) + (beatIndex * (4f / beatsPerMeasure));
                char[] laneData = new char[LANE_COUNT];
                Array.Fill(laneData, '0'); // 기본값 '0'으로 초기화

                // 현재 비트에 있는 노트들 확인
                foreach (var note in controller.Notes)
                {
                    if (note.type == NoteType.ShortNote)
                    {
                        // 숏노트 처리
                        if (Mathf.Approximately(note.beat, currentBeat))
                        {
                            laneData[note.lane] = '1';
                        }
                    }
                    else if (note.type == NoteType.LongNote)
                    {
                        // 롱노트 처리
                        float noteEndBeat = note.beat + note.length;
                        if (currentBeat >= note.beat && currentBeat <= noteEndBeat)
                        {
                            laneData[note.lane] = '2';
                        }
                    }
                }

                // 비트 데이터를 문자열로 변환
                string beatData = new string(laneData);
                beatDataLines.Add(beatData);
            }
        }

        // 마지막 마디 구분선 추가
        beatDataLines.Add("--");

        // 파일로 저장
        string filePath = EditorUtility.SaveFilePanel(
            "Save Beat Data",
            "",
            "beatdata.txt",
            "txt");

        if (!string.IsNullOrEmpty(filePath))
        {
            File.WriteAllLines(filePath, beatDataLines);
            Debug.Log($"Beat data saved to: {filePath}");
        }
    }

    public void LoadData()
    {
        
    }
#region AudioUtility
    // AudioUtility 클래스 (Unity 2019.1 이상)
    private static class AudioUtility
    {
        private static Assembly assembly;
        private static Type audioUtilType;
        private static MethodInfo playClipMethod;
        private static MethodInfo stopAllClipsMethod;

        static AudioUtility()
        {
            assembly = Assembly.GetAssembly(typeof(AudioImporter));
            audioUtilType = assembly.GetType("UnityEditor.AudioUtil");
            playClipMethod = audioUtilType.GetMethod("PlayPreviewClip", new Type[] { typeof(AudioClip), typeof(Int32), typeof(Boolean) });
            stopAllClipsMethod = audioUtilType.GetMethod("StopAllPreviewClips");
        }

        public static void PlayClip(AudioClip clip, float startTime = 0f)
        {
            if (playClipMethod == null)
            {
                Debug.LogError("PlayPreviewClip method not found");
                return;
            }
            
            int startSample = (int)(startTime * clip.frequency);
            playClipMethod.Invoke(null, new object[] { clip, startSample, false });
        }

        public static void StopAllClips()
        {
            if (stopAllClipsMethod == null)
            {
                Debug.LogError("StopAllPreviewClips method not found");
                return;
            }
            
            stopAllClipsMethod.Invoke(null, null);
        }
    }
#endregion
}
