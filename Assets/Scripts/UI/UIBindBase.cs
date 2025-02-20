using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIBindBase : MonoBehaviour
{
    private bool _isBindingDone = false;
    protected bool IsBindingDone => _isBindingDone;

    protected virtual void Awake()
    {
        InstallBindings();
    }

    public void InstallBindings()
    {
        if(_isBindingDone) return;
        BindAttribute.InstallBindings(this);
        _isBindingDone = true;
    }

    protected virtual void Initialize() { }

    protected static void BindEvent(Button InComponent, UnityAction InAction)
    {
        InComponent?.onClick.AddListener(InAction);
    }
}