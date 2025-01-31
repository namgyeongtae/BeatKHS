using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Manager
{
    public Canvas BaseCanvas;

    public override void Init()
    {
        base.Init();

        BaseCanvas = GameObject.Find("BaseCanvas").GetComponent<Canvas>();
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
