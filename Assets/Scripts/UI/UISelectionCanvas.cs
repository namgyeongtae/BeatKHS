using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class UISelectionCanvas : CanvasPanel
{
    [Bind("MusicList")] private UIMusicList _musicList;
    [Bind("PlayButton")] private UIButton _playButton;
    [Bind("Speed")] private TextMeshProUGUI _speedText;

    protected override void Initialize()
    {
        base.Initialize();

        _playButton.BindEvent(() => { 
        
            AudioManager.Instance.CurrentBGM = Enum.Parse<BGM>(_musicList.SelectedMusic.MusicData.clip);
            SceneManager.LoadScene("InGameScene");
        });
    }

    void Update()
    {
        OnUpdateNoteSpeed();
    }

    private void OnUpdateNoteSpeed()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _speedText.text = Mathf.Clamp(float.Parse(_speedText.text) + 0.5f, Define.NOTE_SPEED_MIN / 1000f, Define.NOTE_SPEED_MAX / 1000f).ToString("F1");
            Setting.NOTE_SPEED = float.Parse(_speedText.text) * 1000f;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _speedText.text = Mathf.Clamp(float.Parse(_speedText.text) - 0.5f, Define.NOTE_SPEED_MIN / 1000f, Define.NOTE_SPEED_MAX / 1000f).ToString("F1");
            Setting.NOTE_SPEED = float.Parse(_speedText.text) * 1000f;
        }
    }
}
