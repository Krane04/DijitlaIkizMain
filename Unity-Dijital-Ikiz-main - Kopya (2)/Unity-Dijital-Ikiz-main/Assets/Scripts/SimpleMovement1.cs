using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam; // Kamera referansı şart!

    public float speed = 6f;
    public float turnSmoothTime = 0.1f; // Dönüş yumuşaklığı
    float turnSmoothVelocity;

    void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (cam == null) cam = Camera.main.transform;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // 1. Gideceğimiz yönü KAMERA'ya göre hesapla
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

            // 2. Karakteri o yöne yumuşakça döndür
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // 3. O yöne doğru hareket et
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        // Yer çekimi (Basit)
        if (!controller.isGrounded)
        {
            controller.Move(Vector3.down * 9.81f * Time.deltaTime);
        }
    }
}