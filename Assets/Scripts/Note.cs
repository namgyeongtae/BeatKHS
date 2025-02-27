using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    protected NoteData _noteData;
    protected float _speed;
    protected float _targetTime;
    protected float _distance;
    protected float _judgementLineY;
    
    public NoteData NoteData => _noteData;
    public RectTransform Rect => GetComponent<RectTransform>();
    
    public virtual void Initialize(NoteData noteData, float speed, float targetTimeInSeconds, float distance, float judgementLineY)
    {
        _noteData = noteData;
        _speed = speed;
        _targetTime = targetTimeInSeconds; // 이미 초 단위로 변환되어 전달됨
        _distance = distance;
        _judgementLineY = judgementLineY;

        AddNote();
    }
    
    protected virtual void Update()
    {
        // 노트를 아래로 이동
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        // 화면 밖으로 완전히 벗어났는지만 체크
        Vector3[] noteCorners = new Vector3[4];
        Rect.GetWorldCorners(noteCorners);
        float noteWorldY = noteCorners[1].y;  // 노트의 상단 Y좌표

        // 화면 밖으로 완전히 벗어났을 때만 제거
        if (noteWorldY < Define.NOTE_REMOVE_Y)  // 적절한 값으로 조정하세요
        {
            Debug.Log("Result Miss");
            Managers.UI.GetUI<ComboUI>("ComboUI").ComboMiss();
            Managers.UI.GetUI<JudgeUI>("JudgeUI").SetJudgeImage(JudgeType.Miss);
            RemoveNote();
        }
    }


    public void AddNote()
    {
        if (!MusicManager.Instance.Notes[_noteData.lane].Contains(this))
        {   
            MusicManager.Instance.Notes[_noteData.lane].Add(this);
            return;
        }

        Debug.LogError("Note already exists in the list");
    }

    public void RemoveNote()
    {
        MusicManager.Instance.Notes[_noteData.lane].Remove(this);
        Managers.Resource.Destroy(gameObject);

        Debug.Log($"Note removed: {this.gameObject.name}, {_noteData.lane}");
    }
}
