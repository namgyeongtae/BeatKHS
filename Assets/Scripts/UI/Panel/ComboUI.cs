using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ComboUI : CanvasPanel
{
    [Bind("ComboText")] private TextMeshProUGUI _comboText;
    [Bind("Combo")] private TextMeshProUGUI _combo;

    private int _currentCombo = 0;

    public void ComboEffect()
    {
        _comboText.gameObject.SetActive(true);
        _currentCombo++;

        _combo.text = _currentCombo.ToString();

        _comboText.DOKill();

        Vector3 originalPos = _comboText.transform.localPosition;

        // 살짝 아래로 이동 후 튀어오르는 애니메이션
        _comboText.transform.DOLocalMoveY(originalPos.y - 10f, 0.05f)
                            .SetEase(Ease.OutQuad)
                            .OnComplete(() =>
                            {
                                _comboText.transform.DOLocalMoveY(originalPos.y, 0.1f).SetEase(Ease.OutBounce);
                            });

        // 크기 약간 변화 (강조 효과)
        _comboText.transform.DOScale(1.2f, 0.05f).OnComplete(() => _comboText.transform.DOScale(1f, 0.1f));
    }

    public void ComboMiss()
    {
        _currentCombo = 0;
        _combo.text = _currentCombo.ToString();

        _comboText.gameObject.SetActive(false);
    }
}


