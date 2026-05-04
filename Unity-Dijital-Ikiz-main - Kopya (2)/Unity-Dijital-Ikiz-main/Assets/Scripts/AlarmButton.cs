using UnityEngine;
using TMPro;

/// <summary>
/// Yangın senaryosu çok aşamalı görev akışı:
///
///  1. "Alarmı Çal"           → alarm butona basılınca tamamlanır
///  2. "Yangın Söndürücüyü Al"→ PlayerExtinguisher üstlenir
///  3. "Yangınları Söndür"    → tüm yangınlar sönünce tamamlanır (burada)
///  4. "Alarmı Kapat"         → alarm kapatılınca tamamlanır (burada)
///
/// Inspector'daki görev adlarını QuestManager listenizdekiyle BİREBİR eşleştirin.
/// </summary>
public class AlarmController : MonoBehaviour
{
    [Header("Siren")]
    public AudioSource siren;

    [Header("UI")]
    public TMP_Text alarmButtonText;

    [Header("Görev Adları")]
    public string questAlarmStart  = "Alarmı Çal";
    public string questFiresOut    = "Yangınları Söndür";
    public string questAlarmStop   = "Alarmı Kapat";

    public bool AlarmStarted        { get; private set; }
    public bool AllFiresExtinguished { get; private set; }

    int totalFires;
    int extinguishedFires;

    void OnEnable()  { FireHealth.OnAnyFireExtinguished += OnFireOut; }
    void OnDisable() { FireHealth.OnAnyFireExtinguished -= OnFireOut; }

    void Start()
    {
        totalFires = FindObjectsOfType<FireHealth>(true).Length;
        UpdateButtonText();
    }

    // AlarmInteractable → BaseInteract() buraya bağlı
    public void OnAlarmButtonPressed()
    {
        if (!AlarmStarted)
        {
            StartAlarm();
            return;
        }

        if (AllFiresExtinguished)
            StopAlarm();
        else
            Debug.Log("Yangın devam ediyor, alarm kapatılamaz.");
    }

    // ── AŞAMA 1: Alarm Başlat ──────────────────────────────────
    void StartAlarm()
    {
        AlarmStarted = true;

        if (siren != null) { siren.loop = true; siren.Play(); }

        UpdateButtonText();

        if (QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest(questAlarmStart);
    }

    // ── AŞAMA 4: Alarm Kapat ───────────────────────────────────
    void StopAlarm()
    {
        AlarmStarted = false;

        if (siren != null) siren.Stop();

        UpdateButtonText();

        if (QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest(questAlarmStop);
    }

    // ── AŞAMA 3: Tüm Yangınlar Söndü ──────────────────────────
    void OnFireOut()
    {
        extinguishedFires++;

        if (extinguishedFires >= totalFires)
        {
            AllFiresExtinguished = true;
            Debug.Log("Tüm yangınlar söndü, alarm kapatılabilir.");

            if (QuestManager.Instance != null)
                QuestManager.Instance.CompleteQuest(questFiresOut);
        }
    }

    void UpdateButtonText()
    {
        if (alarmButtonText == null) return;
        alarmButtonText.text = AlarmStarted ? "ALARM KAPAT" : "ALARM AÇ";
    }
}
