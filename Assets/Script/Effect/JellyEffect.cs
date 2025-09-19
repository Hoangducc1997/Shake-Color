using UnityEngine;
using System.Collections;

public class JellyEffect : MonoBehaviour
{
    public float duration = 0.4f;      // Thời gian hiệu ứng
    public float intensity = 0.25f;    // Mức độ rung

    private Vector3 originalScale;
    private Coroutine jellyRoutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    /// <summary>
    /// Gọi để chạy hiệu ứng "rau câu"
    /// </summary>
    public void PlayJelly()
    {
        if (jellyRoutine != null) StopCoroutine(jellyRoutine);
        jellyRoutine = StartCoroutine(JellyRoutine());
    }

    private IEnumerator JellyRoutine()
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            // Tính tỉ lệ theo sin -> phóng to thu nhỏ
            float progress = time / duration;
            float damper = Mathf.Exp(-3 * progress); // giảm dần
            float scale = 1 + Mathf.Sin(progress * Mathf.PI * 4) * intensity * damper;
            transform.localScale = originalScale * scale;
            yield return null;
        }
        transform.localScale = originalScale;
        jellyRoutine = null;
    }
}
