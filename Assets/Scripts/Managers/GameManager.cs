using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Managers _managers = new Managers();

    private void Awake()
    {
        Instance = this;
        
        _managers.Init();
    }

    private void Update()
    {
        _managers.Update();
    }
}
