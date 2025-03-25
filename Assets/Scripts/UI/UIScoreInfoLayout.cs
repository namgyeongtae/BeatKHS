using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIScoreInfoLayout : CanvasPanel
{
    [Bind("ScoreText")] private TextMeshProUGUI _bestScoreText;
    [Bind("BestComboText")] private TextMeshProUGUI _bestComboText;
    [Bind("JudgeRateText")] private TextMeshProUGUI _judgeRateText;

    [Bind("PerfectCount")] private TextMeshProUGUI _perfectCountText;
    [Bind("GoodCount")] private TextMeshProUGUI _goodCountText;
    [Bind("BadCount")] private TextMeshProUGUI _badCountText;
    [Bind("MissCount")] private TextMeshProUGUI _missCountText;

    public void UpdateScoreInfo(ScoreInfo InScoreInfo)
    {
        _bestScoreText.text = InScoreInfo.BestScore.ToString("D6");
        _bestComboText.text = InScoreInfo.BestCombo.ToString();
        _judgeRateText.text = InScoreInfo.JudgeRate.ToString("F2");

        _perfectCountText.text = InScoreInfo.PerfectCount.ToString();
        _goodCountText.text = InScoreInfo.GoodCount.ToString();
        _badCountText.text = InScoreInfo.BadCount.ToString();
        _missCountText.text = InScoreInfo.MissCount.ToString();
    }
}
