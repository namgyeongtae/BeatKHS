using System.Collections;
using System.Collections.Generic;
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

    void Update()
    {
        if (IsEndMusic() && !_isEndMusic)
        {
            Debug.Log("Music End");
            Managers.Scene.LoadScene("SelectionScene").Forget();
            _isEndMusic = true;
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
        if (AudioManager.Instance.SongPosition >= AudioManager.Instance.SongLength)
        {
            return true;
        }
        return false;
    }
}
