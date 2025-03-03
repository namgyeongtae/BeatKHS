using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers
{
    private static ResourceManager _resource = new ResourceManager();
    private static UIManager _ui = new UIManager();
    private static DataManager _data = new DataManager();
    private static SceneManagerEx _scene = new SceneManagerEx();

    public static ResourceManager Resource => _resource;
    public static UIManager UI => _ui;
    public static DataManager Data => _data;
    public static SceneManagerEx Scene => _scene;

    public void Init()
    {
        _ui.Init();
        _data.Init();
    }

    public void Update()
    {

    }
}
