using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Yangın söndürücü alma/bırakma ve püskürtme sistemi.
///
/// SPRAY MEKANİĞİ:
///   Tüp elde olduğunda ekranın herhangi bir yerine basılınca (joystick ve butonlar hariç)
///   dokunulan yöne doğru köpük fırlar. Parmak sürüklenince nişan güncellenir.
///   Ayrı bir spray butonu GEREKMİYOR.
/// </summary>
public class PlayerExtinguisher : MonoBehaviour
{
    [Header("Pickup")]
    public Transform elTutmaNoktasi;
    public float pickupRadius = 1.5f;
    public LayerMask tupLayer;

    [Header("Spray")]
    public Transform sprayPoint;
    public float sondurmeGucu = 10f;
    public float maxAimDistance = 200f;
    public float extinguishRange = 20f;
    public LayerMask fireLayer;

    [Header("Görev Entegrasyonu")]
    [Tooltip("Tüp alınınca tamamlanacak görevin adı (QuestManager listesiyle birebir aynı olmalı)")]
    public string pickupQuestName = "Yangın Söndürücüyü Al";

    [Header("Debug")]
    public GameObject eldekiTup;

    // ── Evrensel sensör bağlantısı ─────────────────────────────
    private InteractionSensor sensor;
    private Interactable dropInteractable;

    private ParticleSystem tupKopugu;
    private bool tupElimde;
    private Camera cam;

    // ════════════════════════════════════════════════════════════
    void Awake()
    {
        cam = Camera.main;

        // Sensörü bul: aynı obje → parent → sahnede
        sensor = GetComponent<InteractionSensor>()
              ?? GetComponentInParent<InteractionSensor>()
              ?? FindObjectOfType<InteractionSensor>();

        // "BIRAK" için runtime Interactable
        dropInteractable = gameObject.AddComponent<Interactable>();
        dropInteractable.promptMessage = "BIRAK";
        if (dropInteractable.onInteract == null)
            dropInteractable.onInteract = new UnityEngine.Events.UnityEvent();
        dropInteractable.onInteract.AddListener(DropTube);
    }

    void Start()
    {
        if (cam == null) cam = Camera.main;
    }

    // ════════════════════════════════════════════════════════════
    void Update()
    {
        if (cam == null) cam = Camera.main;
        if (!tupElimde) return;

        // Ekrana dokunulan pozisyonu al (joystick / UI butonlar hariç)
        bool spraying = TryGetAimScreenPos(out Vector2 aimPos);

        if (spraying)
            ShootExtinguisher(aimPos);
        else
            StopSpray();
    }

    // ════════════════════════════════════════════════════════════
    // EKRANA BASILINCA NİŞAN AL
    // UI üzerindeki (joystick, butonlar) dokunuşları atlar.
    // ════════════════════════════════════════════════════════════
    bool TryGetAimScreenPos(out Vector2 screenPos)
    {
        screenPos = Vector2.zero;

#if UNITY_ANDROID || UNITY_IOS
        // Tüm parmakları tara, UI dışındaki ilk geçerli parmağı kullan
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            // UI'ın üzerindeyse (joystick, buton vb.) bu parmağı atla
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                continue;

            if (t.phase == TouchPhase.Began    ||
                t.phase == TouchPhase.Moved     ||
                t.phase == TouchPhase.Stationary)
            {
                screenPos = t.position;
                return true;
            }
        }
        return false;
#else
        // PC / Editor: sol tık basılıyken ve UI üzerinde değilken
        if (Input.GetMouseButton(0))
        {
            bool overUI = EventSystem.current != null
                       && EventSystem.current.IsPointerOverGameObject();
            if (!overUI)
            {
                screenPos = Input.mousePosition;
                return true;
            }
        }
        return false;
#endif
    }

    // ════════════════════════════════════════════════════════════
    // AL / BIRAK
    // ════════════════════════════════════════════════════════════
    public void OnPickupButton()
    {
        if (!tupElimde) TryPickupNearestTube();
        else            DropTube();
    }

    void TryPickupNearestTube()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, tupLayer);
        if (hits == null || hits.Length == 0) return;

        Collider best  = hits[0];
        float    bestD = Vector3.Distance(transform.position, best.transform.position);
        for (int i = 1; i < hits.Length; i++)
        {
            float d = Vector3.Distance(transform.position, hits[i].transform.position);
            if (d < bestD) { bestD = d; best = hits[i]; }
        }

        TupuElineAl(best.gameObject);
    }

    void TupuElineAl(GameObject tup)
    {
        tupElimde = true;
        eldekiTup = tup;

        Rigidbody rb = tup.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic     = true;
            rb.useGravity      = false;
            rb.velocity        = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col = tup.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        tup.transform.SetParent(elTutmaNoktasi);
        tup.transform.localPosition = Vector3.zero;
        tup.transform.localRotation = Quaternion.identity;

        tupKopugu = tup.GetComponentInChildren<ParticleSystem>(true);
        if (sprayPoint == null && tupKopugu != null)
            sprayPoint = tupKopugu.transform;

        // Sensöre BIRAK modunu bildir
        if (sensor != null)
            sensor.overrideInteractable = dropInteractable;

        // Görev: Yangın Söndürücüyü Al
        if (!string.IsNullOrEmpty(pickupQuestName) && QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest(pickupQuestName);
    }

    public void DropTube()
    {
        if (eldekiTup == null) return;

        StopSpray();

        eldekiTup.transform.SetParent(null);
        eldekiTup.transform.position = transform.position + transform.forward * 0.8f + Vector3.up * 0.5f;

        Collider col = eldekiTup.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = eldekiTup.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic     = false;
            rb.useGravity      = true;
            rb.velocity        = transform.forward * 1.5f;
            rb.angularVelocity = Vector3.zero;
        }

        tupElimde = false;
        tupKopugu = null;
        eldekiTup = null;

        // Sensörü normal moda döndür
        if (sensor != null)
            sensor.overrideInteractable = null;
    }

    // ════════════════════════════════════════════════════════════
    // SPRAY — Dokunulan ekran noktasına doğru köpük fırlat
    // ════════════════════════════════════════════════════════════
    void ShootExtinguisher(Vector2 aimScreenPos)
    {
        if (tupKopugu == null || cam == null) return;

        if (!tupKopugu.isPlaying)
            tupKopugu.Play();

        Transform sp = sprayPoint != null ? sprayPoint : tupKopugu.transform;

        // Dokunulan ekran koordinatından dünyaya ray
        Ray ray = cam.ScreenPointToRay(aimScreenPos);
        RaycastHit hit;

        Vector3 hedef = Physics.Raycast(ray, out hit, maxAimDistance)
            ? hit.point
            : ray.GetPoint(50f);

        sp.LookAt(hedef);

        // Yangını söndür
        if (Physics.Raycast(sp.position, sp.forward, out hit, extinguishRange, fireLayer))
        {
            FireHealth ates = hit.collider.GetComponent<FireHealth>()
                           ?? hit.collider.GetComponentInParent<FireHealth>();
            if (ates != null)
                ates.ExtinguishFire(sondurmeGucu * Time.deltaTime);
        }

        Debug.DrawRay(sp.position, sp.forward * extinguishRange, Color.cyan);
    }

    void StopSpray()
    {
        if (tupKopugu != null && tupKopugu.isPlaying)
            tupKopugu.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
