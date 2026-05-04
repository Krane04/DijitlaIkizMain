using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MOBİL çöp fırlatma sistemi.
/// Player objesine ekle. PlayerInventory ve TrajectoryPredictor ile çalışır.
///
/// KULLANIM AKIŞI:
///   1. Çöpü al (TrashItem → InteractionSensor üzerinden)
///   2. "Fırlat" butonu görünür
///   3. Butona bas ve tut → güç çubuğu ping-pong + yay gösterilir
///   4. Parmağı kaldır → çöp fırlatılır
///
/// Canvas Kurulumu:
///   • throwButton  → EventTrigger: PointerDown=OnThrowDown, PointerUp=OnThrowUp
///   • powerSlider  → güç göstergesi (interactable=false yapılabilir)
/// </summary>
public class TrashThrower : MonoBehaviour
{
    [Header("Fırlatma")]
    [Tooltip("Rigidbody'li çöp prefab'ı")]
    public GameObject trashProjectilePrefab;
    [Tooltip("Fırlatma çıkış noktası (el noktası)")]
    public Transform  throwPoint;
    [Tooltip("Fırlatma yönü (Y=yukarı, Z=ileri). İnce ayar yapılabilir.")]
    public Vector3    throwAngle  = new Vector3(0f, 1.5f, 1f);
    public float      maxForce    = 22f;

    [Header("Güç Çubuğu UI")]
    [Tooltip("Canvas'taki Slider (güç göstergesi)")]
    public Slider  powerSlider;
    public float   powerSpeed = 2.2f;

    [Header("Fırlat Butonu")]
    [Tooltip("Canvas'taki fırlat butonu (elde çöp varken görünür)")]
    public GameObject throwButton;

    // ── İç durum ─────────────────────────────────────────────
    private PlayerInventory      inventory;
    private TrajectoryPredictor  predictor;

    private float currentPower = 0f;
    private bool  isAiming     = false;

    // ════════════════════════════════════════════════════════════
    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        predictor = GetComponent<TrajectoryPredictor>();
    }

    void Start()
    {
        if (powerSlider  != null) powerSlider.gameObject.SetActive(false);
        if (throwButton  != null) throwButton.SetActive(false);
    }

    void Update()
    {
        // Fırlat butonunu elde çöp varsa göster, yoksa gizle
        bool holdingTrash = inventory != null
                         && inventory.isCarryingSomething
                         && inventory.currentItemTag == "Trash";

        if (throwButton != null && throwButton.activeSelf != holdingTrash)
            throwButton.SetActive(holdingTrash);

        // Nişan alma aktifse güç çubuğunu ve yayı güncelle
        if (isAiming && holdingTrash)
        {
            currentPower = Mathf.PingPong(Time.time * powerSpeed * 50f, 100f);

            if (powerSlider != null) powerSlider.value = currentPower;

            if (predictor != null)
                predictor.RenderTrajectory(throwPoint.position, CalculateVelocity());
        }

        // Editor/PC test: sol tık
#if UNITY_EDITOR
        if (holdingTrash)
        {
            if (Input.GetMouseButtonDown(0) && !isAiming) OnThrowDown();
            if (Input.GetMouseButtonUp(0)   &&  isAiming) OnThrowUp();
        }
#endif
    }

    // ════════════════════════════════════════════════════════════
    // MOBİL BUTON OLAYLARI
    // Canvas > Button > EventTrigger:
    //   PointerDown → OnThrowDown
    //   PointerUp   → OnThrowUp
    // ════════════════════════════════════════════════════════════
    public void OnThrowDown()
    {
        if (inventory == null || !inventory.isCarryingSomething) return;
        isAiming = true;
        if (powerSlider != null) powerSlider.gameObject.SetActive(true);
    }

    public void OnThrowUp()
    {
        if (!isAiming) return;
        Launch();
    }

    // ════════════════════════════════════════════════════════════
    // FIRLAT
    // ════════════════════════════════════════════════════════════
    void Launch()
    {
        isAiming = false;

        if (powerSlider != null) powerSlider.gameObject.SetActive(false);
        if (predictor   != null) predictor.ClearTrajectory();

        // El görselini kaldır ve envanteri sıfırla
        inventory.ResetVisuals();

        // Fiziksel çöpü sahneye ekle ve fırlat
        if (trashProjectilePrefab != null && throwPoint != null)
        {
            GameObject proj = Instantiate(trashProjectilePrefab, throwPoint.position, throwPoint.rotation);
            Rigidbody  rb   = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(CalculateVelocity(), ForceMode.Impulse);
            }
        }

        // Sayım TrashProjectile → TrashManager üzerinden yapılır.
        // (Çöp konteynere girince OnTrashBinned() tetiklenir)
    }

    Vector3 CalculateVelocity()
    {
        return transform.TransformDirection(throwAngle).normalized
               * (currentPower / 100f)
               * maxForce;
    }
}
