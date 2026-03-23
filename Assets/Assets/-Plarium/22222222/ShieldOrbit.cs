using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldOrbit : MonoBehaviour
{
    [Header("Mode")]
    public bool uniformShield = false;
    public bool chaoticShield = false;
    public bool fixedShield = false;

    [Header("Panels")]
    public GameObject panelPrefab;
    public int panelCount = 8;
    public float orbitRadius = 1.2f;
    public float orbitSpeed = 30f;

    [Header("Layers")]
    public int layerCount = 2;
    public float layerOffset = 0.3f;

    [Header("Chaotic Settings")]
    public int chaoticPanelCount = 24;
    public float chaoticRadiusMin = 1.5f;
    public float chaoticRadiusMax = 1.8f;
    public float chaoticHeightMax = 1.0f;
    public float hexSize = 0.32f;

    [Header("Fixed Seed Settings")]
    public int seedValue = 42;

    [Header("Color Settings")]
    public Color color1 = new Color(0f, 0.7f, 1f);
    public Color color2 = new Color(0f, 1f, 0.4f);
    public float colorSpeed = 1f;

    [Header("Pool Settings")]
    public int poolSize = 80;

    [Header("Hit Effect")]
    public float hitFlashDuration = 0.15f;
    public float hitScalePunch = 1.3f;
    public float hitPunchRadius = 0.8f;

    [Header("Break Effect")]
    public ParticleSystem breakParticlesPrefab;
    [SerializeField] private HexShieldBreak _hexShieldBreak;

    private GameObject[] panels;
    private readonly Queue<GameObject> panelPool = new Queue<GameObject>();
    private bool lastUniform = false;
    private bool lastChaotic = false;
    private bool lastFixed = false;
    private MaterialPropertyBlock propBlock;
    private Renderer[] renderers;
    private Coroutine hitRoutine;
    private bool isBroken = false;

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        PrewarmPool();
    }

    void Update()
    {
        if (uniformShield != lastUniform || chaoticShield != lastChaotic || fixedShield != lastFixed)
        {
            lastUniform = uniformShield;
            lastChaotic = chaoticShield;
            lastFixed = fixedShield;
            RebuildShield();
        }

        if (uniformShield || chaoticShield || fixedShield)
        {
            transform.Rotate(0f, orbitSpeed * Time.deltaTime, 0f);
            UpdateColor();
        }
    }

    void PrewarmPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(panelPrefab, transform);
            obj.SetActive(false);
            panelPool.Enqueue(obj);
        }
    }

    GameObject GetFromPool()
    {
        if (panelPool.Count > 0)
        {
            GameObject obj = panelPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        return Instantiate(panelPrefab, transform);
    }

    void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        obj.transform.SetParent(transform);
        obj.SetActive(false);

        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            r.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_Alpha", 1f);
            propBlock.SetFloat("_HitIntensity", 0f);
            r.SetPropertyBlock(propBlock);
        }

        panelPool.Enqueue(obj);
    }

    void ReturnAllPanels()
    {
        if (panels == null) return;

        foreach (var p in panels)
            ReturnToPool(p);

        panels = null;
        renderers = null;
    }

    void RebuildShield()
    {
        ReturnAllPanels();

        if (!uniformShield && !chaoticShield && !fixedShield)
            return;

        isBroken = false;

        if (uniformShield)
            SpawnUniform();
        else if (chaoticShield)
            SpawnChaotic(false);
        else if (fixedShield)
            SpawnChaotic(true);

        renderers = GetComponentsInChildren<Renderer>();
    }

    void UpdateColor()
    {
        if (renderers == null) return;

        float t = (Mathf.Sin(Time.time * colorSpeed) + 1f) / 2f;
        Color currentColor = Color.Lerp(color1, color2, t);

        foreach (var r in renderers)
        {
            if (r == null) continue;

            r.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color_1", currentColor);
            r.SetPropertyBlock(propBlock);
        }
    }

    void SpawnUniform()
    {
        panels = new GameObject[panelCount * layerCount];
        float angleStep = 360f / panelCount;

        for (int layer = 0; layer < layerCount; layer++)
        {
            for (int i = 0; i < panelCount; i++)
            {
                float angle = angleStep * i + (layer * (angleStep / 2f));
                float rad = angle * Mathf.Deg2Rad;

                Vector3 pos = new Vector3(
                    Mathf.Cos(rad) * orbitRadius,
                    layer * layerOffset - (layerCount * layerOffset / 2f),
                    Mathf.Sin(rad) * orbitRadius
                );

                GameObject panel = GetFromPool();
                panel.transform.SetParent(transform);
                panel.transform.localPosition = pos;
                panel.transform.localRotation =
                    Quaternion.LookRotation(pos.normalized, Vector3.up) *
                    Quaternion.Euler(90f, 0f, 0f);

                panels[layer * panelCount + i] = panel;
            }
        }
    }

    void SpawnChaotic(bool useFixedSeed)
    {
        if (useFixedSeed)
            Random.InitState(seedValue);

        panels = new GameObject[chaoticPanelCount];

        float sharedRadius = (chaoticRadiusMin + chaoticRadiusMax) / 2f;
        float minAngleStep = Mathf.Rad2Deg * (2.5f * hexSize / sharedRadius);
        int maxClusters = Mathf.Max(1, Mathf.FloorToInt(360f / minAngleStep));

        int[] clusterSizes = BuildClusterSizes(chaoticPanelCount, maxClusters);
        int clusterCount = clusterSizes.Length;
        float angleStep = 360f / clusterCount;

        Vector3[] hexOffsets = new Vector3[]
        {
            new Vector3(hexSize, 0, 0),
            new Vector3(-hexSize, 0, 0),
            new Vector3(hexSize * 0.5f,  hexSize * 0.866f, 0),
            new Vector3(-hexSize * 0.5f, hexSize * 0.866f, 0),
            new Vector3(hexSize * 0.5f, -hexSize * 0.866f, 0),
            new Vector3(-hexSize * 0.5f, -hexSize * 0.866f, 0),
            new Vector3(0,  hexSize * 1.732f, 0),
            new Vector3(0, -hexSize * 1.732f, 0),
        };

        int index = 0;

        for (int c = 0; c < clusterCount; c++)
        {
            if (clusterSizes[c] <= 0) continue;

            float sectorAngle = angleStep * c + Random.Range(-minAngleStep * 0.15f, minAngleStep * 0.15f);
            float rad = sectorAngle * Mathf.Deg2Rad;

            Vector3 clusterCenter = new Vector3(
                Mathf.Cos(rad) * sharedRadius,
                Random.Range(0.3f, chaoticHeightMax * 0.7f),
                Mathf.Sin(rad) * sharedRadius
            );

            if (index < panels.Length)
            {
                SpawnPanelAtWorld(index, clusterCenter);
                index++;
            }

            Vector3[] shuffled = ShuffleOffsets(hexOffsets);

            for (int p = 1; p < clusterSizes[c] && p <= shuffled.Length; p++)
            {
                if (index >= panels.Length) break;

                Quaternion faceOut = Quaternion.LookRotation(clusterCenter.normalized, Vector3.up);
                SpawnPanelAtWorld(index, clusterCenter + faceOut * shuffled[p - 1]);
                index++;
            }
        }
    }

    void SpawnPanelAtWorld(int index, Vector3 localPos)
    {
        if (index >= panels.Length) return;

        GameObject panel = GetFromPool();
        panel.transform.SetParent(transform);
        panel.transform.localPosition = localPos;
        panel.transform.localRotation =
            Quaternion.LookRotation(localPos.normalized, Vector3.up) *
            Quaternion.Euler(90f, 0f, 0f);
        panel.transform.localScale = Vector3.one * Random.Range(0.9f, 1.1f) * 0.3f;

        panels[index] = panel;
    }

    public void OnHit(Vector3 hitPoint)
    {
        if (isBroken || panels == null) return;

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        hitRoutine = StartCoroutine(HitRoutine(hitPoint));
    }

    IEnumerator HitRoutine(Vector3 hitPoint)
    {
        Vector3[] origScales = new Vector3[panels.Length];
        bool[] inRange = new bool[panels.Length];

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] == null) continue;

            origScales[i] = panels[i].transform.localScale;
            inRange[i] = Vector3.Distance(panels[i].transform.position, hitPoint) < hitPunchRadius;

            if (inRange[i])
                panels[i].transform.localScale = origScales[i] * hitScalePunch;
        }

        if (renderers != null)
        {
            foreach (var r in renderers)
            {
                if (r == null) continue;

                r.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_HitIntensity", 1f);
                r.SetPropertyBlock(propBlock);
            }
        }

        float elapsed = 0f;

        while (elapsed < hitFlashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hitFlashDuration;

            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i] == null || !inRange[i]) continue;
                panels[i].transform.localScale = Vector3.Lerp(origScales[i] * hitScalePunch, origScales[i], t);
            }

            if (renderers != null)
            {
                foreach (var r in renderers)
                {
                    if (r == null) continue;

                    r.GetPropertyBlock(propBlock);
                    propBlock.SetFloat("_HitIntensity", Mathf.Lerp(1f, 0f, t));
                    r.SetPropertyBlock(propBlock);
                }
            }

            yield return null;
        }

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null && inRange[i])
                panels[i].transform.localScale = origScales[i];
        }
    }

    public void OnBreak()
    {
        Debug.Log("OnBreak called");

        if (isBroken) return;
        isBroken = true;

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        if (_hexShieldBreak != null)
            _hexShieldBreak.PlayBreak();

        if (breakParticlesPrefab != null)
        {
            ParticleSystem spawnedBreak = Instantiate(
                breakParticlesPrefab,
                transform.position + Vector3.up * 1.0f,
                Quaternion.identity
            );

            spawnedBreak.Play(true);
            Destroy(spawnedBreak.gameObject, 3f);

            Debug.Log("Break particles spawned");
        }
        else
        {
            Debug.Log("breakParticlesPrefab is NULL");
        }

        ReturnAllPanels();

        uniformShield = false;
        chaoticShield = false;
        fixedShield = false;

        lastUniform = false;
        lastChaotic = false;
        lastFixed = false;
    }

    int[] BuildClusterSizes(int total, int maxClusters)
    {
        var sizes = new List<int>();
        int remaining = total;

        while (remaining > 0 && sizes.Count < maxClusters)
        {
            int take = (sizes.Count == maxClusters - 1)
                ? Mathf.Min(remaining, 8)
                : Random.Range(2, Mathf.Min(9, remaining + 1));

            sizes.Add(take);
            remaining -= take;
        }

        return sizes.ToArray();
    }

    Vector3[] ShuffleOffsets(Vector3[] src)
    {
        Vector3[] arr = (Vector3[])src.Clone();

        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector3 tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }

        return arr;
    }
}