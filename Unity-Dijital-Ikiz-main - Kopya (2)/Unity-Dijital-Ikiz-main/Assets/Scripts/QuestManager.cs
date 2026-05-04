using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<Quest> allQuests = new List<Quest>();

    [Header("UI Bağlantıları")]
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI timerText; // SÜRE GÖSTERGESİ (Yeni)
    public GameObject questPanel;

    private Quest currentQuest; // Şu an yapılan görev

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Oyuna girişte ilk görevi başlat (Dosyadaki 1. Görev)
       StartQuest("Danışmanı Bul");
    }

    void Update()
    {
        // SÜRE SAYACI MANTIĞI (2. Görev İçin)
        if (currentQuest != null && currentQuest.isActive && currentQuest.timeLimit > 0)
        {
            currentQuest.timeLimit -= Time.deltaTime;

            // Ekrana Yazdır (00:59 formatında)
            if (timerText != null)
                timerText.text = Mathf.Ceil(currentQuest.timeLimit).ToString() + " sn";

            // Süre biterse?
            if (currentQuest.timeLimit <= 0)
            {
                FailQuest();
            }
        }
        else if (timerText != null)
        {
            timerText.text = ""; // Süreli görev değilse boş kalsın
        }
    }

    public void StartQuest(string questTitle)
    {
        currentQuest = allQuests.Find(x => x.title == questTitle);

        if (currentQuest != null && !currentQuest.isCompleted)
        {
            currentQuest.isActive = true;

            if (questPanel != null) questPanel.SetActive(true);
            if (questTitleText != null) questTitleText.text = "- " + currentQuest.title;

            // GPS Hedefi
            if (PathManager.Instance != null && currentQuest.targetLocation != null)
            {
                PathManager.Instance.SetTarget(currentQuest.targetLocation);
            }
        }
    }

    public void CompleteQuest(string questTitle)
    {
        Quest q = allQuests.Find(x => x.title == questTitle);

        if (q != null && q.isActive)
        {
            q.isActive = false;
            q.isCompleted = true;
            Debug.Log("Görev Bitti: " + q.title);

            // GPS Temizle
            if (PathManager.Instance != null) PathManager.Instance.ClearTarget();

            // ZİNCİRLEME SİSTEMİ (Mektubu okuyunca diğer göreve geç)
            if (!string.IsNullOrEmpty(q.nextQuestName))
            {
                StartQuest(q.nextQuestName);
            }
            else
            {
                // EĞER SONRAKİ GÖREV YOKSA (HİKAYE BİTTİYSE) SONSUZ MODU AÇ!
                if (questTitleText != null) questTitleText.text = "Tüm Ana Görevler Bitti!";

                if (WeeklyTaskManager.Instance != null)
                {
                    WeeklyTaskManager.Instance.OpenWeeklyPanel();
                }
            }
        }
    }

    public void FailQuest()
    {
        if (questTitleText != null) questTitleText.text = "GÖREV BAŞARISIZ! (Süre Doldu)";
        currentQuest.isActive = false;
        PathManager.Instance.ClearTarget();
    }
}