using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    private FMOD.ChannelGroup _bgmChannelGroup;
    private FMOD.Sound[] _bgms;
    private FMOD.Channel _bgmChannel;

    [Header("SFX")]
    private FMOD.ChannelGroup _sfxChannelGroup;
    private FMOD.Sound[] _sfxs;
    private FMOD.Channel[] _sfxChannels;

    [Header("Audio Settings")]
    [SerializeField, Range(0f, 1f)] private float _bgmVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float _sfxVolume = 0.5f;
    [SerializeField] private float _offset = 0f; // 노트 타이밍 오프셋

    private double _dspStartTime;  // 음악 시작 시의 DSP 시간
    private bool _isPlaying = false;
    private float _songPosition = 0f;
    private float _songLength = 0f;
    private uint _songLengthInMs = 0;

    public BGM CurrentBGM = BGM.None;

    public bool IsPlaying => _isPlaying;
    public float SongPosition => _songPosition;
    public float SongLength => _songLength;
    public float SongProgress => _songPosition / _songLength;

    private void Awake()
    {
        Instance = this;
        InitializeAudio();

        DontDestroyOnLoad(gameObject);
    }

    private void InitializeAudio()
    {
        // BGM 채널 그룹 생성
        _bgmChannelGroup = new FMOD.ChannelGroup();
        LoadBGM();
        LoadSFX();
    }

    void LoadBGM()
    {
        int count = System.Enum.GetValues(typeof(BGM)).Length;
        _bgms = new FMOD.Sound[count];

        for (int i = 0; i < count; i++)
        {
            string bgmFileName = System.Enum.GetName(typeof(BGM), i);
            string audioType = "mp3";

            FMODUnity.RuntimeManager.CoreSystem.createSound(
                Path.Combine(Application.streamingAssetsPath, "BGMS", bgmFileName + "." + audioType),
                FMOD.MODE.LOOP_NORMAL | FMOD.MODE.CREATESTREAM,
                out _bgms[i]
            );
        }
    }

    void LoadSFX()
    {
        int count = System.Enum.GetValues(typeof(SFX)).Length;

        _sfxChannelGroup = new FMOD.ChannelGroup();
        _sfxChannels = new FMOD.Channel[count];
        _sfxs = new FMOD.Sound[count];

        for (int i = 0; i < count; i++)
        {
            string sfxFileName = System.Enum.GetName(typeof(SFX), i);
            string audioType = "mp3";

            FMODUnity.RuntimeManager.CoreSystem.createSound(Path.Combine(Application.streamingAssetsPath, "SFXS", sfxFileName, "." + audioType), FMOD.MODE.CREATESAMPLE, out _sfxs[i]);
        }

        for (int i = 0; i < count; i++)
        {
            _sfxChannels[i].setChannelGroup(_sfxChannelGroup);
        }
    }

    public void LoadSong(BGM bgm)
    {
        if (_bgmChannel.hasHandle())
            _bgmChannel.stop();

        int bgmIndex = (int)bgm;
        FMOD.Sound sound = _bgms[bgmIndex];
        
        sound.setMode(FMOD.MODE.DEFAULT);

        // 곡 길이 가져오기
        sound.getLength(out _songLengthInMs, FMOD.TIMEUNIT.MS);
        _songLength = _songLengthInMs / 1000f;
        
        FMODUnity.RuntimeManager.CoreSystem.playSound(sound, _bgmChannelGroup, true, out _bgmChannel);
        _bgmChannel.setVolume(_bgmVolume);
        _isPlaying = false;
        _songPosition = 0f;
    }

    public void PlaySong(bool loop = false, float startTime = 0f)
    {
        if (!_bgmChannel.hasHandle()) return;
        
        _bgmChannel.setPosition((uint)(startTime * 1000), FMOD.TIMEUNIT.MS);
        _bgmChannel.setPaused(false);
        _bgmChannel.setLoopCount(loop ? -1 : 0);
        _isPlaying = true;
    }

    public void PauseSong()
    {
        if (!_bgmChannel.hasHandle()) return;

        _bgmChannel.setPaused(true);
        _isPlaying = false;
    }

    public void ResumeSong()
    {
        if (!_bgmChannel.hasHandle()) return;

        _bgmChannel.setPaused(false);
        _isPlaying = true;
    }

    public void StopSong()
    {
        if (!_bgmChannel.hasHandle()) return;

        _bgmChannel.stop();
        _isPlaying = false;
        _songPosition = 0f;
    }

    private void Update()
    {
        if (_isPlaying && _bgmChannel.hasHandle())
        {
            _bgmChannel.getPosition(out uint pos, FMOD.TIMEUNIT.MS);
            _songPosition = (pos / 1000f) + _offset;
        }
    }

    public float GetCurrentTime()
    {
        return _songPosition * 1000f;  // 초를 밀리초로 변환 (× 1000)
    }

    public float GetCurrentBeat(float bpm)
    {
        return _songPosition * (bpm / 60f);
    }

    public bool IsOnBeat(float bpm, float threshold = 0.05f)
    {
        float currentBeat = GetCurrentBeat(bpm);
        return Mathf.Abs(currentBeat - Mathf.Round(currentBeat)) < threshold;
    }

    public void SetOffset(float offset)
    {
        _offset = offset;
    }

    // 효과음 재생 (노트 히트, 미스 등)
    public void PlaySFX(SFX sfx, float pitch = 1.0f)
    {
        int sfxIndex = (int)sfx;
        _sfxChannels[sfxIndex].stop();

        FMODUnity.RuntimeManager.CoreSystem.playSound(_sfxs[sfxIndex], _sfxChannelGroup, false, out _sfxChannels[sfxIndex]);
        
        _sfxChannels[sfxIndex].setPaused(true);
        _sfxChannels[sfxIndex].setVolume(_sfxVolume);
        _sfxChannels[sfxIndex].setPitch(pitch);
        _sfxChannels[sfxIndex].setPaused(false);
    }

    public void SetBGMVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);
        if (_bgmChannel.hasHandle())
        {
            _bgmChannel.setVolume(_bgmVolume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        if (_sfxChannelGroup.hasHandle())
        {
            _sfxChannelGroup.setVolume(_sfxVolume);
        }
    }

    private void OnDestroy()
    {
        // BGM 리소스 해제
        if (_bgmChannel.hasHandle())
            _bgmChannel.stop();
        
        foreach (var bgm in _bgms)
        {
            if (bgm.hasHandle())
                bgm.release();
        }

        // SFX 리소스 해제
        foreach (var channel in _sfxChannels)
        {
            if (channel.hasHandle())
                channel.stop();
        }

        foreach (var sfx in _sfxs)
        {
            if (sfx.hasHandle())
                sfx.release();
        }

        if (_bgmChannelGroup.hasHandle())
            _bgmChannelGroup.release();
        if (_sfxChannelGroup.hasHandle())
            _sfxChannelGroup.release();
    }
}