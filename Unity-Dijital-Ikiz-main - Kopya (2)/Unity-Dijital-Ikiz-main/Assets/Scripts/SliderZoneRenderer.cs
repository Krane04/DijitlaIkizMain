using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slider arka planına yeşil/sarı/kırmızı bölge renkleri çizer.
/// SliderBackground objesine ekle. TrashMiniGame referansını ver.
/// Bölge boyutları mesafeye göre dinamik olarak güncellenir.
/// </summary>
public class SliderZoneRenderer : MonoBehaviour
{
    [Header("TrashMiniGame Referansı")]
    public TrashMiniGame miniGame;

    [Header("Bölge Görselleri")]
    [Tooltip("Slider arka planı (bu scriptin olduğu obje) — 300px geniş önerilir")]
    public RectTransform sliderBackground;

    // Bölge Image'ları — bu script otomatik oluşturur
    Image greenImage;
    Image yellowImage;
    Image redImage;

    static readonly Color ColGreen  = new Color(0.15f, 0.75f, 0.25f, 0.55f);
    static readonly Color ColYellow = new Color(0.95f, 0.70f, 0.0f,  0.45f);
    static readonly Color ColRed    = new Color(0.85f, 0.15f, 0.10f, 0.35f);

    void Awake()
    {
        if (sliderBackground == null)
            sliderBackground = GetComponent<RectTransform>();

        // Kırmızı bölge — en geniş, arka planda
        redImage    = CreateZoneImage("Zone_Red",    ColRed,    0);
        // Sarı bölge — ortada
        yellowImage = CreateZoneImage("Zone_Yellow", ColYellow, 1);
        // Yeşil bölge — en dar, öne çıkacak
        greenImage  = CreateZoneImage("Zone_Green",  ColGreen,  2);
    }

    void Update()
    {
        if (miniGame == null || sliderBackground == null) return;

        float totalWidth = sliderBackground.rect.width;  // örn. 300px

        // Yeşil genişlik — dinamik (TrashMiniGame'den al)
        float greenW  = GetGreenZoneHalf() * totalWidth * 2f;
        // Sarı genişlik — sabit
        float yellowW = miniGame.yellowZoneHalf * totalWidth * 2f;
        // Kırmızı = tüm bar
        float redW    = totalWidth;

        SetZoneWidth(redImage,    redW);
        SetZoneWidth(yellowImage, yellowW);
        SetZoneWidth(greenImage,  greenW);
    }

    float GetGreenZoneHalf()
    {
        return miniGame.GetDynamicGreenZone();
    }

    void SetZoneWidth(Image img, float width)
    {
        if (img == null) return;
        var rt = img.rectTransform;
        rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
    }

    Image CreateZoneImage(string name, Color color, int siblingIndex)
    {
        var go  = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(sliderBackground, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(50f, 0f); // Update'de ayarlanacak

        var img   = go.GetComponent<Image>();
        img.color = color;

        go.transform.SetSiblingIndex(siblingIndex);
        return img;
    }
}
