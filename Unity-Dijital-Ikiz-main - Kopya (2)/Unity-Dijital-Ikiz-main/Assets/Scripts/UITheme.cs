using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tüm UI butonlarına ve joystick'e modern tema uygular.
/// Bu scripti sahneye boş bir objeye ekle, otomatik çalışır.
///
/// Tema: Koyu arkaplan + Neon cyan/yeşil vurgu + beyaz yazı
/// </summary>
public class UITheme : MonoBehaviour
{
    [Header("Buton Renkleri")]
    public Color buttonNormal      = new Color(0.08f, 0.08f, 0.12f, 0.85f);  // Koyu lacivert
    public Color buttonHighlight   = new Color(0f,    0.85f, 0.8f,  0.95f);  // Neon cyan
    public Color buttonPressed     = new Color(0f,    0.6f,  0.55f, 1f);     // Koyu cyan
    public Color buttonTextColor   = new Color(1f,    1f,    1f,    1f);     // Beyaz

    [Header("Joystick Renkleri")]
    [Tooltip("Joystick arka plan objesinin adı (hiyerarşide)")]
    public string joystickBgName   = "Background";
    [Tooltip("Joystick topuzu objesinin adı")]
    public string joystickKnobName = "Handle";
    public Color joystickBgColor   = new Color(1f, 1f, 1f, 0.08f);           // Neredeyse şeffaf
    public Color joystickKnobColor = new Color(0f, 1f, 0.9f, 0.5f);          // Yarı şeffaf cyan

    [Header("Etkileşim Butonu (AL/BIRAK/Alarmı Çal)")]
    [Tooltip("Etkileşim butonunun adı (hiyerarşide)")]
    public string interactButtonName = "InteractButton";
    public Color interactNormal    = new Color(0f,   0.75f, 0.7f,  0.9f);
    public Color interactPressed   = new Color(0f,   0.5f,  0.45f, 1f);

    // ════════════════════════════════════════════════════════════
    void Start()
    {
        ApplyToAllButtons();
        ApplyToJoystick();
    }

    // ── TÜM BUTONLARA UYGULA ──────────────────────────────────
    void ApplyToAllButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (Button btn in buttons)
        {
            // Renk bloğu
            ColorBlock cb = btn.colors;

            if (btn.gameObject.name == interactButtonName)
            {
                // Etkileşim butonu farklı renk
                cb.normalColor      = interactNormal;
                cb.highlightedColor = interactNormal;
                cb.pressedColor     = interactPressed;
                cb.selectedColor    = interactNormal;
            }
            else
            {
                cb.normalColor      = buttonNormal;
                cb.highlightedColor = buttonHighlight;
                cb.pressedColor     = buttonPressed;
                cb.selectedColor    = buttonNormal;
            }

            cb.fadeDuration = 0.08f;
            btn.colors = cb;

            // Buton Image arka planı
            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = cb.normalColor;
                // Eğer sprite yoksa Unity'nin default rounded rect'ini kullan
                if (img.sprite == null)
                    img.type = Image.Type.Sliced;
            }

            // Yazı rengi
            TMP_Text tmp = btn.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                tmp.color = buttonTextColor;
                tmp.fontStyle = FontStyles.Bold;
            }

            // Shadow efekti (varsa)
            Shadow shadow = btn.GetComponentInChildren<Shadow>();
            if (shadow == null)
                shadow = btn.gameObject.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0f, 0.8f, 0.8f, 0.4f);
            shadow.effectDistance = new Vector2(1f, -1f);
        }
    }

    // ── JOYSTİCK'E UYGULA ─────────────────────────────────────
    void ApplyToJoystick()
    {
        // Arka plan
        GameObject bg = GameObject.Find(joystickBgName);
        if (bg != null)
        {
            Image img = bg.GetComponent<Image>();
            if (img != null) img.color = joystickBgColor;
        }

        // Topuz
        GameObject knob = GameObject.Find(joystickKnobName);
        if (knob != null)
        {
            Image img = knob.GetComponent<Image>();
            if (img != null) img.color = joystickKnobColor;
        }
    }
}
