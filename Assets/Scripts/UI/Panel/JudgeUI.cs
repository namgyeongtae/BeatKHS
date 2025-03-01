using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class JudgeUI : CanvasPanel
{
    [Bind("Perfect")] private Image _perfect;
    [Bind("Good")] private Image _good;
    [Bind("Bad")] private Image _bad;
    [Bind("Miss")] private Image _miss;

    private Dictionary<JudgeType, int> _judgeTypeCount = new Dictionary<JudgeType, int>();

    private Image _currentImage = null;
    private JudgeType _currentJudgeType = JudgeType.Perfect;

    private Vector3 _originalJudgeImagePosition;

    protected override void Initialize()
    {
        _judgeTypeCount.Add(JudgeType.Perfect, 0);
        _judgeTypeCount.Add(JudgeType.Good, 0);
        _judgeTypeCount.Add(JudgeType.Bad, 0);
        _judgeTypeCount.Add(JudgeType.Miss, 0);

        _originalJudgeImagePosition = _perfect.transform.localPosition;
    }

    public void SetJudgeImage(JudgeType judgeType)
    {
        if (_currentImage != null)
        {
            _currentImage.gameObject.SetActive(false);
        }

        switch (judgeType)
        {
            case JudgeType.Perfect:
                _currentImage = _perfect;
                _perfect.gameObject.SetActive(true);
                _judgeTypeCount[JudgeType.Perfect]++;
                break;
            case JudgeType.Good:
                _currentImage = _good;
                _good.gameObject.SetActive(true);
                _judgeTypeCount[JudgeType.Good]++;
                break;
            case JudgeType.Bad:
                _currentImage = _bad;
                _bad.gameObject.SetActive(true);
                _judgeTypeCount[JudgeType.Bad]++;
                break;
            case JudgeType.Miss:
                _currentImage = _miss;
                _miss.gameObject.SetActive(true);
                _judgeTypeCount[JudgeType.Miss]++;
                break;
        }

        _currentImage.DOKill();

        // 살짝 아래로 이동 후 튀어오르는 애니메이션
        _currentImage.transform.DOLocalMoveY(_originalJudgeImagePosition.y - 10f, 0.05f)
                            .SetEase(Ease.OutQuad)
                            .OnComplete(() =>
                            {
                                _currentImage.transform.DOLocalMoveY(_originalJudgeImagePosition.y, 0.1f).SetEase(Ease.OutBounce);
                            })
                            .OnKill(() => {
                                _currentImage.transform.localPosition = _originalJudgeImagePosition;
                            });

        // 크기 약간 변화 (강조 효과)
        _currentImage.transform.DOScale(1.2f, 0.05f).OnComplete(() => _currentImage.transform.DOScale(1f, 0.1f));
    }
}
