using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public struct DestinationData
{
    public string targetName;     // Örn: "Sekreterlik", "Ahmet Hoca"
    public Transform location;    // Gidilecek yerin koordinatı
}

public class WeeklyTaskManager : MonoBehaviour
{
    public static WeeklyTaskManager Instance;

    [Header("Hedef Veritabanı")]
    public List<DestinationData> destinations;

    [Header("UI Bağlantıları")]
    public GameObject weeklyTaskPanel;   // Oyun sonunda açılacak panel
    public TMP_Dropdown targetDropdown;  // Seçim menüsü

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Başlangıçta paneli gizle ve Dropdown'ı doldur
        if (weeklyTaskPanel != null) weeklyTaskPanel.SetActive(false);

        if (targetDropdown != null)
        {
            targetDropdown.ClearOptions();
            List<string> names = new List<string>();
            foreach (var dest in destinations)
            {
                names.Add(dest.targetName);
            }
            targetDropdown.AddOptions(names);
        }
    }

    // BU FONKSİYON SON GÖREV BİTİNCE ÇAĞRILACAK
    public void OpenWeeklyPanel()
    {
        weeklyTaskPanel.SetActive(true);

        // Fareyi göster (Geçen hafta yazdığımız kural sayesinde karakter otomatik donacak!)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // "GİT" BUTONUNA BASINCA ÇALIŞACAK
    public void StartSelectedTask()
    {
        string selectedName = targetDropdown.options[targetDropdown.value].text;
        Transform targetRoom = null;

        foreach (var dest in destinations)
        {
            if (dest.targetName == selectedName)
            {
                targetRoom = dest.location;
                break;
            }
        }

        // GPS'i doğrudan seçilen yere yönlendir
        if (targetRoom != null && PathManager.Instance != null)
        {
            PathManager.Instance.SetTarget(targetRoom);

            // Ekranda bilgi ver
            if (QuestManager.Instance != null && QuestManager.Instance.questTitleText != null)
            {
                QuestManager.Instance.questTitleText.text = "Haftalık Görev: " + selectedName + " odasına git.";
            }
        }

        // Paneli kapat ve fareyi gizle (Karakter otomatik uyanacak)
        weeklyTaskPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}