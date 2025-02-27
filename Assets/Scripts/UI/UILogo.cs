using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System;
using System.Security.Cryptography;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UILogo : CanvasPanel
{
    [SerializeField] private float beatScale = 1.05f;     // 비트 감지시 통통튀는 크기
    [SerializeField] private float smoothSpeed = 8f;      // 크기 변화 부드러움
    [SerializeField] private float minThreshold = 0.4f;   // 최소 임계값
    [SerializeField] private float maxThreshold = 0.8f;   // 최대 임계값
    [SerializeField] private float beatCooldown = 0.2f;   // 비트 감지 쿨다운 시간

    private Image _image_Logo;
    private UIButton _button_Logo;
    
    private Vector3 originalScale;
    private float currentPulse = 1f;
    private float lastBeatTime = -1f;
    private FMOD.ChannelGroup masterGroup;
    private FMOD.DSP spectrumDSP;

    private bool _isClicked = false;
    
    protected override void Start()
    {
        _image_Logo = GetComponent<Image>();
        _button_Logo = GetComponent<UIButton>();

        _button_Logo.BindEvent(() => { StartCoroutine(OnClickLogo()); });

        originalScale = transform.localScale;
        
        RuntimeManager.CoreSystem.getMasterChannelGroup(out masterGroup);
        FMOD.DSP dsp;
        RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out dsp);
        spectrumDSP = dsp;
        masterGroup.addDSP(0, spectrumDSP);
        spectrumDSP.setParameterInt((int)FMOD.DSP_FFT.WINDOWTYPE, (int)FMOD.DSP_FFT_WINDOW.HANNING);
        spectrumDSP.setParameterInt((int)FMOD.DSP_FFT.WINDOWSIZE, 1024);

        base.Start();
    }
    
    private void Update()
    {
        if (_isClicked)
            return;

        if (IsMouseOver())
        {
            _image_Logo.rectTransform.localScale = Vector3.one * 1.1f;
        }
        else
        {
            _image_Logo.rectTransform.localScale = Vector3.one;
        }

        BounceByBeat();
    }

    private IEnumerator OnClickLogo()
    {
        _isClicked = true;

        _image_Logo.rectTransform.localScale = Vector3.one * 1.1f;

        var sceneAnimator = GameObject.Find("SceneAnimator").GetComponent<Animator>();
        sceneAnimator.SetTrigger("FadeIn");

        yield return new WaitForSeconds(2f);

        SceneManager.LoadSceneAsync("SelectionScene");
    }

    private void BounceByBeat()
    {
        float intensity = GetAudioIntensity() * 1000;
        
        // 디버그용 로그
        Debug.Log($"Audio Intensity: {intensity}");
        
        // 쿨다운 체크 후 비트 감지 (최소값과 최대값 사이일 때만 감지)
        if (intensity >= minThreshold && intensity <= maxThreshold && 
            Time.time > lastBeatTime + beatCooldown)
        {
            currentPulse = beatScale;
            lastBeatTime = Time.time;
        }
        else
        {
            // 부드럽게 원래 크기로 돌아감
            currentPulse = Mathf.Lerp(currentPulse, 1f, Time.deltaTime * smoothSpeed);
        }
        
        // 크기 적용
        var rectTr = GetComponent<RectTransform>();
        rectTr.localScale = rectTr.localScale * currentPulse;
    }
    
    private float GetAudioIntensity()
    {
        // FFT 데이터를 저장할 배열
        FMOD.DSP_PARAMETER_FFT spectrum;
        spectrumDSP.getParameterData((int)FMOD.DSP_FFT.SPECTRUMDATA, out IntPtr ptr, out uint length);
        spectrum = (FMOD.DSP_PARAMETER_FFT)System.Runtime.InteropServices.Marshal.PtrToStructure(ptr, typeof(FMOD.DSP_PARAMETER_FFT));
        
        if (spectrum.length == 0 || spectrum.spectrum == null || spectrum.spectrum.Length == 0)
        {
            return 0f;
        }

        // 저주파 영역의 평균 강도를 계산
        float sum = 0f;
        int sampleCount = Mathf.Min(64, spectrum.length);
        
        for (int i = 0; i < sampleCount; i++)
        {
            sum += spectrum.spectrum[0][i]; // [0]은 왼쪽 채널
        }
        
        return sum / sampleCount;
    }

    private bool IsMouseOver()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition);
    }

    private void OnDestroy()
    {
        if (spectrumDSP.hasHandle())
        {
            spectrumDSP.release();
        }
    }
}
