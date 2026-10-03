using UnityEngine;
using UnityEngine.UI;

public class KeycardDoorSystem : MonoBehaviour
{
    public enum KeyType { None, Red, Blue, Green }

    [Header("Player")]
    public Transform player;

    [Header("Keys")]
    public GameObject redKey;
    public GameObject blueKey;
    public GameObject greenKey;

    [Header("Doors")]
    public Transform redDoor;
    public Transform blueDoor;
    public Transform greenDoor;

    [Header("UI Positions")]
    public Transform redKeyUI;
    public Transform blueKeyUI;
    public Transform greenKeyUI;

    public Transform redDoorUI;
    public Transform blueDoorUI;
    public Transform greenDoorUI;

    [Header("UI")]
    public RawImage pressEImage;
    public RawImage warningImage;
    public Camera cam;
    public float fadeSpeed = 4f;
    public float interactDistance = 1f; 

    [Header("Door Settings")]
    public float doorMoveDistance = -3f;
    public float doorSpeed = 0.3f; 

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip keyPickupSound;
    public AudioClip doorOpenSound;

    private KeyType currentKey = KeyType.None;

    private bool redOpened, blueOpened, greenOpened;

    private Vector3 redTarget, blueTarget, greenTarget;

    private float warningAlpha = 0f;

    void Start()
    {

        redKey.SetActive(true);
        blueKey.SetActive(false);
        greenKey.SetActive(false);

        redTarget = redDoor.position + Vector3.up * doorMoveDistance;
        blueTarget = blueDoor.position + Vector3.up * doorMoveDistance;
        greenTarget = greenDoor.position + Vector3.up * doorMoveDistance;

        SetAlpha(pressEImage, 0f);
        SetAlpha(warningImage, 0f);
    }

    void Update()
    {
        bool isInteracting = false;

        if (HandleKey(redKey, redKeyUI, KeyType.Red)) isInteracting = true;
        else if (HandleKey(blueKey, blueKeyUI, KeyType.Blue)) isInteracting = true;
        else if (HandleKey(greenKey, greenKeyUI, KeyType.Green)) isInteracting = true;


        if (!isInteracting)
        {
            if (HandleDoor(redDoorUI, KeyType.Red, ref redOpened, blueKey)) isInteracting = true;
            else if (HandleDoor(blueDoorUI, KeyType.Blue, ref blueOpened, greenKey)) isInteracting = true;
            else if (HandleDoor(greenDoorUI, KeyType.Green, ref greenOpened, null)) isInteracting = true;
        }


        if (!isInteracting)
        {
            FadeUI(false);
        }

        MoveDoors();
        UpdateWarningUI();
    }


    bool HandleKey(GameObject key, Transform uiPos, KeyType type)
    {
        if (key == null || !key.activeSelf) return false;

        float dist = Vector3.Distance(player.position, key.transform.position);

        if (dist > interactDistance || currentKey != KeyType.None)
            return false;

        // FOLLOW UI
        pressEImage.transform.position = cam.WorldToScreenPoint(uiPos.position);

        FadeUI(true);

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentKey = type;
            key.SetActive(false);

            if (audioSource && keyPickupSound)
                audioSource.PlayOneShot(keyPickupSound);
        }

        return true;
    }


    bool HandleDoor(Transform uiPos, KeyType needed, ref bool opened, GameObject nextKey)
    {
        if (opened) return false;

        float dist = Vector3.Distance(player.position, uiPos.position);

        if (dist > interactDistance)
            return false;


        pressEImage.transform.position = cam.WorldToScreenPoint(uiPos.position);

        FadeUI(true);

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentKey == needed)
            {
                opened = true;
                currentKey = KeyType.None;

                if (audioSource && doorOpenSound)
                    audioSource.PlayOneShot(doorOpenSound);

                if (nextKey != null)
                    nextKey.SetActive(true);
            }
            else
            {
                warningAlpha = 1f;
            }
        }

        return true;
    }

 
    void MoveDoors()
    {
        if (redOpened)
            redDoor.position = Vector3.Lerp(redDoor.position, redTarget, Time.deltaTime * doorSpeed);

        if (blueOpened)
            blueDoor.position = Vector3.Lerp(blueDoor.position, blueTarget, Time.deltaTime * doorSpeed);

        if (greenOpened)
            greenDoor.position = Vector3.Lerp(greenDoor.position, greenTarget, Time.deltaTime * doorSpeed);
    }


    void FadeUI(bool show)
    {
        Color c = pressEImage.color;

        float speed = show ? fadeSpeed * 3f : fadeSpeed;
        float target = show ? 1f : 0f;

        c.a = Mathf.MoveTowards(c.a, target, speed * Time.deltaTime);

        pressEImage.color = c;
    }


void UpdateWarningUI()
{

    warningAlpha = Mathf.MoveTowards(warningAlpha, 0f, 3f * Time.deltaTime);

    Color c = warningImage.color;

 
    c.a = Mathf.Lerp(c.a, warningAlpha, Time.deltaTime * 4f);

    warningImage.color = c;
}


    void SetAlpha(RawImage img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
