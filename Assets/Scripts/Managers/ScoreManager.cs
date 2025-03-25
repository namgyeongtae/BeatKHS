using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RankType
{
    S,
    A,
    B,
    C
}

public class ScoreManager : Manager
{
    private Dictionary<JudgeType, int> _scoreCount = new Dictionary<JudgeType, int>();

    private readonly int PERFECT_SCORE = 100;
    private readonly int GOOD_SCORE = 50;
    private readonly int BAD_SCORE = 0;
    private readonly int MISS_SCORE = 0;

    private int _currentScore = 0;
    private int _currentCombo = 0;
    private int _maxCombo = 0;
    private float _comboBonusMultiplier = 1f;

    private float _currentAccuracy = 0f;

    public int BestScore => _currentScore;
    public int BestCombo => _maxCombo;
    public float JudgeRate => _currentAccuracy;
    public int PerfectCount => _scoreCount[JudgeType.Perfect];
    public int GoodCount => _scoreCount[JudgeType.Good];
    public int BadCount => _scoreCount[JudgeType.Bad];
    public int MissCount => _scoreCount[JudgeType.Miss];
    
    public override void Init()
    {
        base.Init();

        _scoreCount.Add(JudgeType.Perfect, 0);
        _scoreCount.Add(JudgeType.Good, 0);
        _scoreCount.Add(JudgeType.Bad, 0);
        _scoreCount.Add(JudgeType.Miss, 0);

        UpdateAccuracy();
    }

    public override void Clear()
    {
        _scoreCount[JudgeType.Perfect] = 0;
        _scoreCount[JudgeType.Good] = 0;
        _scoreCount[JudgeType.Bad] = 0;
        _scoreCount[JudgeType.Miss] = 0;

        _currentScore = 0;
        _currentCombo = 0;
        _currentAccuracy = 0f;
        _maxCombo = 0;
        _comboBonusMultiplier = 1f;
    }

    public void AddScore(JudgeType judgeType)
    {
        int baseScore = 0;

        switch (judgeType)
        {
            case JudgeType.Perfect:
                baseScore = PERFECT_SCORE;
                _currentCombo++;
                break;
            case JudgeType.Good:
                baseScore = GOOD_SCORE;
                _currentCombo++;
                break;
            case JudgeType.Bad:
                baseScore = BAD_SCORE;
                break;
            case JudgeType.Miss:
                baseScore = MISS_SCORE;
                break;
        }
        
        _scoreCount[judgeType]++;

        _comboBonusMultiplier  = Mathf.Min(1.0f + (_currentCombo * 0.01f), 4.0f);
        int finalScore = Mathf.RoundToInt(baseScore * _comboBonusMultiplier);

        _currentScore += finalScore;

        if (_currentCombo > _maxCombo)
            _maxCombo = _currentCombo;

        UpdateAccuracy();

        Managers.UI.GetUI<ScoreUI>("ScoreUI")?.SetScore(_currentScore);

        Debug.Log($"판정: {judgeType}, 점수: {finalScore}, 현재 총점: {_currentScore}, 콤보: {_currentCombo}");
    }

    public void JudgeRank(UIScoreDisplay scoreDisplay)
    {
        Debug.Log("JudgeRank");
        if (_currentAccuracy >= 0.95f)
        {
            Debug.Log("Rank : S");
            scoreDisplay?.SetRank(RankType.S);
        }
        else if (_currentAccuracy >= 0.9f)
        {
            Debug.Log("Rank : A");
            scoreDisplay?.SetRank(RankType.A);
        }
        else if (_currentAccuracy >= 0.8f)
        {
            Debug.Log("Rank : B");
            scoreDisplay?.SetRank(RankType.B);
        }
        else
        {
            Debug.Log("Rank : C");
            scoreDisplay?.SetRank(RankType.C);
        }
    }

    private void UpdateAccuracy()
    {
        int totalJudgeNotes = _scoreCount[JudgeType.Perfect] + _scoreCount[JudgeType.Good] + _scoreCount[JudgeType.Bad] + _scoreCount[JudgeType.Miss];

        if (totalJudgeNotes == 0)
        {
            _currentAccuracy = 100f;
            return;
        }

        int perfectNotes = _scoreCount[JudgeType.Perfect];
        int goodNotes = _scoreCount[JudgeType.Good];

        float accuracy = (100f * perfectNotes + 50f * goodNotes) / totalJudgeNotes;
        
        _currentAccuracy = accuracy;

        Managers.UI.GetUI<ScoreUI>("ScoreUI")?.SetAccuracy(_currentAccuracy);
    }

    // 콤보 리셋
    private void ResetCombo()
    {
        _currentCombo = 0;
        _comboBonusMultiplier = 1.0f;
    }

    // 최종 점수 가져오기
    public int GetCurrentScore()
    {
        return _currentScore;
    }

    // 최고 콤보 가져오기
    public int GetMaxCombo()
    {
        return _maxCombo;
    }

    public int GetScoreCount(JudgeType judgeType)
    {
        return _scoreCount[judgeType];
    }
}
