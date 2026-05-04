using UnityEngine;

/// <summary>
/// Alarm butonuna bağlanan dinamik Interactable.
/// AlarmController'ın durumuna göre buton yazısını otomatik günceller:
///
///   Alarm kapalı              → "Alarmı Çal"
///   Alarm açık, yangın devam  → "Yangın Sürüyor..."
///   Alarm açık, yangın bitti  → "Alarmı Kapat"
///
/// Inspector kurulumu:
///   Bu script yangın alarmı trigger objesine eklenir.
///   AlarmController alanına sahnedeki AlarmController objesi sürüklenir.
///   Ayrıca Interactable → onInteract olayına GEREK YOK;
///   BaseInteract() bu script tarafından doğrudan yönetilir.
/// </summary>
public class AlarmInteractable : Interactable
{
    [Header("Alarm Bağlantısı")]
    public AlarmController alarmController;

    public override string GetPromptMessage()
    {
        if (alarmController == null)
            return promptMessage; // fallback

        if (!alarmController.AlarmStarted)
            return "Alarmı Çal";

        if (!alarmController.AllFiresExtinguished)
            return "Yangın Sürüyor...";

        return "Alarmı Kapat";
    }

    public override void BaseInteract()
    {
        if (alarmController != null)
            alarmController.OnAlarmButtonPressed();
    }
}
