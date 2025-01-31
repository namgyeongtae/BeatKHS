using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyInputManager : MonoBehaviour
{
    public static KeyInputManager Instance;

    public List<Action> OnKeyInputDown = new List<Action>();

    public List<bool> IsKeyInputDown = new List<bool>();

    public List<Action> OnKeyInputUp = new List<Action>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        OnKeyInputDown.Add(null);
        OnKeyInputDown.Add(null);
        OnKeyInputDown.Add(null);
        OnKeyInputDown.Add(null);

        OnKeyInputUp.Add(null);
        OnKeyInputUp.Add(null);
        OnKeyInputUp.Add(null);
        OnKeyInputUp.Add(null);

        for (int i = 0; i < OnKeyInputDown.Count; i++)
        {
            IsKeyInputDown.Add(false);
        }

        for (int i = 0; i < OnKeyInputDown.Count; i++)
        {
            int index = i;

            OnKeyInputDown[index] += () => { IsKeyInputDown[index] = true; };
            OnKeyInputUp[index] += () => { IsKeyInputDown[index] = false; };
        }
    }

    void Update()
    {
        KeyInputDown();
        KeyInputUp();
    }

    void KeyInputDown()
    {
        if (Input.GetKeyDown(KeyCode.D))
            OnKeyInputDown[0]?.Invoke();
        if (Input.GetKeyDown(KeyCode.F))
            OnKeyInputDown[1]?.Invoke();
        if (Input.GetKeyDown(KeyCode.J))
            OnKeyInputDown[2]?.Invoke();
        if (Input.GetKeyDown(KeyCode.K))
            OnKeyInputDown[3]?.Invoke();
    }

    void KeyInputUp()
    {
        if (Input.GetKeyUp(KeyCode.D))
            OnKeyInputUp[0]?.Invoke();
        if (Input.GetKeyUp(KeyCode.F))
            OnKeyInputUp[1]?.Invoke();
        if (Input.GetKeyUp(KeyCode.J))
            OnKeyInputUp[2]?.Invoke();
        if (Input.GetKeyUp(KeyCode.K))
            OnKeyInputUp[3]?.Invoke();
    }
}
