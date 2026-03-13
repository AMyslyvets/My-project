using UnityEngine;
using System.Collections;

public class BottleWobble : MonoBehaviour
{
    [Header("Start")]
    public bool playOnStart = true;
    public float startDelay = 0f;

    [Header("Playback")]
    public float playbackSpeed = 1f;

    [Header("Amplitude")]
    public float tiltAngleX = 10f;
    public float tiltAngleZ = 8f;
    public float twistAngleY = 2f;

    [Header("Motion")]
    public float frequency = 2.2f;
    public float damping = 1.8f;
    public float duration = 2.2f;

    [Header("Phase")]
    public float phaseOffset = 90f;

    [Header("Direction")]
    public float directionX = 1f;
    public float directionZ = 1f;
    public float directionY = 1f;

    [Header("Space")]
    public bool useLocalRotation = true;

    private Quaternion startRotation;
    private Coroutine wobbleRoutine;

    private void Awake()
    {
        startRotation = GetRotation();
    }

    private void Start()
    {
        if (playOnStart)
            PlayWobble();
    }

    [ContextMenu("Play Wobble")]
    public void PlayWobble()
    {
        if (wobbleRoutine != null)
            StopCoroutine(wobbleRoutine);

        SetRotation(startRotation);
        wobbleRoutine = StartCoroutine(WobbleRoutine());
    }

    [ContextMenu("Stop Wobble")]
    public void StopWobble()
    {
        if (wobbleRoutine != null)
        {
            StopCoroutine(wobbleRoutine);
            wobbleRoutine = null;
        }
    }

    [ContextMenu("Reset Rotation")]
    public void ResetRotation()
    {
        StopWobble();
        SetRotation(startRotation);
    }

    private IEnumerator WobbleRoutine()
    {
        float safeSpeed = Mathf.Max(0.0001f, playbackSpeed);

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay / safeSpeed);

        float time = 0f;
        float phase = phaseOffset * Mathf.Deg2Rad;

        while (time < duration)
        {
            time += Time.deltaTime * safeSpeed;

            float decay = Mathf.Exp(-damping * time);

            float x = Mathf.Sin(time * frequency * Mathf.PI * 2f) * tiltAngleX * decay * Mathf.Sign(Mathf.Approximately(directionX, 0f) ? 1f : directionX);
            float z = Mathf.Sin(time * frequency * Mathf.PI * 2f + phase) * tiltAngleZ * decay * Mathf.Sign(Mathf.Approximately(directionZ, 0f) ? 1f : directionZ);

            // небольшой скручивающий поворот, чтобы шатание выглядело живее
            float y = Mathf.Sin(time * frequency * Mathf.PI * 2f + phase * 0.5f) * twistAngleY * decay * Mathf.Sign(Mathf.Approximately(directionY, 0f) ? 1f : directionY);

            Quaternion target = startRotation * Quaternion.Euler(x, y, z);
            SetRotation(target);

            yield return null;
        }

        // плавный возврат в исходное положение
        Quaternion from = GetRotation();
        float returnTime = 0.15f;
        float t = 0f;

        while (t < returnTime)
        {
            t += Time.deltaTime * safeSpeed;
            float k = Mathf.Clamp01(t / returnTime);
            k = k * k * (3f - 2f * k);

            SetRotation(Quaternion.Slerp(from, startRotation, k));
            yield return null;
        }

        SetRotation(startRotation);
        wobbleRoutine = null;
    }

    private Quaternion GetRotation()
    {
        return useLocalRotation ? transform.localRotation : transform.rotation;
    }

    private void SetRotation(Quaternion rotation)
    {
        if (useLocalRotation)
            transform.localRotation = rotation;
        else
            transform.rotation = rotation;
    }
}