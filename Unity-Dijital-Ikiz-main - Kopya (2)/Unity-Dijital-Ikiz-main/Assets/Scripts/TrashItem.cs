using UnityEngine;

/// <summary>
/// Sahnedeki çöp nesnelerine ekle.
/// Interactable'ı miras alır — InteractionSensor otomatik algılar.
/// Eller boşsa "Çöpü Al" butonu gösterir, dolu değilse gizler.
/// </summary>
public class TrashItem : Interactable
{
    private PlayerInventory inventory;

    void Awake()
    {
        promptMessage = "Çöpü Al";
        inventory = FindObjectOfType<PlayerInventory>();
    }

    // Eller doluyken bu çöp butonunu gösterme
    public override bool IsAvailable()
    {
        if (inventory == null) return true;
        return !inventory.isCarryingSomething;
    }

    // Butona basılınca → çöpü al
    public override void BaseInteract()
    {
        if (inventory == null || inventory.isCarryingSomething) return;

        inventory.EquipItem("Trash");
        Destroy(gameObject); // Yerdeki çöpü kaldır
    }
}
