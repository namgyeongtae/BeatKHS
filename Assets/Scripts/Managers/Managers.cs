using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers
{
    private static ResourceManager _resource = new ResourceManager();
    private static UIManager _ui = new UIManager();

    public static ResourceManager Resource => _resource;
    public static UIManager UI => _ui;

    public void Init()
    {
        _ui.Init();
    }

    public void Update()
    {

    }
}
