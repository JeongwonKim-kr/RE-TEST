using UnityEngine;
using UnityEngine.UI;

public class DoorController : MonoBehaviour
{
    [Header("References")]
    public Transform doorHinge;
    public Transform doorUIPos;
    public Transform player;

    [Header("UI")]
    public RawImage pressEImage;
    public Camera cam;
    public float fadeSpeed = 4f;

    [Header("Settings")]
    public float interactDistance = 1f;

    [Header("Door")]
    public float doorOpenAngle = -90f;
    public float doorRotateSpeed = 3.5f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip doorOpenSound;

    private bool doorOpened = false;
    private bool doorDisabled = false;

    private Quaternion startRot;

    void Start()
    {
        startRot = doorHinge.localRotation;

        Color c = pressEImage.color;
        c.a = 0f;
        pressEImage.color = c;
    }

    void Update()
    {
        bool showUI = false;

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
                    doorOpened = true;
                    audioSource.PlayOneShot(doorOpenSound);
                }
            }
        }

        FadeUI(showUI);
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

        if (Quaternion.Angle(doorHinge.localRotation, targetRot) < 1f)
        {
            doorHinge.localRotation = targetRot;
            doorDisabled = true;
        }
    }

    void FadeUI(bool show)
    {
        Color c = pressEImage.color;
        float targetAlpha = show ? 1f : 0f;
        c.a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
        pressEImage.color = c;
    }
}
