using UnityEngine;

public class AlarmUITrigger : MonoBehaviour
{
    [Header("UI")]
    public GameObject alarmButtonUI;   // Canvas'taki alarm butonu

    [Header("Refs")]
    public AlarmController alarmController;

    [Header("Detect")]
    public string playerTag = "Player";

    private bool playerInside;

    void Start()
    {
        if (alarmButtonUI != null)
            alarmButtonUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = true;
        if (alarmButtonUI != null)
            alarmButtonUI.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = false;
        if (alarmButtonUI != null)
            alarmButtonUI.SetActive(false);
    }

    // UI Button OnClick buna baðlanacak
    public void OnAlarmButtonClick()
    {
        if (!playerInside) return; // güvenlik
        if (alarmController != null)
            alarmController.OnAlarmButtonPressed();
    }
}