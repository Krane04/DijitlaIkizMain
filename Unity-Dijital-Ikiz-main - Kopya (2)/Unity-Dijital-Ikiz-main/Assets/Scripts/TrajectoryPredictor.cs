using UnityEngine;

/// <summary>
/// Fırlatma sırasında parabolik yay çizer.
/// TrashThrower ile aynı Player objesine ekle.
/// Player objesinde LineRenderer component olmalı (veya otomatik eklenir).
/// </summary>
public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Yay Ayarları")]
    [SerializeField] private int   segments  = 25;    // Nokta sayısı (artırınca yumuşar)
    [SerializeField] private float timeStep  = 0.08f; // Zaman adımı

    [Header("Yay Rengi")]
    public Color arcColor = new Color(1f, 0.85f, 0f, 0.85f); // Sarı

    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        if (lr == null) lr = gameObject.AddComponent<LineRenderer>();

        lr.useWorldSpace    = true;
        lr.numCornerVertices = 3;
        lr.numCapVertices    = 3;
        lr.startWidth       = 0.06f;
        lr.endWidth         = 0.02f;

        var mat = new Material(Shader.Find("Sprites/Default"));
        lr.material = mat;

        var grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(arcColor, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)  // sona doğru solar
            }
        );
        lr.colorGradient = grad;
        lr.enabled = false;
    }

    /// <summary>Başlangıç pozisyonu ve hız vektörüne göre yayı çiz.</summary>
    public void RenderTrajectory(Vector3 startPos, Vector3 velocity)
    {
        lr.enabled = true;
        lr.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float t = i * timeStep;
            // Fizik formülü: P = P0 + V0*t + 0.5*g*t²
            lr.SetPosition(i, startPos + velocity * t + 0.5f * Physics.gravity * t * t);
        }
    }

    /// <summary>Yayı gizle.</summary>
    public void ClearTrajectory()
    {
        lr.enabled = false;
        lr.positionCount = 0;
    }
}
