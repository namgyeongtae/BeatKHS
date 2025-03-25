using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreInfo
{
    public int BestScore { get; set; }
    public int BestCombo { get; set; }
    public float JudgeRate { get; set; }
    public int PerfectCount { get; set; }
    public int GoodCount { get; set; }
    public int BadCount { get; set; }
    public int MissCount { get; set; }

    public ScoreInfo()
    {
        
    }

    public ScoreInfo(int bestScore, int bestCombo, float judgeRate, int perfectCount, int goodCount, int badCount, int missCount)
    {
        BestScore = bestScore;
        BestCombo = bestCombo;
        JudgeRate = judgeRate;
        PerfectCount = perfectCount;
        GoodCount = goodCount;
        BadCount = badCount;
        MissCount = missCount;
    }
}
