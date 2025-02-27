using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteGenerator : MonoBehaviour
{
    [SerializeField] private List<Transform> _keyLines;
    [SerializeField] private RectTransform _metronomeBar;
    [SerializeField] private GameObject _notePrefab;
    [SerializeField] private GameObject _longNotePrefab;
    [SerializeField] private float _judgementLineY; // 판정선의 Y 좌표
    [SerializeField] private Transform _noteSpawnPoint; // 노트가 생성될 위치를 가진 게임오브젝트

    [SerializeField] private float _timeOffset;

    private float _noteSpeed = 3000f;
    private string _musicJsonPath = "Assets/Resources/Music/Only_For_You.json";
    private bool _isPlaying = false;
    private float _playStartTime;
    private List<NoteData> _remainingNotes;

    public float PlayStartTime => _playStartTime;
    
    private void Start()
    {
        LoadNoteData();
        StartMusicAndNoteGeneration();

        _judgementLineY = _metronomeBar.anchoredPosition.y;
    }
    
    private void LoadNoteData()
    {
        var bgm = (AudioManager.Instance.CurrentBGM == BGM.None) ? BGM.Only_For_You : AudioManager.Instance.CurrentBGM;

        _musicJsonPath = $"Assets/Resources/Music/{bgm}.json";
        string jsonText = Resources.Load<TextAsset>(_musicJsonPath.Replace("Assets/Resources/", "").Replace(".json", "")).text;
        NoteDataContainer noteDataContainer = JsonUtility.FromJson<NoteDataContainer>(jsonText);
        _remainingNotes = new List<NoteData>(noteDataContainer.notes);
        
        // 시간순으로 정렬
        _remainingNotes.Sort((a, b) => a.time.CompareTo(b.time));
    }
    
    private void StartMusicAndNoteGeneration()
    {
        var bgm = (AudioManager.Instance.CurrentBGM == BGM.None) ? BGM.Only_For_You : AudioManager.Instance.CurrentBGM;

        AudioManager.Instance.LoadSong(bgm);
        AudioManager.Instance.PlaySong();
        _playStartTime = Time.time;
        _isPlaying = true;
    }
    
    private void Update()
    {
        if (!_isPlaying || _remainingNotes.Count == 0) return;
        
        float currentMusicTime = AudioManager.Instance.GetCurrentTime();

        // 생성해야 할 노트들 확인
        while (_remainingNotes.Count > 0 && ShouldSpawnNote(_remainingNotes[0], currentMusicTime))
        {
            SpawnNote(_remainingNotes[0]);
            _remainingNotes.RemoveAt(0);
        }
    }
    
    private bool ShouldSpawnNote(NoteData noteData, float currentMusicTime)
    {
        // 스폰 위치에서 판정선까지의 거리
        double distanceToTravel = Mathf.Abs(_noteSpawnPoint.GetComponent<RectTransform>().anchoredPosition.y - _metronomeBar.anchoredPosition.y);
        
        // 거리와 속도로 이동 시간 계산 (초 단위)
        double travelTimeInSeconds = distanceToTravel / _noteSpeed;
        // ms 단위로 변환
        double travelTimeInMs = travelTimeInSeconds * Define.MULTIPLIER_SEC_TO_MS;

        // 노트를 생성해야 할 시간 계산
        double spawnTiming = noteData.time - travelTimeInMs;
        Debug.Log($"spawnTiming: {spawnTiming}ms");
        
        return currentMusicTime >= spawnTiming - _timeOffset;
    }
    
    private void SpawnNote(NoteData noteData)
    {
        if (noteData.lane < 0 || noteData.lane >= _keyLines.Count)
        {
            Debug.LogError($"Invalid lane number: {noteData.lane}");
            return;
        }

        if (noteData.type == "LongNote")
        {
            SpawnLongNote(noteData);
        }
        else
        {
            SpawnShortNote(noteData);
        }
    }

    private void SpawnLongNote(NoteData noteData)
    {
        Transform keyLine = _keyLines[noteData.lane];
        GameObject note = Instantiate(_longNotePrefab, keyLine);

        RectTransform noteRect = note.GetComponent<RectTransform>();

        // 스폰 위치를 고정된 위치로 설정
        noteRect.anchoredPosition = new Vector2(0, _noteSpawnPoint.GetComponent<RectTransform>().anchoredPosition.y);

        LongNote longNoteComponent = note.GetComponent<LongNote>();
        longNoteComponent.Initialize(noteData, _noteSpeed, noteData.time / 1000, noteRect.anchoredPosition.y, _judgementLineY);
    }

    private void SpawnShortNote(NoteData noteData)
    {
        Transform keyLine = _keyLines[noteData.lane];
        GameObject note = Instantiate(_notePrefab, keyLine);
        RectTransform noteRect = note.GetComponent<RectTransform>();
            
        // 스폰 위치를 고정된 위치로 설정
        noteRect.anchoredPosition = new Vector2(0, _noteSpawnPoint.GetComponent<RectTransform>().anchoredPosition.y);
            
        Note noteComponent = note.GetComponent<Note>();
        noteComponent.Initialize(noteData, _noteSpeed, noteData.time / 1000f, noteRect.anchoredPosition.y, _judgementLineY);
    }
}
