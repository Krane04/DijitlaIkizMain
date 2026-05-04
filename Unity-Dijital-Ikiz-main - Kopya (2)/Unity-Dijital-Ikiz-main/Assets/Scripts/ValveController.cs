using UnityEngine;

public class ValveController : MonoBehaviour
{
    public ParticleSystem waterEffect; // Su efekti
    public Transform valveHandle;      // Dönecek olan vana başlığı (Sphere)

    private bool isClosed = false;

    public void CloseTheValve()
    {
        if (isClosed) return; // Zaten kapalıysa işlem yapma

        // 1. Suyu Durdur
        if (waterEffect != null)
        {
            waterEffect.Stop();
        }

        // 2. Vanayı Görsel Olarak Çevir (90 derece sağa)
        if (valveHandle != null)
        {
            valveHandle.Rotate(0, 0, -90);
        }

        // 3. Görevi Tamamla (QuestManager'a haber ver)
        // Görev adını senaryodakiyle BİREBİR aynı yazmalısın!
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest("Su Vanasını Kapat");
        }

        isClosed = true;
        Debug.Log("Vana Kapatıldı!");
    }
}