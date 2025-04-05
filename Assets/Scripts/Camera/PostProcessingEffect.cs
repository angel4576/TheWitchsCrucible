using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PostProcessingEffect : MonoBehaviour
{
    public Volume volume;            
    private ColorAdjustments colorAdj;  // to access Post Exposure

    public float targetExposure = -2f; 
    public float effectDuration = 0.6f;

    
    void Start()
    {
        // Get ColorAdjustments Component
        // 注意：URP 下要用 TryGet<T>(out T) 这种泛型方法
        if (volume.profile.TryGet<ColorAdjustments>(out colorAdj) == false)
        {
            Debug.LogError("Volume Profile does not have Color Adjustments Override！");
        }
    }

    public void PlayHitEffect()
    {
        StartCoroutine(HitEffectRoutine());
    }

    IEnumerator HitEffectRoutine()
    {
        float originalExposure = colorAdj.postExposure.value;
        float halfTime = effectDuration / 2f;
        float timer = 0f;

        // Become darker
        while (timer < halfTime)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / halfTime;
            colorAdj.postExposure.Override(Mathf.Lerp(originalExposure, targetExposure, t));
            yield return null;
        }
        colorAdj.postExposure.Override(targetExposure);
        
        yield return new WaitForSecondsRealtime(0.1f);

        // Recover
        timer = 0f;
        while (timer < halfTime)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / halfTime;
            colorAdj.postExposure.Override(Mathf.Lerp(targetExposure, originalExposure, t));
            yield return null;
        }
        colorAdj.postExposure.Override(originalExposure);
    }
}