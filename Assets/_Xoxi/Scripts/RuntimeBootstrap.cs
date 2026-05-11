using DG.Tweening;
using UnityEngine;

public static class RuntimeBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ConfigureRuntime()
    {
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
        DOTween.SetTweensCapacity(200, 50);
        DOTween.useSmoothDeltaTime = true;

#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        Debug.unityLogger.logEnabled = false;
#endif
    }
}
