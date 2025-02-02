using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBind : UIBindBase
{
    public Action OnClose_Event { get; set; } = null;
    
    public virtual void Open()
    {

    }

    public virtual void Close()
    {
        CanvasManager.Instance.ReleaseUI(this);

        OnClose_Event?.Invoke();
        OnClose_Event = null;
    }
}
