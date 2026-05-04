using UnityEngine;
using TMPro;

/// <summary>
/// Çöp toplama görevini yönetir.
/// Sahnedeki tüm TrashItem'ları başta sayar.
/// Çöpler konteynere girince sayar — hepsi tamamlanınca görevi bitirir.
///
/// Kurulum:
///   - Boş bir objeye ekle
///   - Çöp konteynerin içine boş child ekle → Collider (Is Trigger=true) → Tag = "Bin"
///   - Fırlatılan çöp prefabına TrashProjectile scripti ekle
/// </summary>
public class TrashManager : MonoBehaviour
{
    public static TrashManager Instance;

    [Header("Görev")]
    public string questName = "Çöpleri Topla";

    [Header("UI (Opsiyonel)")]
    [Tooltip("'2/5 çöp atıldı' ilerleme yazısı")]
    public TextMeshProUGUI progressText;

    // ── Sayaçlar ──────────────────────────────────────────────
    private int totalTrash;   // Başta sahnedeki TrashItem sayısı
    private int binnedCount;  // Konteynere giren çöp sayısı

    // ════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        totalTrash  = FindObjectsOfType<TrashItem>(true).Length;
        binnedCount = 0;

        UpdateUI();
        Debug.Log($"[TrashManager] Sahnede {totalTrash} çöp var.");
    }

    /// <summary>
    /// TrashProjectile konteynere girince bunu çağırır.
    /// </summary>
    public void OnTrashBinned()
    {
        binnedCount++;
        UpdateUI();
        Debug.Log($"[TrashManager] {binnedCount}/{totalTrash} çöp konteynere girdi.");

        if (binnedCount >= totalTrash)
        {
            Debug.Log("[TrashManager] Tüm çöpler toplandı! Görev tamamlandı.");
            if (QuestManager.Instance != null)
                QuestManager.Instance.CompleteQuest(questName);
        }
    }

    void UpdateUI()
    {
        if (progressText != null)
            progressText.text = $"Çöp: {binnedCount}/{totalTrash}";
    }
}
