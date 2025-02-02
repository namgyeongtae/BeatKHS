using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Manager
{
    public RectTransform BaseCanvas => CanvasManager.Instance.MainRect;

    public override void Init()
    {
        base.Init();
    }

    public T GetUI<T>(string name) where T : CanvasPanel
    {
        return CanvasManager.Instance.GetPanel<T>(name);
    }

    public CanvasPanel GetUI(string name)
    {
        return CanvasManager.Instance.GetPanel(name);
    }

    public T AddPanel<T>(string name, object param = null) where T : CanvasPanel
    {
        if (name == null) name = typeof(T).Name;

        T panel = CanvasManager.Instance?.AddPanel<T>(name, param);
        if (panel == null)
        {
            Debug.LogError($"Failed to add panel : {name}");
            return null;
        }

        return panel;
    }

    public T AddPanel<T>(object param = null) where T : CanvasPanel
    {
        string name = typeof(T).Name;
        T panel = CanvasManager.Instance?.AddPanel<T>(name, param);
        if (panel == null)
        {
            Debug.LogError($"Failed to add panel : {name}");
            return null;
        }

        return panel;
    }

    public void RemovePanel(CanvasPanel obj)
    {
        CanvasManager.Instance.RemovePanel(obj);
    }

    public void RemovePanel(string panelObj)
    {
        CanvasManager.Instance.RemovePanel(panelObj);
    }

    public void RemoveAllPanel()
    {
        CanvasManager.Instance.RemoveAllPanel();
    }
    
    public void ShowResultUI(JudgeType judgeType)
    {
        
    }

    public GameObject ShowHitEffect(Transform targetTransform)
    {
        var go = Managers.Resource.Instantiate("Hit");
        go.transform.SetParent(BaseCanvas.transform);
        go.transform.position = targetTransform.position;

        // Animator 컴포넌트 가져오기
        Animator animator = go.GetComponent<Animator>();
        if (animator != null)
        {
            // 현재 재생 중인 애니메이션 클립의 길이 가져오기
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                float clipLength = clipInfo[0].clip.length;
                GameObject.Destroy(go, clipLength);
            }
        }

        return go;
    }

    public GameObject ShowLongHitEffect(Transform targetTransform)
    {
        var go = Managers.Resource.Instantiate("LongHit");
        go.transform.SetParent(BaseCanvas.transform);
        go.transform.position = targetTransform.position;

        return go;
    }
}
