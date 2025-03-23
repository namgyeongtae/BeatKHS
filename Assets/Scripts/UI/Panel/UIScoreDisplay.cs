using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class UIScoreDisplay : CanvasPanel
{
    [Bind("PerfectCount")] private TextMeshProUGUI _perfectCount;
    [Bind("GoodCount")] private TextMeshProUGUI _goodCount;
    [Bind("BadCount")] private TextMeshProUGUI _badCount;
    [Bind("MissCount")] private TextMeshProUGUI _missCount;
    [Bind("FinalScore")] private TextMeshProUGUI _finalScore;

    [Bind("RankImage")] private Image _rankImage;
    [Bind("Block")] private Image _block;

    [SerializeField] private Sprite[] _rankSprites;

    protected override void Initialize()
    {
        base.Initialize();

        ScoreInfo();

        _block.DOFade(0f, 1f).SetDelay(1f);
    }

    private void ScoreInfo()
    {
        _perfectCount.text = Managers.Score.GetScoreCount(JudgeType.Perfect).ToString();
        _goodCount.text = Managers.Score.GetScoreCount(JudgeType.Good).ToString();
        _badCount.text = Managers.Score.GetScoreCount(JudgeType.Bad).ToString();
        _missCount.text = Managers.Score.GetScoreCount(JudgeType.Miss).ToString();

        _finalScore.text = Managers.Score.GetCurrentScore().ToString();

        Managers.Score.JudgeRank(this);
    }

    public void SetRank(RankType rankType)
    {
        Debug.Log("SetRank : " + rankType);
        _rankImage.sprite = _rankSprites[(int)rankType];
    }
}
