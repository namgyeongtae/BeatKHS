using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreUI : CanvasPanel
{
    [Bind("ScoreText")] private TextMeshProUGUI _scoreText;

    protected override void Initialize()
    {
        base.Initialize();
        
        _scoreText.text = "000000";
    }

    public void SetScore(int score)
    {
        _scoreText.text = score.ToString("D6");
    }
}
