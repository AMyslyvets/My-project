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

    private GameObject[] panels;
    private bool lastUniform = false;
    private bool lastChaotic = false;
    private bool lastFixed = false;
    private MaterialPropertyBlock propBlock;
    private Renderer[] renderers;

    void Start()
    {
        propBlock = new MaterialPropertyBlock();
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

    void RebuildShield()
    {
        if (panels != null)
        {
            foreach (var p in panels)
                if (p != null) Destroy(p);
        }
        if (!uniformShield && !chaoticShield && !fixedShield) return;
        if (uniformShield) SpawnUniform();
        else if (chaoticShield) SpawnChaotic(false);
        else if (fixedShield) SpawnChaotic(true);
        renderers = GetComponentsInChildren<Renderer>();
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
                GameObject panel = Instantiate(panelPrefab, transform);
                panel.transform.localPosition = pos;
                panel.transform.localRotation = Quaternion.LookRotation(pos.normalized, Vector3.up)
                    * Quaternion.Euler(90f, 0f, 0f);
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
            new Vector3(hexSize,          0,                 0),
            new Vector3(-hexSize,         0,                 0),
            new Vector3(hexSize * 0.5f,   hexSize * 0.866f,  0),
            new Vector3(-hexSize * 0.5f,  hexSize * 0.866f,  0),
            new Vector3(hexSize * 0.5f,   -hexSize * 0.866f, 0),
            new Vector3(-hexSize * 0.5f,  -hexSize * 0.866f, 0),
            new Vector3(0,                hexSize * 1.732f,   0),
            new Vector3(0,               -hexSize * 1.732f,   0),
        };

        int index = 0;
        for (int c = 0; c < clusterCount; c++)
        {
            if (clusterSizes[c] <= 0) continue;

            float sectorAngle = angleStep * c
                + Random.Range(-minAngleStep * 0.15f, minAngleStep * 0.15f);
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
                Vector3 offset = faceOut * shuffled[p - 1];
                SpawnPanelAtWorld(index, clusterCenter + offset);
                index++;
            }
        }
    }

    int[] BuildClusterSizes(int total, int maxClusters)
    {
        var sizes = new System.Collections.Generic.List<int>();
        int remaining = total;
        while (remaining > 0 && sizes.Count < maxClusters)
        {
            int take;
            if (sizes.Count == maxClusters - 1)
                take = Mathf.Min(remaining, 8);
            else
                take = Random.Range(2, Mathf.Min(9, remaining + 1));
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

    void SpawnPanelAtWorld(int index, Vector3 worldPos)
    {
        if (index >= panels.Length) return;
        GameObject panel = Instantiate(panelPrefab, transform);
        panel.transform.localPosition = worldPos;
        panel.transform.localRotation = Quaternion.LookRotation(worldPos.normalized, Vector3.up)
            * Quaternion.Euler(90f, 0f, 0f);
        float scale = Random.Range(0.9f, 1.1f);
        panel.transform.localScale = Vector3.one * scale * 0.3f;
        panels[index] = panel;
    }
}