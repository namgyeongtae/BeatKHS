using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Video;

public class UIMusicList : CanvasPanel
{
    [Bind("MusicContent")] private GameObject _songGroup;
    [Bind("Thumbnail")] private Image _thumbnail;
    private ScrollRect _scrollRect;

    private List<UIMusicSelection> _musicList = new List<UIMusicSelection>();

    private UIMusicSelection _selectedMusic;
    private int _selectedIndex = 0;
    private bool _isScrolling = false;

    private float _scrollStep = 0.13f;

    private float _timer = 0f;

    private VideoPlayer _videoPlayer;
    private Coroutine _videoChangeCoroutine;

    public UIMusicSelection SelectedMusic => _selectedMusic;

    protected override void Start()
    {
        base.Start();
        _scrollRect = GetComponent<ScrollRect>();
        
        _videoPlayer.prepareCompleted -= OnPrepareCompleted;
        _videoPlayer.prepareCompleted += OnPrepareCompleted;
    }

    public override void Open()
    {
        base.Open();
        LoadMusicList();

        _videoPlayer = FindObjectOfType<VideoPlayer>();
        _videoPlayer.clip = Managers.Resource.Load<VideoClip>($"Video/{_selectedMusic.MusicData.video}");
        _videoPlayer.SetDirectAudioMute(0, false);
        _videoPlayer.Prepare();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Scroll(-_scrollStep);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Scroll(_scrollStep);
        }
    }

    public void LoadMusicList()
    {
        var musicList = Managers.Data.MusicList;

        foreach (var song in musicList)
        {
            var songUI = Managers.Resource.Instantiate("Song", _songGroup.transform);
            var musicSelection = songUI.GetComponent<UIMusicSelection>();
            musicSelection.SetMusicInfo(song);

            _musicList.Add(musicSelection);
        }

        _selectedMusic = _musicList[0];
        _thumbnail.sprite = Managers.Resource.Load<Sprite>($"Sprites/Album/{_selectedMusic.MusicData.album}");

        SelectMusic(_selectedMusic);
    }

    private void Scroll(float amount)
    {
        if (_isScrolling)
            return;

        if (amount < 0)
        {
            _selectedIndex = Mathf.Clamp(_selectedIndex + 1, 0, _musicList.Count - 1);
        }
        else
        {
            _selectedIndex = Mathf.Clamp(_selectedIndex - 1, 0, _musicList.Count - 1);
        }

        SelectMusic(_musicList[_selectedIndex]);

        float nextPos = _scrollRect.verticalNormalizedPosition + amount;
        
        // 스크롤 한계값을 약간 더 여유있게 설정 (-0.01f, 1.01f)
        if (nextPos < -0.1f || nextPos > 1.1f)
        {
            Debug.Log($"스크롤 한계값을 넘었습니다. {nextPos}");
            return;
        }
        
        _isScrolling = true;

        _scrollRect.DOVerticalNormalizedPos(nextPos, 0.5f).OnComplete(() =>
        {
            _isScrolling = false;
        });
    }

    private void SelectMusic(UIMusicSelection musicSelection)
    {
        /* if (_selectedMusic == musicSelection)
        {
            return;
        } */
        _selectedMusic.Deselected();
        _selectedMusic = musicSelection;
        _selectedMusic.Selected();

        

        // 이전 코루틴이 실행 중이라면 중지
        if (_videoChangeCoroutine != null)
            StopCoroutine(_videoChangeCoroutine);
        
        // 새로운 비디오 변경 코루틴 시작
        _videoChangeCoroutine = StartCoroutine(ChangeVideoAfterDelay());
    }

    private IEnumerator ChangeVideoAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        
        if (_selectedMusic != null && !string.IsNullOrEmpty(_selectedMusic.MusicData.video))
        {
            _thumbnail.sprite = Managers.Resource.Load<Sprite>($"Sprites/Album/{_selectedMusic.MusicData.album}");
            _videoPlayer.clip = Managers.Resource.Load<VideoClip>($"Video/{_selectedMusic.MusicData.video}");
            _videoPlayer.Prepare();
        }
    }

    private void OnPrepareCompleted(VideoPlayer vp)
    {
        vp.time = _selectedMusic.MusicData.showtime;
        vp.Play();
    }
}
