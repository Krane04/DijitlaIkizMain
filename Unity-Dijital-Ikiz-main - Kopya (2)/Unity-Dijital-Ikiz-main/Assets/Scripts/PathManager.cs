using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Görev hedefine giden yolu çizen modern navigasyon çizgisi.
/// İki katmanlı (glow + core), animasyonlu, gradient renk sistemi.
/// Material gerektirmez — Unity'nin built-in shader'ı kullanılır.
/// </summary>
public class PathManager : MonoBehaviour
{
    public static PathManager Instance;

    [Header("Hedef")]
    public Transform target;

    [Header("Çizgi Görünümü")]
    [Tooltip("Ana çizgi rengi (başlangıç — oyuncu tarafı)")]
    public Color colorStart = new Color(0f, 1f, 0.9f, 1f);      // Cyan
    [Tooltip("Ana çizgi rengi (bitiş — hedef tarafı)")]
    public Color colorEnd   = new Color(1f, 1f, 1f, 0.6f);       // Beyaz

    [Tooltip("Çizgi kalınlığı")]
    public float lineWidth  = 0.08f;

    [Tooltip("Nabız hızı (0 = animasyon yok)")]
    public float pulseSpeed = 2.5f;
    [Tooltip("Nabız genişliği (ne kadar kalınlaşsın)")]
    public float pulseAmount = 0.03f;

    [Header("Glow Katmanı")]
    [Tooltip("Arkadaki parlama rengi")]
    public Color glowColor  = new Color(0f, 0.8f, 1f, 0.12f);    // Şeffaf cyan
    public float glowWidth  = 0.35f;

    [Header("Yol Noktası Marker")]
    [Tooltip("Hedef üzerinde küçük bir marker koyulsun mu?")]
    public bool showTargetMarker = true;
    public Color markerColor = new Color(0f, 1f, 0.9f, 1f);

    // ── İç bileşenler ─────────────────────────────────────────
    private LineRenderer coreLine;
    private LineRenderer glowLine;
    private GameObject   targetMarker;
    private NavMeshPath  path;
    private Transform    playerTransform;

    // ════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance == null) Instance = this;
        path = new NavMeshPath();

        // ── CORE çizgisi ──
        coreLine = GetComponent<LineRenderer>();
        if (coreLine == null) coreLine = gameObject.AddComponent<LineRenderer>();
        SetupLineRenderer(coreLine, lineWidth, colorStart, colorEnd, sortOrder: 1);

        // ── GLOW katmanı (arka planda, daha geniş + şeffaf) ──
        GameObject glowObj = new GameObject("PathGlow");
        glowObj.transform.SetParent(transform);
        glowLine = glowObj.AddComponent<LineRenderer>();
        SetupLineRenderer(glowLine, glowWidth, glowColor, new Color(glowColor.r, glowColor.g, glowColor.b, 0f), sortOrder: 0);

        // ── Hedef marker ──
        if (showTargetMarker)
        {
            targetMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            targetMarker.name = "TargetMarker";
            Destroy(targetMarker.GetComponent<Collider>());
            targetMarker.transform.localScale = Vector3.one * 0.3f;

            var mr = targetMarker.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (mat.shader.name == "Hidden/InternalErrorShader") // URP yoksa built-in'e dön
                mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = markerColor;
            mr.material = mat;

            targetMarker.SetActive(false);
        }

        // Başta gizle
        coreLine.enabled = false;
        glowLine.enabled = false;
    }

    // ════════════════════════════════════════════════════════════
    void Update()
    {
        // Oyuncuyu bul
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            else return;
        }

        if (target == null || !coreLine.enabled) return;

        // ── Yolu hesapla ──
        Vector3[] points = CalculatePath();
        ApplyPoints(points);

        // ── Nabız animasyonu ──
        AnimatePulse();

        // ── Marker ──
        if (targetMarker != null && targetMarker.activeSelf)
        {
            float bob = Mathf.Sin(Time.time * 3f) * 0.08f;
            targetMarker.transform.position = target.position + Vector3.up * (0.5f + bob);
            targetMarker.transform.Rotate(Vector3.up, 90f * Time.deltaTime);
        }
    }

    // ════════════════════════════════════════════════════════════
    // YOL HESAPLAMA
    // ════════════════════════════════════════════════════════════
    Vector3[] CalculatePath()
    {
        bool ok = NavMesh.CalculatePath(playerTransform.position, target.position, NavMesh.AllAreas, path);

        if (ok && path.status == NavMeshPathStatus.PathComplete)
            return path.corners;

        // Fallback: düz çizgi
        return new Vector3[] { playerTransform.position, target.position };
    }

    void ApplyPoints(Vector3[] points)
    {
        coreLine.positionCount = points.Length;
        glowLine.positionCount = points.Length;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 p = points[i] + Vector3.up * 0.12f;
            coreLine.SetPosition(i, p);
            glowLine.SetPosition(i, p);
        }
    }

    // ════════════════════════════════════════════════════════════
    // ANİMASYON
    // ════════════════════════════════════════════════════════════
    void AnimatePulse()
    {
        if (pulseSpeed <= 0f) return;

        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        coreLine.startWidth = lineWidth + pulse;
        coreLine.endWidth   = (lineWidth * 0.4f) + pulse * 0.3f;

        glowLine.startWidth = glowWidth + pulse * 2f;
        glowLine.endWidth   = glowWidth * 0.5f;

        // Gradient'ı zamanla kaydır (akan efekti)
        float shift = (Mathf.Sin(Time.time * 1.5f) * 0.5f + 0.5f) * 0.3f;
        coreLine.colorGradient = BuildGradient(colorStart, colorEnd, shift);
    }

    Gradient BuildGradient(Color a, Color b, float shift)
    {
        var g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(a, Mathf.Clamp01(shift)),
                new GradientColorKey(Color.Lerp(a, b, 0.5f), Mathf.Clamp01(0.5f + shift * 0.5f)),
                new GradientColorKey(b, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.2f, 0f),
                new GradientAlphaKey(1f,   0.15f),
                new GradientAlphaKey(1f,   1f)
            }
        );
        return g;
    }

    // ════════════════════════════════════════════════════════════
    // SETUP YARDIMCISI
    // ════════════════════════════════════════════════════════════
    void SetupLineRenderer(LineRenderer lr, float width, Color start, Color end, int sortOrder)
    {
        lr.useWorldSpace    = true;
        lr.numCornerVertices = 4;
        lr.numCapVertices    = 4;
        lr.startWidth = width;
        lr.endWidth   = width * 0.4f;
        lr.sortingOrder = sortOrder;

        // Material — built-in Sprite/Default her pipeline'da çalışır
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = Color.white;
        lr.material = mat;

        // Gradient
        var grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(start, 0f), new GradientColorKey(end, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(start.a, 0f), new GradientAlphaKey(end.a, 1f) }
        );
        lr.colorGradient = grad;
    }

    // ════════════════════════════════════════════════════════════
    // DIŞARIDAN KULLANIM
    // ════════════════════════════════════════════════════════════
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        coreLine.enabled = true;
        glowLine.enabled = true;

        if (targetMarker != null)
        {
            targetMarker.SetActive(true);
            targetMarker.transform.position = newTarget.position + Vector3.up * 0.5f;
        }
    }

    public void ClearTarget()
    {
        target = null;
        coreLine.enabled = false;
        glowLine.enabled = false;
        coreLine.positionCount = 0;
        glowLine.positionCount = 0;

        if (targetMarker != null) targetMarker.SetActive(false);
    }
}
