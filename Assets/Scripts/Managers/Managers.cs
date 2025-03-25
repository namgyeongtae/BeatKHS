using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers
{
    private static ResourceManager _resource = new ResourceManager();
    private static UIManager _ui = new UIManager();
    private static DataManager _data = new DataManager();
    private static SceneManagerEx _scene = new SceneManagerEx();
    private static AudioManager _audio = new AudioManager();
    private static ScoreManager _score = new ScoreManager();
    private static FirebaseAuthManager _auth = new FirebaseAuthManager();
    private static FirebaseDataManager _firebaseData = new FirebaseDataManager();

    public static ResourceManager Resource => _resource;
    public static UIManager UI => _ui;
    public static DataManager Data => _data;
    public static SceneManagerEx Scene => _scene;
    public static AudioManager Audio => _audio;
    public static ScoreManager Score => _score;
    public static FirebaseAuthManager Auth => _auth;
    public static FirebaseDataManager FirebaseData => _firebaseData;

    public void Init()
    {
        _auth.Init();
        _firebaseData.Init();
        _ui.Init();
        _data.Init();
        _audio.Init();
        _score.Init();
    }

    public void Update()
    {
        _audio.Update();
    }

    public void Clear()
    {
        _audio.Clear();
    }
}
