using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : Button
{
    private static readonly Vector3 SMALL_SCALE = new Vector3(0.9f, 0.9f, 0.9f);
    private static readonly float SCALE_DURATION = 0.1f;
    private Vector3 _initScale = Vector3.one;
    
    protected Coroutine _coroutine_UpAndDown = null;

    protected override void Awake()
    {
        base.Awake();

        _initScale = transform.localScale;
    }

    protected override void OnDisable()
    {
        if (_coroutine_UpAndDown != null)
        {
            StopCoroutine(_coroutine_UpAndDown);
            _coroutine_UpAndDown = null;
            transform.localScale = _initScale;
        }
        base.OnDisable();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!isActiveAndEnabled)
            return;
        
        if (_coroutine_UpAndDown != null)
        {
            StopCoroutine(_coroutine_UpAndDown);
            _coroutine_UpAndDown = null;
        }

        if (!interactable)
            return;

        _coroutine_UpAndDown = StartCoroutine(CoScaleUpAndDown(SMALL_SCALE, SCALE_DURATION));
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!isActiveAndEnabled)
            return;

        Debug.Log("OnPointerClick");
        PressEvent();
    }

    private void PressEvent()
    {
        if (!IsActive() || !IsInteractable())
            return;

        onClick?.Invoke();
    }

    public void BindEvent(UnityAction action)
    {
        onClick.RemoveAllListeners();
        onClick.AddListener(action);
    }

    IEnumerator CoScaleUpAndDown(Vector3 InUpScale, float InDuration)
    {
        Vector3 initialScale = _initScale;

        for (float time = 0; time < InDuration * 2; time += Time.deltaTime)
        {
            float progress = Mathf.PingPong(time, InDuration) / InDuration;
            transform.localScale = Vector3.Lerp(initialScale, InUpScale, progress);
            yield return null;
        }
        transform.localScale = initialScale;
    }
}
