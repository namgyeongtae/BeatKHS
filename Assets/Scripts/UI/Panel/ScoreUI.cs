using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreUI : CanvasPanel
{
    [Bind("ScoreText")] private TextMeshProUGUI _scoreText;
    [Bind("Accuracy")] private TextMeshProUGUI _accuracyText;

    protected override void Initialize()
    {
        base.Initialize();
        
        _scoreText.text = "000000";
        _accuracyText.text = "100.00";
    }

    public void SetScore(int score)
    {
        _scoreText.text = score.ToString("D6");
    }

    public void SetAccuracy(float accuracy)
    {
        _accuracyText.text = $"{accuracy:F2}%";
    }
}
