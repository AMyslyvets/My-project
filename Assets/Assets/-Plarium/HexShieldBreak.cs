using System.Collections;
using UnityEngine;

public class HexShieldBreak : MonoBehaviour
{
    [SerializeField] private Transform shieldTransform;

    [Header("Timing")]
    [SerializeField] private float breakDuration = 1f;
    [SerializeField] private float appearDuration = 0.5f;

    [Header("Scale")]
    [SerializeField] private float normalScale = 0.4f;
    [SerializeField] private float breakScale = 1.4f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (shieldTransform == null)
            shieldTransform = transform;

        shieldTransform.localScale = Vector3.one * normalScale;
    }

    public void PlayBreak()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(BreakRoutine());
    }

    public void PlayAppear()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(AppearRoutine());
    }

    private IEnumerator BreakRoutine()
    {
        float time = 0f;

        Vector3 from = Vector3.one * normalScale;
        Vector3 to = Vector3.one * breakScale;

        while (time < breakDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / breakDuration);

            shieldTransform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        shieldTransform.localScale = Vector3.zero;
        currentRoutine = null;
    }

    private IEnumerator AppearRoutine()
    {
        float time = 0f;

        Vector3 from = Vector3.zero;
        Vector3 to = Vector3.one * normalScale;

        shieldTransform.localScale = from;

        while (time < appearDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / appearDuration);

            shieldTransform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        shieldTransform.localScale = to;
        currentRoutine = null;
    }
}