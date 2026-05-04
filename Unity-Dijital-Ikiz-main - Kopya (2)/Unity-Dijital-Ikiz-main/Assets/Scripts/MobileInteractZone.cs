using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class MobileInteractZone : MonoBehaviour
{
    [Header("Arayüz (UI)")]
    public GameObject mobileButtonUI;
    public string interactMessage = "Etkileşime Geç";

    [Header("Etkileşim Ayarları")]
    public string playerTag = "Player";
    public UnityEvent OnInteractAction;

    private bool playerInside = false;

    void Start()
    {
        // Oyun başında butonu gizle (Buradan Koltuk Kapmaca kodlarını sildik!)
        if (mobileButtonUI != null)
            mobileButtonUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = true;

        if (mobileButtonUI != null)
        {
            Debug.Log("DİKKAT: Alana şu obje değdi: " + other.name + " | Tag: " + other.tag);
            // YENİ RÖNTGEN: Kim konuşuyor ve butonu var mı?
            Debug.Log("TETİKLENEN OBJE: " + gameObject.name + " | BUTON KUTUSU DOLU MU?: " + (mobileButtonUI != null));

            if (!other.CompareTag(playerTag)) return;

            if (!other.CompareTag(playerTag))
            {
                Debug.Log("HATA: Değen objenin Tag'i uyuşmuyor! Beklenen: " + playerTag + " Gelen: " + other.tag);
                return;
            }

            playerInside = true;
            // 1. BUTONUN YAZISINI DEĞİŞTİR
            TextMeshProUGUI buttonText = mobileButtonUI.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = interactMessage;

            // 2. BUTONUN GÖREVİNİ SADECE O ANKİ OBJEYE (Mektup/Alarm) BAĞLA!
            UnityEngine.UI.Button btn = mobileButtonUI.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners(); // Başkasından kalan görevi sil
                btn.onClick.AddListener(ButtonPressed); // Benim görevimi yükle
            }

            // 3. BUTONU GÖSTER
            mobileButtonUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = false;

        if (mobileButtonUI != null)
        {
            // KARAKTER UZAKLAŞINCA GÖREVİ TEMİZLE Kİ BAŞKASI KULLANABİLSİN
            UnityEngine.UI.Button btn = mobileButtonUI.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
            }

            // BUTONU GİZLE
            mobileButtonUI.SetActive(false);
        }
    }

    public void ButtonPressed()
    {
        if (!playerInside) return;
        OnInteractAction.Invoke();
    }
}