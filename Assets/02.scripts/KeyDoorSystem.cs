using UnityEngine;
using UnityEngine.UI;

public class KeyDoorSystem : MonoBehaviour
{
    [Header("References")]
    public Collider invisibleWall;
    public GameObject keyObject;
    public Transform keySpawnPoint;
private bool doorDisabled = false;

    public Transform doorHinge;   
    public Transform doorUIPos;   
    public Transform player;

    [Header("UI")]
    public RawImage pressEImage;
    public RawImage warningImage;
    public Camera cam;
    public float fadeSpeed = 4f;

    [Header("Warning UI")]
    public float warningFadeInSpeed = 10f;
    public float warningFadeOutSpeed = 2f;

    [Header("Settings")]
    public float interactDistance = 1f;

    [Header("Door")]
    public float doorOpenAngle = -90f;
    public float doorRotateSpeed = 3.5f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip keySpawnSound;
    public AudioClip keyPickupSound;
    public AudioClip doorOpenSound;

    private bool keySpawned = false;
    private bool hasKey = false;
    private bool doorOpened = false;

    private float warningAlpha = 0f;
    private Quaternion startRot;

    void Start()
    {
        keyObject.SetActive(false);

        startRot = doorHinge.localRotation;

        Color c = pressEImage.color;
        c.a = 0f;
        pressEImage.color = c;

        Color w = warningImage.color;
        w.a = 0f;
        warningImage.color = w;

        if (invisibleWall != null)
            invisibleWall.isTrigger = true;
    }

    void Update()
    {
        bool showUI = false;

        if (keySpawned && !hasKey)
        {
            float keyDist = Vector3.Distance(player.position, keyObject.transform.position);

            if (keyDist < interactDistance)
            {
                showUI = true;

                Vector3 screenPos = cam.WorldToScreenPoint(keyObject.transform.position);
                pressEImage.transform.position = screenPos;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    hasKey = true;
                    keyObject.SetActive(false);

                    audioSource.PlayOneShot(keyPickupSound);
                }
            }
        }

        // ❌ Don't show UI if door already opened
if (!doorOpened)
{
    float doorDist = Vector3.Distance(player.position, doorUIPos.position);

    if (doorDist < interactDistance)
    {
        showUI = true;

        Vector3 screenPos = cam.WorldToScreenPoint(doorUIPos.position);
        pressEImage.transform.position = screenPos;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (hasKey)
            {
                doorOpened = true;
                audioSource.PlayOneShot(doorOpenSound);
            }
            else
            {
                warningAlpha = 1f;
            }
        }
    }
}


        FadeUI(showUI);
        UpdateWarningUI();
        RotateDoor();
    }

    void RotateDoor()
{
    if (!doorOpened || doorDisabled) return;

    Quaternion targetRot = startRot * Quaternion.Euler(0, 0, doorOpenAngle);

    doorHinge.localRotation = Quaternion.Slerp(
        doorHinge.localRotation,
        targetRot,
        Time.deltaTime * doorRotateSpeed
    );

    // ✅ Check if door finished rotating
    if (Quaternion.Angle(doorHinge.localRotation, targetRot) < 1f)
    {
        doorHinge.localRotation = targetRot;

        // 🔥 Disable door
        doorHinge.gameObject.SetActive(false);

        doorDisabled = true;
    }
}


    void UpdateWarningUI()
    {
        Color c = warningImage.color;

        // fade out slowly
        warningAlpha = Mathf.MoveTowards(warningAlpha, 0f, warningFadeOutSpeed * Time.deltaTime);

        // apply alpha (fast in, slow out)
        c.a = Mathf.Lerp(c.a, warningAlpha, Time.deltaTime * warningFadeInSpeed);

        warningImage.color = c;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !keySpawned)
        {
            SpawnKey();
        }
    }

    void SpawnKey()
    {
        keySpawned = true;
        keyObject.transform.position = keySpawnPoint.position;
        keyObject.SetActive(true);

        audioSource.PlayOneShot(keySpawnSound);
    }

    void FadeUI(bool show)
    {
        Color c = pressEImage.color;
        float targetAlpha = show ? 1f : 0f;
        c.a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
        pressEImage.color = c;
    }
}
