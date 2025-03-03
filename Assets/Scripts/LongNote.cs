using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LongNote : Note
{
    [SerializeField] private GameObject _startNote;
    [SerializeField] private GameObject _bodyNote;
    [SerializeField] private GameObject _endNote;

    private int _targetCombo;

    private bool _isMissed = false;
    private bool _isJudged = false;

    public GameObject StartNote => _startNote;
    public GameObject BodyNote => _bodyNote;
    public GameObject EndNote => _endNote;

    public int TargetCombo => _targetCombo;
    public bool IsMissed => _isMissed;
    public bool IsJudged => _isJudged;

    public override void Initialize(NoteData noteData, float speed, float targetTimeInSeconds, float distance, float judgementLineY)
    {
        base.Initialize(noteData, speed, targetTimeInSeconds, distance, judgementLineY);

        SetLongNote(_noteData);
    }
    
    protected override void Update()
    {
        // 노트를 아래로 이동
        // transform.Translate(Vector3.down * _speed * Time.deltaTime);
        Rect.anchoredPosition += Vector2.down * _speed * Time.deltaTime;
        
        Vector3[] noteCorners = new Vector3[4];
        _endNote.GetComponent<RectTransform>().GetWorldCorners(noteCorners);
        float endNoteWorldY = noteCorners[1].y;  // 노트의 상단 Y좌표

        _startNote.GetComponent<RectTransform>().GetWorldCorners(noteCorners);
        float startNoteWorldY = noteCorners[1].y;

        // startNote가 판정선 아래에 있는데
        // 키 입력이 되어 있지 않고
        // 롱노트가 미스되지 않았다면
        // TODO : 롱노트가 TARGET_COMBO 이상의 콤보를 획득했는지 확인 
        // 획득했다면 미스처리를 하지 않고
        // 획득하지 못했다면 미스처리를 한다.
        if (startNoteWorldY <= Define.NOTE_REMOVE_Y && !KeyInputManager.Instance.IsKeyInputDown[_noteData.lane] && !_isMissed && !_isJudged)
        {
            Debug.Log("LongNote Miss");
            _isMissed = true;
            Managers.UI.GetUI<ComboUI>("ComboUI").ComboMiss();
            Managers.UI.GetUI<JudgeUI>("JudgeUI").SetJudgeImage(JudgeType.Miss);
        }

        // 판정선 도달 여부 확인 및 처리
        if (endNoteWorldY <= Define.NOTE_REMOVE_Y)
        {
            RemoveNote();
        }
    }

    void SetLongNote(NoteData noteData)
    {
        // 롱노트의 지속 시간(ms)을 초 단위로 변환
        float durationInSeconds = noteData.duration / 1000f;
        
        // 실제 이동해야 할 픽셀 거리 계산
        float desiredPixelLength = durationInSeconds * _speed;
        
        var noteRect = _bodyNote.GetComponent<RectTransform>();
        var parentRect = noteRect.parent.GetComponent<RectTransform>();
        
        // 부모의 높이를 고려하여 sizeDelta 계산
        float normalizedLength = desiredPixelLength / parentRect.rect.height;
        float adjustedLength = normalizedLength * parentRect.rect.height;
        
        // 노트의 높이 설정
        Vector2 sizeDelta = noteRect.sizeDelta;
        sizeDelta.y = adjustedLength;
        noteRect.sizeDelta = sizeDelta;

        var endPosY = noteRect.localPosition.y + noteRect.sizeDelta.y - _endNote.GetComponent<RectTransform>().sizeDelta.y;
        _endNote.GetComponent<RectTransform>().localPosition = new Vector2(0, endPosY);

        // targetCombo 계산
        // 예: 매 0.1초마다 1콤보씩 증가
        float comboInterval = 0.1f; // 0.1초마다 1콤보
        _targetCombo = Mathf.CeilToInt(durationInSeconds / comboInterval);
        
        // 최소 콤보 보장
        _targetCombo = Mathf.Max(_targetCombo, 1);
    }

    public void SetNoteJudged()
    {
        _isJudged = true;
    }

    public void SetNoteMiss()
    {
        _startNote.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        _bodyNote.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        _endNote.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
    }
}
