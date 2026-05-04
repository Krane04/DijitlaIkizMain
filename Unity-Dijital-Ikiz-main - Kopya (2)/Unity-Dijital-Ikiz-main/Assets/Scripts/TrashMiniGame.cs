using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Çöp Atma Mini-Oyunu — Super Golf / Basketbol tarzı timing mekaniği
///
/// AKIŞ:
///   1. Çöpü al (TrashItem → InteractionSensor)
///   2. Konteyner menzil + engel kontrolü yapılır
///      - Uygunsa: "Fırlat" butonu aktif (yeşil)
///      - Çok uzaksa: buton sarı + "Çok Uzak" uyarısı
///      - Engel varsa: buton kırmızı + "Hedef Kapalı" uyarısı
///   3. Butona bas ve tut → slider gidip gelir
///   4. Parmağı bırak:
///      - Yeşil bölgede → mükemmel, çöp AUTO-AIM ile konteynere girer
///      - Sarı bölgede  → çerçeveye çarpar, konteynerin yakınına düşer
///      - Kırmızı bölge → ıskalar, konteynerin daha uzağına düşer
///
/// YEŞİL BÖLGE DİNAMİK:
///   - Konteynere yakın → yeşil bölge geniş (kolay)
///   - Konteynere uzak  → yeşil bölge dar (zor)
/// </summary>
public class TrashMiniGame : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────
    [Header("Referanslar")]
    public Transform     throwPoint;             // El noktası
    public GameObject    trashProjectilePrefab;  // Rigidbody'li çöp prefab
    public LayerMask     obstacleMask;           // Engel layer'ları (duvar, mobilya vb.)

    [Header("Mesafe Ayarları")]
    public float maxThrowDistance = 10f;         // Maksimum atış menzili (metre)
    public float minThrowDistance =  1f;         // Konteynere çok yakın (direkt girer)

    [Header("Timing Ayarları")]
    [Range(0.3f, 3f)]
    public float sliderSpeed = 1.4f;             // Göstergenin gidip gelme hızı

    [Header("Yeşil Bölge (Mesafeye Göre Dinamik)")]
    [Range(0f, 0.5f)]
    [Tooltip("Çok yakında iken yeşil bölge yarısı — kolay")]
    public float greenZoneHalfClose = 0.32f;
    [Range(0f, 0.5f)]
    [Tooltip("Maksimum mesafede yeşil bölge yarısı — zor")]
    public float greenZoneHalfFar   = 0.08f;
    [Range(0f, 0.5f)]
    [Tooltip("Sarı bölge sabit yarısı")]
    public float yellowZoneHalf     = 0.24f;

    [Header("Atış Fiziği")]
    public float flightTime    = 1.1f;           // Çöpün hedefe ulaşma süresi (sn)
    [Tooltip("Maksimum ıska yarıçapı (metre, konteynere göre)")]
    public float maxMissRadius = 3.5f;

    // ─────────────────────────────────────────────────────────
    [Header("UI — Atış Butonu")]
    public GameObject      throwButtonRoot;      // Atış butonunu içeren panel
    public Image           throwButtonImage;     // Renk durumu için
    public TextMeshProUGUI throwButtonLabel;     // "Fırlat" / "Çok Uzak" / "Hedef Kapalı"

    [Header("UI — Timing Slider")]
    public GameObject    sliderPanel;            // Slider paneli (butona basınca açılır)
    public RectTransform sliderIndicator;        // Gidip gelen gösterge çizgisi
    public Image         sliderFill;             // Slider dolgu rengi (zone feedback)
    public float         sliderHalfWidth = 150f; // Göstergenin hareket aralığı (px)

    [Header("UI — Bölge Görsel Genişliği (Opsiyonel)")]
    [Tooltip("Yeşil bölge Image RectTransform — dinamik olarak boyutlanır")]
    public RectTransform greenZoneRect;
    [Tooltip("Sarı bölge Image RectTransform — sabit boyut için null bırakın")]
    public RectTransform yellowZoneRect;

    [Header("UI — Durum Mesajı")]
    public TextMeshProUGUI statusText;
    public float           statusShowTime = 1.5f;

    // ─────────────────────────────────────────────────────────
    // Renkler
    static readonly Color ColorGreen  = new Color(0.2f, 0.85f, 0.3f);
    static readonly Color ColorYellow = new Color(1f,   0.75f, 0f);
    static readonly Color ColorRed    = new Color(0.9f, 0.2f, 0.15f);

    // ─────────────────────────────────────────────────────────
    // İç durum
    enum ThrowState { Idle, ReadyOK, ReadyBlocked, ReadyFar, Aiming }
    ThrowState state = ThrowState.Idle;

    PlayerInventory     inventory;
    TrajectoryPredictor predictor;
    InteractionSensor   sensor;

    Transform nearestBin;
    float     sliderValue      = 0f;   // -1 … +1 (simetrik)
    bool      sliderGoingRight = true;
    float     statusTimer      = 0f;
    float     dynamicGreenZone = 0.12f; // Her karede mesafeye göre hesaplanır

    // ════════════════════════════════════════════════════════════
    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        predictor = GetComponent<TrajectoryPredictor>();
        sensor    = GetComponent<InteractionSensor>()
                 ?? GetComponentInParent<InteractionSensor>()
                 ?? FindObjectOfType<InteractionSensor>();
    }

    void Start()
    {
        if (throwButtonRoot != null) throwButtonRoot.SetActive(false);
        if (sliderPanel     != null) sliderPanel.SetActive(false);
        if (statusText      != null) statusText.text = "";
    }

    // ════════════════════════════════════════════════════════════
    void Update()
    {
        bool holdingTrash = inventory != null
                         && inventory.isCarryingSomething
                         && inventory.currentItemTag == "Trash";

        // Status mesajı zamanlayıcı
        if (statusTimer > 0f)
        {
            statusTimer -= Time.deltaTime;
            if (statusTimer <= 0f && statusText != null)
                statusText.text = "";
        }

        if (!holdingTrash)
        {
            SetState(ThrowState.Idle);
            return;
        }

        // Her karede en yakın konteyneri ve durumunu kontrol et
        EvaluateBin();

        // Slider animasyonu
        if (state == ThrowState.Aiming)
            AnimateSlider();

#if UNITY_EDITOR
        if (state == ThrowState.ReadyOK && Input.GetMouseButtonDown(0)) OnThrowDown();
        if (state == ThrowState.Aiming  && Input.GetMouseButtonUp(0))   OnThrowUp();
#endif
    }

    // ════════════════════════════════════════════════════════════
    // KONTEYNER DEĞERLENDİRME
    // ════════════════════════════════════════════════════════════
    void EvaluateBin()
    {
        nearestBin = FindNearestBin();

        // Dinamik yeşil bölgeyi her zaman güncelle (Aiming sırasında da)
        if (nearestBin != null)
        {
            Vector3 from = throwPoint != null ? throwPoint.position : transform.position;
            float   dist = Vector3.Distance(from, nearestBin.position);
            float   t    = Mathf.InverseLerp(minThrowDistance, maxThrowDistance, dist);
            dynamicGreenZone = Mathf.Lerp(greenZoneHalfClose, greenZoneHalfFar, t);
            RefreshZoneVisuals();
        }

        // ── Aiming sırasında state değiştirme! ──────────────────
        // (Slider açıkken EvaluateBin ReadyOK yapıp slider'ı kapatmasın)
        if (state == ThrowState.Aiming) return;

        if (nearestBin == null)
        {
            SetState(ThrowState.Idle);
            return;
        }

        Vector3 fromPos = throwPoint != null ? throwPoint.position : transform.position;
        Vector3 to      = nearestBin.position;
        float   distance = Vector3.Distance(fromPos, to);

        if (distance > maxThrowDistance)
        {
            SetState(ThrowState.ReadyFar);
            return;
        }

        if (distance < minThrowDistance)
        {
            SetState(ThrowState.ReadyOK);
            return;
        }

        Vector3 fromChest = fromPos + Vector3.up * 0.5f;
        bool    blocked   = Physics.Linecast(fromChest, to + Vector3.up * 0.3f, obstacleMask);

        SetState(blocked ? ThrowState.ReadyBlocked : ThrowState.ReadyOK);
    }

    Transform FindNearestBin()
    {
        GameObject[] bins = GameObject.FindGameObjectsWithTag("Bin");
        Transform nearest = null;
        float     minDist = Mathf.Infinity;

        foreach (var b in bins)
        {
            float d = Vector3.Distance(transform.position, b.transform.position);
            if (d < minDist) { minDist = d; nearest = b.transform; }
        }
        return nearest;
    }

    // ════════════════════════════════════════════════════════════
    // DURUM MAKİNESİ
    // ════════════════════════════════════════════════════════════
    void SetState(ThrowState newState)
    {
        if (state == newState) return;
        state = newState;

        switch (state)
        {
            case ThrowState.Idle:
                if (throwButtonRoot != null) throwButtonRoot.SetActive(false);
                if (sliderPanel     != null) sliderPanel.SetActive(false);
                if (predictor       != null) predictor.ClearTrajectory();
                break;

            case ThrowState.ReadyOK:
                ShowThrowButton(true, ColorGreen, "Fırlat!");
                break;

            case ThrowState.ReadyFar:
                ShowThrowButton(true, ColorYellow, "Çok Uzak");
                break;

            case ThrowState.ReadyBlocked:
                ShowThrowButton(true, ColorRed, "Hedef Kapalı");
                break;

            case ThrowState.Aiming:
                if (sliderPanel != null) sliderPanel.SetActive(true);
                sliderValue      = 0f;
                sliderGoingRight = true;
                // Yayı göster
                if (predictor != null && nearestBin != null && throwPoint != null)
                    predictor.RenderTrajectory(throwPoint.position, GetPerfectVelocity());
                break;
        }
    }

    void ShowThrowButton(bool show, Color color, string label)
    {
        if (throwButtonRoot != null) throwButtonRoot.SetActive(show);
        if (sliderPanel != null && state != ThrowState.Aiming)
            sliderPanel.SetActive(false);
        if (throwButtonImage != null) throwButtonImage.color = color;
        if (throwButtonLabel != null) throwButtonLabel.text  = label;
    }

    // ════════════════════════════════════════════════════════════
    // SLIDER ANİMASYON
    // ════════════════════════════════════════════════════════════
    void AnimateSlider()
    {
        float delta = sliderSpeed * Time.deltaTime * 2f;
        sliderValue += sliderGoingRight ? delta : -delta;

        if (sliderValue >=  1f) { sliderValue =  1f; sliderGoingRight = false; }
        if (sliderValue <= -1f) { sliderValue = -1f; sliderGoingRight = true;  }

        if (sliderIndicator != null)
            sliderIndicator.anchoredPosition =
                new Vector2(sliderValue * sliderHalfWidth, sliderIndicator.anchoredPosition.y);

        if (sliderFill != null)
            sliderFill.color = GetZoneColor(Mathf.Abs(sliderValue));
    }

    Color GetZoneColor(float absVal)
    {
        if (absVal <= dynamicGreenZone) return ColorGreen;
        if (absVal <= yellowZoneHalf)   return ColorYellow;
        return ColorRed;
    }

    Zone GetCurrentZone()
    {
        float abs = Mathf.Abs(sliderValue);
        if (abs <= dynamicGreenZone) return Zone.Green;
        if (abs <= yellowZoneHalf)   return Zone.Yellow;
        return Zone.Red;
    }

    enum Zone { Green, Yellow, Red }

    // ════════════════════════════════════════════════════════════
    // ZONE VİZÜEL GÜNCELLEME (Opsiyonel RectTransform'lar için)
    // ════════════════════════════════════════════════════════════
    void RefreshZoneVisuals()
    {
        // Yeşil bölgenin pixel genişliğini mesafeye göre ayarla
        if (greenZoneRect != null)
        {
            float greenPx = dynamicGreenZone * sliderHalfWidth * 2f * 2f;
            greenZoneRect.sizeDelta = new Vector2(greenPx, greenZoneRect.sizeDelta.y);
        }
        if (yellowZoneRect != null)
        {
            float yellowPx = yellowZoneHalf * sliderHalfWidth * 2f * 2f;
            yellowZoneRect.sizeDelta = new Vector2(yellowPx, yellowZoneRect.sizeDelta.y);
        }
    }

    // ════════════════════════════════════════════════════════════
    // BUTON OLAYLARI (EventTrigger'a bağla)
    // ════════════════════════════════════════════════════════════
    public void OnThrowDown()
    {
        if (state != ThrowState.ReadyOK) return;
        SetState(ThrowState.Aiming);
    }

    public void OnThrowUp()
    {
        if (state != ThrowState.Aiming) return;
        Launch(GetCurrentZone());
    }

    // ════════════════════════════════════════════════════════════
    // FIRLAT
    // ════════════════════════════════════════════════════════════
    void Launch(Zone zone)
    {
        if (predictor   != null) predictor.ClearTrajectory();
        if (sliderPanel != null) sliderPanel.SetActive(false);

        inventory.ResetVisuals();

        Vector3 spawnPos = throwPoint != null ? throwPoint.position : transform.position;

        // missT: 0 = mükemmel isabet, 1 = maksimum ıska
        float absVal = Mathf.Abs(sliderValue);
        float missT  = 0f;
        bool  scores = false;
        string msg;

        switch (zone)
        {
            case Zone.Green:
                missT  = 0f;
                scores = true;
                msg    = "Mükemmel!";
                break;

            case Zone.Yellow:
                // Yeşilden ne kadar uzakta → hafif ıska (0 … 0.35)
                missT  = Mathf.InverseLerp(dynamicGreenZone, yellowZoneHalf, absVal) * 0.35f;
                scores = false;
                msg    = "Çerçeveye Çarptı!";
                break;

            default: // Red
                // Sarıdan ne kadar uzakta → tam ıska (0.35 … 1.0)
                missT  = Mathf.Lerp(0.35f, 1f, Mathf.InverseLerp(yellowZoneHalf, 1f, absVal));
                scores = false;
                msg    = "Iskaladın!";
                break;
        }

        Vector3 velocity = GetShotVelocity(missT, scores);
        Color   msgColor = zone == Zone.Green  ? ColorGreen
                         : zone == Zone.Yellow ? ColorYellow : ColorRed;

        // Projectile oluştur ve fırlat (çöp her zaman fiziksel olarak havada uçar)
        if (trashProjectilePrefab != null)
        {
            GameObject proj = Instantiate(trashProjectilePrefab, spawnPos, Quaternion.identity);
            Rigidbody  rb   = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(velocity, ForceMode.Impulse);
            }

            // Yeşilde konteyner trigger'ı aktif (sayılır), sarı/kırmızıda kapalı (sayılmaz)
            var tp = proj.GetComponent<TrashProjectile>();
            if (tp != null) tp.SetBinDetection(scores);
        }

        ShowStatus(msg, msgColor);
        SetState(ThrowState.Idle);
    }

    // ════════════════════════════════════════════════════════════
    // HIZ HESAPLAMASI — Tek birleşik fonksiyon
    // ════════════════════════════════════════════════════════════
    /// <summary>
    /// missT = 0         → tam isabet, konteynere balistik atış
    /// missT = 0…0.35    → sarı bölge, konteynerin hemen yanına düşer
    /// missT = 0.35…1.0  → kırmızı bölge, konteynerin daha uzağına düşer
    /// </summary>
    Vector3 GetShotVelocity(float missT, bool perfectShot)
    {
        if (nearestBin == null || throwPoint == null)
            return transform.forward * 8f;

        Vector3 from   = throwPoint.position;
        Vector3 binPos = nearestBin.position + Vector3.up * 0.1f;

        Vector3 landTarget = binPos;

        if (!perfectShot && missT > 0f)
        {
            // Atış yönüne dik (XZ) sapma vektörü
            Vector3 toDir   = (binPos - from); toDir.y = 0f; toDir.Normalize();
            Vector3 lateral = Vector3.Cross(toDir, Vector3.up).normalized;

            float dist      = Vector3.Distance(from, binPos);
            float maxOffset = Mathf.Clamp(dist * 0.45f, 0.3f, maxMissRadius);
            float offset    = missT * maxOffset;

            float sideSign    = Random.value > 0.5f ? 1f : -1f;
            float forwardFrac = Random.Range(-0.25f, 0.25f);

            landTarget = binPos
                       + lateral * (offset * sideSign)
                       + toDir   * (offset * forwardFrac);
            landTarget.y = binPos.y;
        }

        // Balistik formül
        float T  = flightTime;
        float vx = (landTarget.x - from.x) / T;
        float vz = (landTarget.z - from.z) / T;
        float vy = (landTarget.y - from.y) / T - 0.5f * Physics.gravity.y * T;

        return new Vector3(vx, vy, vz);
    }

    Vector3 GetPerfectVelocity() => GetShotVelocity(0f, true);

    /// <summary>SliderZoneRenderer'ın dinamik yeşil genişliği okuması için.</summary>
    public float GetDynamicGreenZone() => dynamicGreenZone;

    // ════════════════════════════════════════════════════════════
    // YARDIMCILAR
    // ════════════════════════════════════════════════════════════
    void ShowStatus(string msg, Color color)
    {
        if (statusText == null) return;
        statusText.text  = msg;
        statusText.color = color;
        statusTimer      = statusShowTime;
    }
}
