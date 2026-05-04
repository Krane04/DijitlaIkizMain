using UnityEngine;

[System.Serializable]
public class Quest
{
    public string title;          // Örn: "Danışmanı Bul"
    [TextArea] public string description; // "Mektubu okumak için odaya git."
    public bool isActive;
    public bool isCompleted;

    [Header("Hedef Ayarları")]
    public Transform targetLocation; // GPS burayı gösterecek

    [Header("Gelişmiş Ayarlar")]
    public float timeLimit;       // 0 ise süre yok. 60 ise 60 saniye.
    public string nextQuestName;  // Bu bitince otomatik başlayacak görevin adı
}