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

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        _managers.Update();
    }
}
