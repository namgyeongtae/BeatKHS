using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public List<List<Note>> Notes = new List<List<Note>>();

    private bool _isEndMusic = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitNoteList();
    }

    private async void Update()
    {
        Debug.Log("MusicManager Update Running");

        if (IsEndMusic() && !_isEndMusic)
        {
            Debug.Log("Starting EndMusic Coroutine");
            await EndMusic();
        }
    }

    void InitNoteList()
    {
        // 4는 이후에 세팅한 값이 들어갈 필요가 있을 수도
        // 현재 노트 라인 수 만큼 초기화
        // 이후에 4노트 말고도 확정 가능성 고려
        for (int i = 0; i < 4; i++)
        {
            Notes.Add(new List<Note>());
        }
    }

    private bool IsEndMusic()
    {
        // 모든 노트가 처리되었는지 확인
        bool allNotesProcessed = true;
        foreach (var laneNotes in Notes)
        {
            if (laneNotes.Count > 0)
            {
                allNotesProcessed = false;
                break;
            }
        }

        bool isMusicEnd = Managers.Audio.IsMusicEnd();
        Debug.Log($"AllNotesProcessed: {allNotesProcessed}, IsMusicEnd: {isMusicEnd}"); // 상태 로깅
        // 음악이 끝났고 모든 노트가 처리되었는지 확인
        return isMusicEnd && allNotesProcessed;
    }

    private async Task EndMusic()
    {
        _isEndMusic = true;
        var sceneAnimator = GameObject.Find("SceneAnimator").GetComponent<Animator>();
        sceneAnimator.SetTrigger("FadeIn");

        await Task.Delay((int)(sceneAnimator.GetCurrentAnimatorStateInfo(0).length * 1000));
        await Task.Delay(1000);
        
        var scoreDisplay = Managers.UI.AddPanel<UIScoreDisplay>("UIScoreDisplay");
        scoreDisplay.transform.SetSiblingIndex(scoreDisplay.transform.parent.childCount - 2);
        scoreDisplay.gameObject.SetActive(false);

        ScoreInfo scoreInfo = new ScoreInfo();
        scoreInfo.BestScore = Managers.Score.BestScore;
        scoreInfo.BestCombo = Managers.Score.BestCombo;
        scoreInfo.JudgeRate = Managers.Score.JudgeRate;
        scoreInfo.PerfectCount = Managers.Score.PerfectCount;
        scoreInfo.GoodCount = Managers.Score.GoodCount;
        scoreInfo.BadCount = Managers.Score.BadCount;
        scoreInfo.MissCount = Managers.Score.MissCount;

        await Managers.FirebaseData.SaveScoreData(Managers.Auth.UserData.UserName, Managers.Audio.CurrentBGM.ToString(), scoreInfo);
        await Task.Delay(1000);
        sceneAnimator.SetTrigger("FadeOut");  
        scoreDisplay.gameObject.SetActive(true);
    }
}
