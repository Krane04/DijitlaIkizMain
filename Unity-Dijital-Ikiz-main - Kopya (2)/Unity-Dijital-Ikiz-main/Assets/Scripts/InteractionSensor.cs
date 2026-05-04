using UnityEngine;
using TMPro;

/// <summary>
/// Evrensel etkileşim sensörü.
/// Player'a bağlanır. OverlapSphere ile etrafındaki Interactable'ları tarar
/// ve CANVAS'TAKİ TEK BUTONU context'e göre günceller.
///
/// Trigger collider GEREKMİYOR — Is Trigger hatası artık imkansız.
/// </summary>
public class InteractionSensor : MonoBehaviour
{
    [Header("UI — TEK Buton (Canvas'taki)")]
    public GameObject interactButton;          // Göster/gizlenecek buton objesi
    public TextMeshProUGUI interactButtonText;  // Butonun yazısı

    [Header("Algılama")]
    [Tooltip("Kaç metrelik çevreye bakılsın?")]
    public float detectionRadius = 2f;

    // ——— Dışarıdan set edilebilen geçersiz kılma ———
    // PlayerExtinguisher elde tüp tutunca buraya BIRAK interactable'ını koyar.
    // Null olduğunda normal tarama devreye girer.
    [HideInInspector] public Interactable overrideInteractable;

    private Interactable closestInteractable;

    // ══════════════════════════════════════════
    void Start()
    {
        if (interactButton != null)
            interactButton.SetActive(false);
    }

    void Update()
    {
        FindClosest();
        UpdateUI();

        // PC / Editor test kolaylığı — Mobil build'de devre dışı kalır
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.E))
            OnInteractButtonPressed();
#endif
    }

    // ══════════════════════════════════════════
    // ALGILAMA
    // ══════════════════════════════════════════
    void FindClosest()
    {
        // Override set edilmişse (örn. elde tüp varken BIRAK) direkt onu kullan
        if (overrideInteractable != null)
        {
            closestInteractable = overrideInteractable;
            return;
        }

        // OverlapSphere: Trigger ya da static collider fark etmeksizin hepsini yakalar
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);

        float minDist = Mathf.Infinity;
        Interactable nearest = null;

        foreach (Collider hit in hits)
        {
            // Karakterin kendi collider'ları ve child'ları atla
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            // Önce collider'ın kendi objesine, sonra parent'a bak
            Interactable ia = hit.GetComponent<Interactable>()
                              ?? hit.GetComponentInParent<Interactable>();
            if (ia == null) continue;
            if (!ia.IsAvailable()) continue; // Örn. eller doluyken çöp gösterme

            float d = Vector3.Distance(transform.position, ia.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = ia;
            }
        }

        closestInteractable = nearest;
    }

    // ══════════════════════════════════════════
    // UI
    // ══════════════════════════════════════════
    void UpdateUI()
    {
        if (closestInteractable != null)
        {
            if (interactButton != null && !interactButton.activeSelf)
                interactButton.SetActive(true);

            if (interactButtonText != null)
                interactButtonText.text = closestInteractable.GetPromptMessage();
        }
        else
        {
            if (interactButton != null && interactButton.activeSelf)
                interactButton.SetActive(false);
        }
    }

    // ══════════════════════════════════════════
    // CANVAS BUTONU ONCLICK'İNE BAĞLA
    // ══════════════════════════════════════════
    public void OnInteractButtonPressed()
    {
        closestInteractable?.BaseInteract();
    }

    // ══════════════════════════════════════════
    // GIZMOS — Scene view'da algılama alanını gösterir
    // ══════════════════════════════════════════
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
