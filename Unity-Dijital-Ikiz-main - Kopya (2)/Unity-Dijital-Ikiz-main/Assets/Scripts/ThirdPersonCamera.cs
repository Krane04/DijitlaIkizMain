using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Takip Ayarları")]
    public Transform target;       // Takip edilecek kişi (Player)
    public Vector3 offset = new Vector3(0, 2f, -4f); // Kamera ne kadar geride/yukarıda dursun?
    public float smoothSpeed = 10f; // Takip yumuşaklığı

    [Header("Dönüş Ayarları")]
    public float mouseSensitivity = 3f;
    public float pitchMin = -35f; // Aşağı bakma sınırı
    public float pitchMax = 60f;  // Yukarı bakma sınırı

    private float yaw;   // Yatay dönüş (Y Ekseni)
    private float pitch; // Dikey dönüş (X Ekseni)

    void Start()
    {
        // Başlangıçta farenin dönüşünü al ki kamera zıplamasın
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // Fareyi kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!target) return;
        if (Cursor.visible == true) return;

        // 1. Fare Hareketini Al
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax); // Boynu kırılmasın diye sınırla

        // 2. Rotasyonu Hesapla
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // 3. Pozisyonu Hesapla (Hedefin konumu + Dönüş * Mesafe)
        // Kamerayı hedefin tam içine değil, offset kadar uzağına koyuyoruz
        Vector3 position = target.position + rotation * offset;

        // 4. Uygula
        transform.rotation = rotation;
        transform.position = Vector3.Lerp(transform.position, position, smoothSpeed * Time.deltaTime);
    }
}