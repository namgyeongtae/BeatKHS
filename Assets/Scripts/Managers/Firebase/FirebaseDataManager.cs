using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System;
using System.Threading.Tasks;
using static UserData;
using System.Linq;
using System.Web;

public class FirebaseDataManager : Manager
{
    public string DB_URL = "https://bestkhs-111cf-default-rtdb.firebaseio.com/";

    private DatabaseReference _databaseReference;

    public UserData UserData;

    public override async void Init()
    {
        try 
        {
            Debug.Log("Firebase 초기화 시작...");
            
            // 1. 종속성 체크
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            Debug.Log($"Firebase 종속성 상태: {dependencyStatus}");
            
            if (dependencyStatus == DependencyStatus.Available)
            {
                // 2. 가장 기본적인 테스트 데이터 쓰기 시도
                try
                {
                    _databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                    
                    // 최상위 레벨에 테스트
                    await _databaseReference.Child("test").SetValueAsync("test_value");
                    Debug.Log("테스트 데이터 쓰기 성공!");
                    
                    // 데이터 읽기 테스트
                    var snapshot = await _databaseReference.Child("test").GetValueAsync();
                    Debug.Log($"테스트 데이터 읽기 결과: {snapshot.Value}");
                }
                catch (Exception dbEx)
                {
                    Debug.LogError($"데이터베이스 테스트 실패: {dbEx.Message}");
                    Debug.LogError($"상세 에러: {dbEx.StackTrace}");
                }
            }
            else
            {
                Debug.LogError($"Firebase 종속성 문제 발생: {dependencyStatus}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"전체 초기화 실패: {ex.Message}");
            Debug.LogError($"스택 트레이스: {ex.StackTrace}");
        }
    }

    public async Task<Dictionary<string, ScoreInfo>> ReadScoreData(string userID)
    {
        try
        {
            string safeKey = ConvertToSafeKey(userID);
            var snapshot = await _databaseReference.Child("users").Child(safeKey).Child("ScoreInfos").GetValueAsync();

            if (!snapshot.Exists)
            {
                Debug.Log($"사용자 {userID}의 점수 데이터가 없습니다.");
                return new Dictionary<string, ScoreInfo>();
            }

            Dictionary<string, ScoreInfo> result = new Dictionary<string, ScoreInfo>();
            
            // 곡 이름별로 데이터 파싱
            var songsData = snapshot.Value as Dictionary<string, object>;
            if (songsData != null)
            {
                foreach (var songPair in songsData)
                {
                    string songName = songPair.Key;
                    var songData = songPair.Value as Dictionary<string, object>;
                    
                    if (songData != null)
                    {
                        ScoreInfo scoreInfo = new ScoreInfo
                        {
                            BadCount = Convert.ToInt32(songData["BadCount"]),
                            BestCombo = Convert.ToInt32(songData["BestCombo"]),
                            BestScore = Convert.ToInt32(songData["BestScore"]),
                            GoodCount = Convert.ToInt32(songData["GoodCount"]),
                            JudgeRate = (float)Convert.ToDouble(songData["JudgeRate"]),
                            MissCount = Convert.ToInt32(songData["MissCount"]),
                            PerfectCount = Convert.ToInt32(songData["PerfectCount"])
                        };
                        
                        result.Add(songName, scoreInfo);
                    }
                }
            }

            Debug.Log($"읽어온 점수 데이터 개수: {result.Count}");
            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"점수 데이터 읽기 실패: {ex.Message}");
            Debug.LogError($"스택 트레이스: {ex.StackTrace}");
            return new Dictionary<string, ScoreInfo>();
        }
    }

    public async Task<bool> CreateUserWithJson(string userID, UserData userData)
    {
        try
        {
            string safeKey = ConvertToSafeKey(userID);
            Debug.Log($"원본 이메일: {userID}");
            Debug.Log($"변환된 키: {safeKey}");

            var userRef = _databaseReference.Child("users").Child(safeKey);
            
            var result = await userRef.RunTransaction(mutableData =>
            {
                if (mutableData.Value == null)
                {
                    // 새로운 사용자 데이터 생성
                    Debug.Log("새로운 사용자 생성");
                    mutableData.Value = new Dictionary<string, object>
                    {
                        ["email"] = userID,
                        ["UserID"] = userData.UserID,
                        ["UserName"] = userData.UserName
                    };
                }
                /* else
                {
                    // 기존 데이터 업데이트
                    Debug.Log("기존 사용자 정보 업데이트");
                    var existingData = mutableData.Value as Dictionary<string, object>;
                    if (existingData != null)
                    {
                        existingData["email"] = userID;
                        existingData["UserID"] = userData.UserID;
                        existingData["UserName"] = userData.UserName;
                        mutableData.Value = existingData;
                    }
                } */
                return TransactionResult.Success(mutableData);
            });

            Debug.Log("사용자 데이터 처리 성공");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"데이터 저장 실패: {ex.Message}");
            Debug.LogError($"스택 트레이스: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<bool> SaveScoreData(string userID, string scoreKey, ScoreInfo scoreInfo)
    {
        try
        {
            string safeKey = ConvertToSafeKey(userID);

            var scoreData = new Dictionary<string, object>
            {
                { "BestScore", scoreInfo.BestScore },
                { "BestCombo", scoreInfo.BestCombo },
                { "JudgeRate", scoreInfo.JudgeRate },
                { "PerfectCount", scoreInfo.PerfectCount },
                { "GoodCount", scoreInfo.GoodCount },
                { "BadCount", scoreInfo.BadCount },
                { "MissCount", scoreInfo.MissCount }
                // ScoreInfo의 다른 필드들도 필요하다면 여기에 추가
            };

            await _databaseReference.Child("users").Child(safeKey).Child("ScoreInfos").Child(scoreKey).SetValueAsync(scoreData);

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"점수 저장 실패: {ex.Message}");
            return false;
        }
    }

    public string ConvertToSafeKey(string userID)
    {
        return userID.Replace(".", "_dot_")
                    .Replace("@", "_at_");
    }

    public async Task<bool> TestReadData(string userID)
    {
        try
        {
            string safeKey = ConvertToSafeKey(userID);
            
            // 전체 경로 로깅
            string path = $"users/{safeKey}/ScoreInfos";
            Debug.Log($"읽기 시도할 경로: {path}");
            
            // 단계별로 확인
            var userRef = _databaseReference.Child("users");
            var userSnapshot = await userRef.GetValueAsync();
            Debug.Log($"users 노드 존재: {userSnapshot.Exists}");
            
            var specificUserRef = userRef.Child(safeKey);
            var specificUserSnapshot = await specificUserRef.GetValueAsync();
            Debug.Log($"{safeKey} 노드 존재: {specificUserSnapshot.Exists}");
            
            var scoreInfosRef = specificUserRef.Child("ScoreInfos");
            var scoreInfosSnapshot = await scoreInfosRef.GetValueAsync();
            Debug.Log($"ScoreInfos 노드 존재: {scoreInfosSnapshot.Exists}");
            
            // 실제 데이터 확인 (변경하지는 않음)
            if (scoreInfosSnapshot.Exists)
            {
                Debug.Log($"데이터 확인: {scoreInfosSnapshot.GetRawJsonValue()}");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"테스트 읽기 실패: {ex.Message}");
            Debug.LogError($"스택 트레이스: {ex.StackTrace}");
            return false;
        }
    }
}
