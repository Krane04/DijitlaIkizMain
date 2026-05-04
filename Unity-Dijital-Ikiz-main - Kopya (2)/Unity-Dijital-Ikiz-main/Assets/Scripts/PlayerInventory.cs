using UnityEngine;

/// <summary>
/// Karakterin elinde taşıdığı eşyayı yönetir.
/// Player objesine ekle.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Durum (Salt Okunur)")]
    public bool   isCarryingSomething = false;
    public string currentItemTag      = "";

    [Header("El Görselleri")]
    [Tooltip("Elde gösterilecek çöp modeli (el noktasının child'ı olmalı)")]
    public GameObject handTrashVisual;

    void Start()
    {
        ResetVisuals();
    }

    /// <summary>Eşyayı envantere al ve el görselini göster.</summary>
    public void EquipItem(string itemTag)
    {
        ResetVisuals();

        isCarryingSomething = true;
        currentItemTag      = itemTag;

        if (itemTag == "Trash" && handTrashVisual != null)
            handTrashVisual.SetActive(true);
    }

    /// <summary>Eli boşalt (fırlatma sonrası veya bırakma).</summary>
    public void ResetVisuals()
    {
        if (handTrashVisual != null) handTrashVisual.SetActive(false);
        isCarryingSomething = false;
        currentItemTag      = "";
    }
}
