using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public struct AdvisorRoom
{
    public string advisorName;
    public Transform roomLocation;
}

public class PlayerProfileManager : MonoBehaviour
{
    [Header("Fakülte Danışman Veritabanı")]
    public List<AdvisorRoom> advisorDatabase;

    [Header("UI Bağlantıları")]
    public TMP_Dropdown advisorDropdown;
    public GameObject loginPanel;

    [Header("Oyuncu Bağlantıları (Dondurmak İçin)")]
    public MonoBehaviour playerMovementScript; // SimpleMovement'ı buraya atacağız
    public MonoBehaviour cameraScript;         // ThirdPersonCamera'yı buraya atacağız

    void Start()
    {
        // 1. MENÜ AÇIKKEN OYUNCUYU VE KAMERAYI DONDUR
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (cameraScript != null) cameraScript.enabled = false;

        // 2. FAREYİ SERBEST BIRAK Kİ BUTONA TIKLAYABİLELİM
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Dropdown menüsünü doldur
        if (advisorDropdown != null)
        {
            advisorDropdown.ClearOptions();
            List<string> names = new List<string>();
            foreach (var advisor in advisorDatabase)
            {
                names.Add(advisor.advisorName);
            }
            advisorDropdown.AddOptions(names);
        }
    }

    public void StartGameSequence()
    {
        // Dropdown'dan seçilen isme göre listedeki hocayı bul
        string selectedName = advisorDropdown.options[advisorDropdown.value].text;
        Transform targetRoom = null;

        foreach (var advisor in advisorDatabase)
        {
            if (advisor.advisorName == selectedName)
            {
                targetRoom = advisor.roomLocation;
                break;
            }
        }

        // QuestManager'a hedefini ver ve görevi başlat
        if (targetRoom != null && QuestManager.Instance != null)
        {
            QuestManager.Instance.allQuests[0].targetLocation = targetRoom;
            QuestManager.Instance.StartQuest("Danışmanı Bul");
        }

        // 3. UI KAPAT VE FAREYİ KİLİTLE (Oyun başlıyor)
        if (loginPanel != null) loginPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 4. OYUNCUYU VE KAMERAYI ÇÖZ (Artık hareket edebiliriz!)
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (cameraScript != null) cameraScript.enabled = true;
    }
}