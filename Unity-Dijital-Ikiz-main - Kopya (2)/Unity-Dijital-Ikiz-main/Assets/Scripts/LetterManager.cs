using UnityEngine;
using UnityEngine.UI;

public class LetterManager : MonoBehaviour
{
    [Header("Mektup UI")]
    public GameObject letterPanel;

    [Header("'Kapat' Butonu")]
    public Button closeButton;

    [Header("Oyun UI'ı (Mektup açılınca gizlenecek)")]
    [Tooltip("Joystick ve butonların bulunduğu Canvas objesini buraya sürükle.")]
    public GameObject gameUIParent;   // CanvasGroup otomatik eklenir, sadece objeyi sürükle

    [Header("Oyuncu (hareketi durdurmak için — opsiyonel)")]
    public CharacterController playerCharacter;

    private CanvasGroup gameUIGroup;

    void Start()
    {
        // gameUIParent'a CanvasGroup yoksa otomatik ekle
        if (gameUIParent != null)
        {
            gameUIGroup = gameUIParent.GetComponent<CanvasGroup>();
            if (gameUIGroup == null)
                gameUIGroup = gameUIParent.AddComponent<CanvasGroup>();
        }

        if (letterPanel != null)
            letterPanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseLetter);
        }
    }

    public void OpenLetter()
    {
        if (letterPanel == null) return;
        letterPanel.SetActive(true);

        SetGameUI(false);

        if (playerCharacter != null) playerCharacter.enabled = false;
    }

    public void CloseLetter()
    {
        if (letterPanel == null) return;
        letterPanel.SetActive(false);

        SetGameUI(true);

        if (playerCharacter != null) playerCharacter.enabled = true;

        if (QuestManager.Instance != null)
            QuestManager.Instance.CompleteQuest("Danışmanı Bul");
    }

    void SetGameUI(bool visible)
    {
        if (gameUIGroup == null) return;
        gameUIGroup.alpha          = visible ? 1f : 0f;
        gameUIGroup.interactable   = visible;
        gameUIGroup.blocksRaycasts = visible;
    }
}
