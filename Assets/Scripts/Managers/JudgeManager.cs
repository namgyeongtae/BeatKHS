using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class JudgeManager : MonoBehaviour
{
    public static JudgeManager Instance { get; private set; }

    [SerializeField] RectTransform _center = null;
    [SerializeField] RectTransform[] _timingRect = null;

    JudgeType[] _judgeTypeList = { JudgeType.Bad, JudgeType.Good, JudgeType.Perfect };

    Vector2[] _timingBoxs = null;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _timingBoxs = new Vector2[_timingRect.Length];

        for (int i = 0; i < _timingRect.Length; i++)
        {
            // 타이밍 박스의 위치를 스크린 좌표계로 변환
            Vector3[] corners = new Vector3[4];
            _timingRect[i].GetWorldCorners(corners);
            float minY = corners[0].y;
            float maxY = corners[1].y;
            _timingBoxs[i] = new Vector2(minY, maxY);
        }
    }

    public void Judge(int lane)
    {
        if (MusicManager.Instance.Notes[lane].Count == 0)
        {
            return;
        }

        var targetNote = MusicManager.Instance.Notes[lane][0];

        // 판정선과의 거리 계산
        // 좌표계의 통일을 위한 GetWorldCorners 사용
        Vector3[] noteCorners = new Vector3[4];
        targetNote.GetComponent<RectTransform>().GetWorldCorners(noteCorners);
        float noteWorldY = noteCorners[0].y;

        Vector3[] judgementCorners = new Vector3[4];
        _center.GetWorldCorners(judgementCorners);
        float judgementWorldY = judgementCorners[0].y;

        float maxJudgeDistance = 200f; // 적절한 값으로 조정하세요
        if (Mathf.Abs(noteWorldY - judgementWorldY) <= maxJudgeDistance)
        {
            if (targetNote.NoteData.type == "ShortNote")
            {
                JudgeShortNote(targetNote, lane);
            }
            else
            {
                StartCoroutine(JudgeLongNote((LongNote)targetNote, lane));
            }
        }
    }

    public void JudgeShortNote(Note targetNote, int lane)
    {
        if (targetNote == null) return;

        // 노트의 위치를 스크린 좌표계로 변환
        Vector3[] noteCorners = new Vector3[4];
        targetNote.GetComponent<RectTransform>().GetWorldCorners(noteCorners);
        float t_notePosY = noteCorners[0].y;

        JudgeType result = JudgeType.Miss;

        for (int y = _timingBoxs.Length - 1; y >= 0; y--)
        {
            if (_timingBoxs[y].x <= t_notePosY && t_notePosY <= _timingBoxs[y].y)
            {
                Debug.Log("Hit " + _judgeTypeList[y].ToString());
                result = _judgeTypeList[y];

                if (result != JudgeType.Miss && result != JudgeType.Bad)
                {
                    Managers.UI.ShowHitEffect(targetNote.transform);
                }

                break;
            }
        }

        // Notes 리스트에서 제거
        if (MusicManager.Instance.Notes[lane].Count > 0)
        {
            MusicManager.Instance.Notes[lane].RemoveAt(0);
        }
        
        // 오브젝트 파괴
        if (targetNote != null)
        {
            Destroy(targetNote.gameObject);
        }

        Debug.Log(result == JudgeType.Miss ? "Miss" : result.ToString());
    }

    public IEnumerator JudgeLongNote(LongNote targetNote, int lane)
    {
        float duration = targetNote.NoteData.duration;
        // JudgeType initialJudge = JudgeShortNote(targetNote, lane);
        JudgeType result = JudgeType.Miss;
        
        // 노트의 위치를 스크린 좌표계로 변환
        Vector3[] noteCorners = new Vector3[4];
        targetNote.Rect.GetWorldCorners(noteCorners);
        float t_notePosY = noteCorners[0].y;
        
        for (int y = _timingBoxs.Length - 1; y >= 0; y--)
        {
            if (_timingBoxs[y].x <= t_notePosY && t_notePosY <= _timingBoxs[y].y)
            {
                result = _judgeTypeList[y];
                Debug.Log("Hit Long Note" + result.ToString());
                break;
            }
        }

        if (result == JudgeType.Miss || result == JudgeType.Bad)
        {
            Debug.Log("Miss or Bad Long Note");
            targetNote.SetNoteMiss();
            yield break;
        }

        var hitEffect = Managers.UI.ShowLongHitEffect(targetNote.transform);

        Vector3[] endNoteCorners = new Vector3[4];
        targetNote.GetComponent<LongNote>().EndNote.GetComponent<RectTransform>().GetWorldCorners(endNoteCorners);
        float endNotePosY = endNoteCorners[0].y;

        // 레인에 해당하는 키를 누르는 동안안
        while (KeyInputManager.Instance.IsKeyInputDown[lane])
        {
            // 매 프레임마다 엔드 노트의 현재 스크린 좌표 업데이트
            targetNote.GetComponent<LongNote>().EndNote.GetComponent<RectTransform>().GetWorldCorners(endNoteCorners);
            endNotePosY = endNoteCorners[0].y;

            Debug.Log($"EndNotePos : {endNotePosY}, T_NotePosY : {t_notePosY}");
            if (targetNote == null || !targetNote.gameObject.activeInHierarchy || endNotePosY < t_notePosY)
            {
                Debug.Log("End Note is out of range");
                break;
            }

            yield return null;
        }

        // 키를 놓았을 때의 처리
        // TODO : 키를 놓았을 때 EndNote의 위치를 확인하여 또 판정을 내려야 한다.
        // 적절한 위치에서 롱노트를 떼지 못하면 miss 혹은 bad 판정을 내려서 노트에 투명도를 부여하고 
        // 콤보 카운트를 초기화 해야한다.

        
        if (hitEffect != null)
        {
            Debug.Log("Destroy Long Hit Effect");
            Managers.Resource.Destroy(hitEffect);
        }

        // TODO
        // 현재 키 입력을 떼기만 해도 제거하고 있는 상황 
        // 롱노트는 endNote가 판정선 아래로 내려가야지만 제거 되야 한다.
        // 키를 놓았을 때의 처리  
        targetNote.RemoveNote();
    }
}

