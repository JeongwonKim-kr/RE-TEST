using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator anim;

    public float walkSpeed = 0.7f;
    public float sprintSpeed = 1.5f;
    public float rotateSpeed = 10f;

    public float walkShakeAmount = 0.07f;
    public float sprintShakeAmount = 0.1f;

    public float walkShakeSpeed = 5f;
    public float sprintShakeSpeed = 7f;

    public Camera cam;
    public Transform flashlight;

    [Header("Footsteps")]
    public AudioSource footstepSource;
    public AudioClip[] footstepClips;

    public float walkStepDelay = 0.6f;
    public float sprintStepDelay = 0.35f;

    Quaternion flashlightStartRot;

    float stepTimer = 0f;
    int lastStepIndex = -1;
    private Rigidbody rb;

    void Start()
    {
        transform.position = new Vector3(0f, 6f, -2f);//0f, 6f, -2f for spawn//28.41f, 6f, 17.54f for boss chase place 1

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        flashlightStartRot = flashlight.localRotation;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        float speed = sprinting ? sprintSpeed : walkSpeed;

        if (sprinting)
            anim.speed = 1.5f;
        else
            anim.speed = 1f;

        Vector3 moveDir = new Vector3(h, 0, v).normalized;
        if (moveDir == Vector3.zero)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (moveDir != Vector3.zero)
            anim.SetInteger("AnimationPar", 1);
        else
            anim.SetInteger("AnimationPar", 0);

        LookAtMouse();

        if (moveDir != Vector3.zero)
        {
            transform.position += moveDir * speed * Time.deltaTime;
        }

        FlashlightShake(moveDir, sprinting);
        HandleFootsteps(moveDir, sprinting);
    }

    void LookAtMouse()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, transform.position);

        float dist;

        if (ground.Raycast(ray, out dist))
        {
            Vector3 point = ray.GetPoint(dist);
            Vector3 dir = point - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }
        }
    }

    void FlashlightShake(Vector3 moveDir, bool sprinting)
    {
        if (moveDir != Vector3.zero)
        {
            float shakeSpeed = sprinting ? sprintShakeSpeed : walkShakeSpeed;
            float shakeAmount = sprinting ? sprintShakeAmount : walkShakeAmount;

            float tiltX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * shakeAmount * 60f;
            float tiltY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * shakeAmount * 60f;
            float tiltZ = (Mathf.PerlinNoise(Time.time * shakeSpeed, Time.time * shakeSpeed) - 0.5f) * shakeAmount * 60f;

            flashlight.localRotation = flashlightStartRot * Quaternion.Euler(tiltX, tiltY, tiltZ);
        }
        else
        {
            flashlight.localRotation = flashlightStartRot;
        }
    }

    void HandleFootsteps(Vector3 moveDir, bool sprinting)
    {
        if (moveDir != Vector3.zero)
        {
            float delay = sprinting ? sprintStepDelay : walkStepDelay;

            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = delay;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        int index;

        do
        {
            index = Random.Range(0, footstepClips.Length);
        }
        while (index == lastStepIndex && footstepClips.Length > 1);

        lastStepIndex = index;

        footstepSource.PlayOneShot(footstepClips[index]);
    }
}
