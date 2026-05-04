using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// TrashMiniGame tarafından fırlatılan fiziksel çöp.
///
/// Davranış:
///   - Konteynere girerse (yeşil zone + Bin tag) → TrashManager'a bildirir, yok olur
///   - Yere/duvara çarparsa → pickableDelay sn sonra tekrar alınabilir hale gelir
/// </summary>
public class TrashProjectile : MonoBehaviour
{
    [Tooltip("Kaç saniye sonra otomatik yok edilsin? 0 = asla (yerde kalır)")]
    public float autoDestroyAfter = 0f;

    [Tooltip("Yere çarptıktan kaç saniye sonra tekrar alınabilsin? (fizik sakinleşsin)")]
    public float pickableDelay = 0.7f;

    // ── İç durum ──────────────────────────────────────────────
    private bool binDetectionEnabled = true;
    private bool hasLanded           = false;

    // ════════════════════════════════════════════════════════
    void Start()
    {
        if (autoDestroyAfter > 0f)
            Destroy(gameObject, autoDestroyAfter);
    }

    // ════════════════════════════════════════════════════════
    // TrashMiniGame çağırır — yeşil zone → true, sarı/kırmızı → false
    public void SetBinDetection(bool enabled)
    {
        binDetectionEnabled = enabled;
    }

    // ════════════════════════════════════════════════════════
    // KONTEYNER TETİKLEYİCİ
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Bin")) return;

        if (!binDetectionEnabled)
        {
            Debug.Log("Çöp konteynere yaklaştı ama sayılmadı (ıska/çerçeve).");
            return;
        }

        Debug.Log("<color=green>Çöp konteynere girdi!</color>");
        if (TrashManager.Instance != null)
            TrashManager.Instance.OnTrashBinned();

        Destroy(gameObject);
    }

    // ════════════════════════════════════════════════════════
    // YERE/DUVARA ÇARPMA — tekrar alınabilir yap
    void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;

        // Konteyner parent veya duvarlarına çarptıysa landed sayma —
        // çöp hâlâ içeriye düşüyor olabilir
        Transform hit = collision.transform;
        while (hit != null)
        {
            if (hit.CompareTag("Bin")) return; // konteynerin herhangi bir parçası
            hit = hit.parent;
        }

        hasLanded = true;
        Debug.Log("Çöp yere düştü — birazdan tekrar alınabilir.");
        Invoke(nameof(MakePickable), pickableDelay);
    }

    // ════════════════════════════════════════════════════════
    // Interactable ekle — InteractionSensor artık bu çöpü görebilir
    void MakePickable()
    {
        // Fiziği dondur — sabit dursun
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // Interactable bileşeni ekle
        var interactable = gameObject.AddComponent<Interactable>();
        interactable.promptMessage = "Çöpü Al";

        // onInteract null guard (runtime AddComponent için gerekli)
        if (interactable.onInteract == null)
            interactable.onInteract = new UnityEvent();

        interactable.onInteract.AddListener(PickUpFromGround);
    }

    // Oyuncu butona basınca çağrılır
    void PickUpFromGround()
    {
        var inventory = FindObjectOfType<PlayerInventory>();
        if (inventory == null || inventory.isCarryingSomething) return;

        inventory.EquipItem("Trash");
        Destroy(gameObject);
    }
}
