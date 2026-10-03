using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public float followSmoothTime = 0f;
    public float mouseInfluence = 0.3f;
    public float mouseSmooth = 5f;

    Vector3 offset;
    Vector3 velocity;
    Vector3 mouseOffset;

    Camera cam;

    void Start()
    {
        offset = transform.position - target.position;

        cam = GetComponent<Camera>();
        if (cam) cam.fieldOfView = 75f;
    }

    void LateUpdate()
    {
        if (!target) return;
        float mx = (Input.mousePosition.x / Screen.width) - 0.5f;
        float my = (Input.mousePosition.y / Screen.height) - 0.5f;

        Vector3 targetMouseOffset = new Vector3(mx, 0, my) * mouseInfluence;

        mouseOffset = Vector3.Lerp(
            mouseOffset,
            targetMouseOffset,
            Time.deltaTime * mouseSmooth
        );

        Vector3 targetPos = target.position + offset + mouseOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            followSmoothTime
        );
    }
}
