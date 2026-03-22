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
        int index = 0;
        int clusterCount = Mathf.Max(1, chaoticPanelCount / 7);

        int[] clusterSizes = new int[clusterCount];
        int remaining = chaoticPanelCount;

        for (int c = 0; c < clusterCount; c++)
        {
            if (c == clusterCount - 1)
                clusterSizes[c] = Mathf.Max(1, remaining);
            else
            {
                clusterSizes[c] = Random.Range(2, 7);
                remaining -= clusterSizes[c];
                if (remaining <= 0)
                {
                    clusterSizes[c] += remaining;
                    remaining = 0;
                    break;
                }
            }
        }

        for (int c = 0; c < clusterCount; c++)
        {
            if (clusterSizes[c] <= 0) continue;

            float sectorAngle = (360f / clusterCount) * c + Random.Range(-12f, 12f);
            float rad = sectorAngle * Mathf.Deg2Rad;

            Vector3 clusterCenter = new Vector3(
                Mathf.Cos(rad) * sharedRadius,
                Random.Range(0.3f, chaoticHeightMax * 0.7f),
                Mathf.Sin(rad) * sharedRadius
            );

            Vector3[] hexOffsets = new Vector3[]
            {
                new Vector3(hexSize, 0, 0),
                new Vector3(-hexSize, 0, 0),
                new Vector3(hexSize * 0.5f, hexSize * 0.866f, 0),
                new Vector3(-hexSize * 0.5f, hexSize * 0.866f, 0),
                new Vector3(hexSize * 0.5f, -hexSize * 0.866f, 0),
                new Vector3(-hexSize * 0.5f, -hexSize * 0.866f, 0),
            };

            if (index < panels.Length)
            {
                SpawnPanelAtWorld(index, clusterCenter);
                index++;
            }

            for (int p = 1; p < clusterSizes[c] && p <= hexOffsets.Length; p++)
            {
                if (index >= panels.Length) break;

                Quaternion faceOut = Quaternion.LookRotation(clusterCenter.normalized, Vector3.up);
                Vector3 offset = faceOut * hexOffsets[p - 1];
                SpawnPanelAtWorld(index, clusterCenter + offset);
                index++;
            }
        }
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