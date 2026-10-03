using UnityEngine;
using UnityEngine.UI;

public class LeverSwitch : MonoBehaviour
{
    public Transform player;
    public Transform lever;

    public float interactDistance = 1f;
    public float rotateAmount = 80f;
    public float rotateSpeed = 5f;

    public Transform door;
    public float doorMoveDistance = -2.28f;
    public float doorMoveSpeed = 0.14f;

    Vector3 doorStartPos;
    Vector3 doorTargetPos;
    bool doorMoving = false;

    public RawImage pressEImage;
    public Camera cam;
    public float fadeSpeed = 4f;

    public AudioSource audioSource;
    public AudioClip leverSound;
    public AudioClip doorSound;

    bool activated = false;
    bool soundPlayed = false;
    float targetAngle = -40f;

    void Start()
    {
        doorStartPos = door.position;
        doorTargetPos = doorStartPos + new Vector3(0f, doorMoveDistance, 0f);

        Color c = pressEImage.color;
        c.a = 0f;
        pressEImage.color = c;
    }

    void Update()
    {
        float dist = Vector3.Distance(player.position, lever.position);

        if (!activated && dist <= interactDistance)
        {
            FadeUI(true);

            Vector3 screenPos = cam.WorldToScreenPoint(lever.position);
            pressEImage.transform.position = screenPos;

            if (Input.GetKeyDown(KeyCode.E))
            {
                activated = true;
                targetAngle = rotateAmount;
            }
        }
        else
        {
            FadeUI(false);
        }

        Vector3 currentRot = lever.localEulerAngles;
        float newX = Mathf.LerpAngle(currentRot.x, targetAngle, Time.deltaTime * rotateSpeed);
        lever.localEulerAngles = new Vector3(newX, currentRot.y, currentRot.z);

        if (activated && !soundPlayed && Mathf.Abs(newX - rotateAmount) < 1f)
        {
            audioSource.PlayOneShot(leverSound);
            soundPlayed = true;
            Invoke(nameof(StartDoor), leverSound.length);
        }

        if (doorMoving)
        {
            door.position = Vector3.MoveTowards(
                door.position,
                doorTargetPos,
                doorMoveSpeed * Time.deltaTime
            );
        }
    }

    void StartDoor()
    {
        audioSource.PlayOneShot(doorSound);
        doorMoving = true;
    }

    void FadeUI(bool show)
    {
        Color c = pressEImage.color;
        float targetAlpha = show ? 1f : 0f;
        c.a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
        pressEImage.color = c;
    }
}
