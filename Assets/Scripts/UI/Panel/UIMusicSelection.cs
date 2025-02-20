using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMusicSelection : CanvasPanel
{
    [Bind("Album")] private Image _album;
    [Bind("MusicTitle")] private TextMeshProUGUI _musicTitle;
    [Bind("Selected")] private Image _selected;

    private MusicData _musicData;

    public MusicData MusicData => _musicData;

    public void SetMusicInfo(MusicData InMusicData)
    {
        _musicData = InMusicData;
        _album.sprite = Managers.Resource.Load<Sprite>($"Sprites/Album/{InMusicData.album}");
        _musicTitle.text = $"{InMusicData.title} - {InMusicData.artist}";
    }

    public void Selected()
    {
        // TODO 
        // 선택됨을 표시하기 위해 UI를 배경을 변경
        _selected.gameObject.SetActive(true);
    }

    public void Deselected()
    {
        // TODO
        // 선택되지 않음을 표시하기 위해 UI를 배경을 변경
        _selected.gameObject.SetActive(false);
    }
}
