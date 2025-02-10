using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
public class CanvasPanel : UIBind
{
    public int CanvasPanelDepth { get; private set; } = 0;
    private Canvas _canvasComponent;

    protected virtual void Initialize() { }

    protected void Start()
    {
        Initialize();
    }

    protected void SetCanvasComponent()
    {
        if (_canvasComponent == null)
        {
            _canvasComponent = GetComponent<Canvas>();
        }
    }

    public virtual void SetPanelInfo(object InInfo) { }

    public void SetPanelDepth(int InDepth)
    {
        SetCanvasComponent();
        if (_canvasComponent == null)
        {
            Debug.LogError("CanvasComponent is null");
            return;
        }

        _canvasComponent.overrideSorting = true;
        _canvasComponent.sortingOrder = InDepth;
        CanvasPanelDepth = InDepth;
    }

    public virtual void CallAfterSetting() { }
}

