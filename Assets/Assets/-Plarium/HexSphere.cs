using UnityEngine;
using UnityEngine.Rendering;

public class HexSphere : MonoBehaviour
{
    [Header("Hex Sphere")]
    public bool hexSphere = false;
    public GameObject hexPrefab;
    public float radius = 1.3f;
    public float hexSize = 0.22f;
    public bool autoRadius = true;
    public int columns = 24;
    public int rows = 4;

    [Header("Shape")]
    public float squish = 0.05f;

    [Header("Spacing Tuning")]
    public float verticalSpacingMult = 0.75f;
    public float horizontalSpacingMult = 1.05f;

    [Header("Rotation")]
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);
    public bool randomSpin = false;

    private GameObject[] hexes;
    private bool lastState = false;
    private MeshFilter combinedMeshFilter;
    private MeshRenderer combinedMeshRenderer;
    private bool isBuilt = false; // чи вже побудовано mesh

    void Start()
    {
        // будуємо один раз при старті сцени
        BuildHexSphere();
        SetVisible(hexSphere);
    }

    void Update()
    {
        if (hexSphere != lastState)
        {
            lastState = hexSphere;
            SetVisible(hexSphere); // просто показуємо/ховаємо
        }
    }

    void SetVisible(bool visible)
    {
        if (combinedMeshRenderer != null)
            combinedMeshRenderer.enabled = visible;
    }

    void BuildHexSphere()
    {
        if (isBuilt) return; // захист від повторного білду

        MeshFilter mf = hexPrefab.GetComponent<MeshFilter>();
        float hexMeshWidth  = mf != null ? mf.sharedMesh.bounds.size.x : 1f;
        float hexMeshHeight = mf != null ? mf.sharedMesh.bounds.size.z : 1f;

        float realWidth  = hexSize * hexMeshWidth;
        float realHeight = hexSize * hexMeshHeight;

        float actualRadius = autoRadius
            ? (realWidth * columns) / (2f * Mathf.PI) * horizontalSpacingMult
            : radius;

        hexes = new GameObject[rows * columns];
        int index = 0;

        float angleStep = 360f / columns;

        float rowSpacing  = realHeight * verticalSpacingMult;
        float totalHeight = rowSpacing * (rows - 1);

        for (int r = 0; r < rows; r++)
        {
            float t = rows > 1 ? (float)r / (rows - 1) : 0.5f;
            float tCentered = t * 2f - 1f;

            float squeeze = 1f - squish * (tCentered * tCentered);
            float rowRadius = actualRadius * squeeze;
            float y = tCentered * totalHeight * 0.5f;

            float offset = (r % 2 == 0) ? 0f : (angleStep * 0.5f);

            for (int c = 0; c < columns; c++)
            {
                float angle = angleStep * c + offset;
                float rad = angle * Mathf.Deg2Rad;

                Vector3 pos = new Vector3(
                    Mathf.Cos(rad) * rowRadius,
                    y,
                    Mathf.Sin(rad) * rowRadius
                );

                Quaternion baseRot = Quaternion.LookRotation(pos.normalized, Vector3.up)
                                     * Quaternion.Euler(rotationOffset);

                Quaternion finalRot = baseRot;
                if (randomSpin)
                {
                    int steps = Random.Range(0, 6);
                    float randomAngle = steps * 60f;
                    Quaternion spinRot = Quaternion.AngleAxis(randomAngle, pos.normalized);
                    finalRot = spinRot * baseRot;
                }

                GameObject hex = Instantiate(hexPrefab, transform);
                hex.transform.localPosition = pos;
                hex.transform.localRotation = finalRot;
                hex.transform.localScale = Vector3.one * hexSize;

                hexes[index++] = hex;
            }
        }

        CombineHexMeshes(hexes);
        isBuilt = true;
    }

    void CombineHexMeshes(GameObject[] sourceHexes)
    {
        CombineInstance[] combine = new CombineInstance[sourceHexes.Length];

        for (int i = 0; i < sourceHexes.Length; i++)
        {
            MeshFilter f = sourceHexes[i].GetComponentInChildren<MeshFilter>();
            combine[i].mesh = f.sharedMesh;
            combine[i].transform = transform.worldToLocalMatrix
                                   * f.transform.localToWorldMatrix;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine, true, true);

        combinedMeshFilter = gameObject.AddComponent<MeshFilter>();
        combinedMeshFilter.mesh = mesh;

        combinedMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        combinedMeshRenderer.material = hexPrefab.GetComponent<MeshRenderer>().sharedMaterial;
        combinedMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;

        // знищуємо тимчасові об'єкти — вони більше не потрібні
        foreach (var h in sourceHexes)
            if (h != null) Destroy(h);
        hexes = null;
    }
}