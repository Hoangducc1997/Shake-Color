using UnityEngine;
using System.Collections;

public class ExplosionEffect : MonoBehaviour
{
    public float scaleUpFactor = 2f;
    public float scaleUpTime = 0.2f;
    public float fadeTime = 0.1f;
    public bool destroyOnComplete = true;

    public void PlayExplosion()
    {
        StartCoroutine(ExplosionCoroutine());
    }

    private IEnumerator ExplosionCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * scaleUpFactor;

        // Scale to lên
        float t = 0;
        while (t < scaleUpTime)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t / scaleUpTime);
            yield return null;
        }

        // Fade out
        t = 0;
        CanvasGroup cg = gameObject.GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }

        if (destroyOnComplete)
        {
            Destroy(gameObject);
        }
        else
        {
            // Reset về trạng thái ban đầu
            transform.localScale = originalScale;
            cg.alpha = 1f;
            this.enabled = false; // Tắt component
        }
    }

    // Tự động dừng coroutine khi object bị destroy
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}