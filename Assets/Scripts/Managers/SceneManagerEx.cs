using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class SceneManagerEx
{
    public async UniTaskVoid LoadScene(string sceneName)
    {
        var sceneAnimator = GameObject.Find("SceneAnimator").GetComponent<Animator>();
        sceneAnimator.SetTrigger("FadeIn");
        
        // 2초 대기
        await UniTask.Delay(2000);
        
        // 씬 전환
        await SceneManager.LoadSceneAsync(sceneName).ToUniTask();
    }
}
