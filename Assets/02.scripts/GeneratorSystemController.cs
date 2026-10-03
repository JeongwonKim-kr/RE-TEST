using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GeneratorSystemController : MonoBehaviour
{
    [Header("PLAYER")]
    public Transform player;
    public Camera cam;

    [Header("GLOBAL UI")]
    public TextMeshProUGUI generatorText;
    public float globalFadeSpeed = 2f;

    [Header("PRESS E UI")]
    public RawImage pressEImage;
    public float uiFadeSpeed = 4f;

    [Header("DOOR")]
    public Transform door;
    public float doorMoveDistance = 5f;
    public float doorMoveSpeed = 0.85f;
    public AudioSource doorAudio;
    public AudioClip doorMoveSound;

    [Header("GENERATOR SETTINGS")]
    public int totalGenerators = 8;
    public Transform[] generatorPositions;

    [Header("SOUNDS")]
    public AudioSource audioSource;
    public AudioClip pressSound;
    public AudioClip generatorOnSound;

    [Header("ENGINE SOUND")]
    public AudioSource engineAudio;

    public float interactDistance = 1.5f;
    public float pressCooldown = 0.7f;

    private int[] requiredPresses;
    private int[] currentPresses;
    private bool[] isOn;
    private float lastPressTime;

    private int currentGenerators = 0;
    private bool playerInside = false;

    private Vector3 doorStart;
    private Vector3 doorTarget;
    private bool doorOpening = false;

    private bool enginePlayed = false;
    private bool fadeOutText = false;

    void Start()
    {
        doorStart = door.position;
        doorTarget = doorStart + Vector3.down * doorMoveDistance;

        generatorText.text = "0/" + totalGenerators + " generators activated";

        SetTextAlpha(0f);

        requiredPresses = new int[generatorPositions.Length];
        currentPresses = new int[generatorPositions.Length];
        isOn = new bool[generatorPositions.Length];

        for (int i = 0; i < generatorPositions.Length; i++)
        {
            requiredPresses[i] = Random.Range(4, 11);
            currentPresses[i] = 0;
            isOn[i] = false;
        }

        SetImageAlpha(pressEImage, 0f);
    }

    void Update()
    {
        HandleGlobalUI();
        HandleGenerators();
        HandleDoor();

 
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    // 🔲 GLOBAL UI FADE
    void HandleGlobalUI()
    {
        float target;

        if (fadeOutText)
            target = 0f;
        else
            target = playerInside ? 1f : 0f;

        Color c = generatorText.color;
        c.a = Mathf.MoveTowards(c.a, target, globalFadeSpeed * Time.deltaTime);
        generatorText.color = c;
    }

    // ⚡ GENERATOR SYSTEM
    void HandleGenerators()
    {
        int closestIndex = -1;
        float closestDist = interactDistance;

        for (int i = 0; i < generatorPositions.Length; i++)
        {
            if (isOn[i]) continue;

            float dist = Vector3.Distance(player.position, generatorPositions[i].position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        bool showUI = closestIndex != -1;

        if (showUI)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(generatorPositions[closestIndex].position);

            if (screenPos.z > 0)
                pressEImage.transform.position = screenPos;
        }

        if (showUI && Input.GetKeyDown(KeyCode.E))
        {
            if (Time.time - lastPressTime > pressCooldown)
            {
                lastPressTime = Time.time;

                currentPresses[closestIndex]++;
                audioSource.PlayOneShot(pressSound);

                if (currentPresses[closestIndex] >= requiredPresses[closestIndex])
                {
                    ActivateGenerator(closestIndex);
                }
            }
        }

        FadeImage(pressEImage, showUI);
    }

    void ActivateGenerator(int index)
    {
        isOn[index] = true;

        audioSource.PlayOneShot(generatorOnSound);

        currentGenerators++;
        generatorText.text = currentGenerators + "/" + totalGenerators + " generators activated";

        if (!enginePlayed)
        {
            engineAudio.Play();
            enginePlayed = true;
        }

        if (currentGenerators >= totalGenerators)
        {
            StartCoroutine(FadeOutDelay());
            OpenDoor();
        }
    }

    // 🧪 TEST FUNCTION
    void TestAddGenerator()
    {
        if (currentGenerators >= totalGenerators) return;

        currentGenerators++;
        generatorText.text = currentGenerators + "/" + totalGenerators + " generators activated";

        if (!enginePlayed)
        {
            engineAudio.Play();
            enginePlayed = true;
        }

        if (currentGenerators >= totalGenerators)
        {
            StartCoroutine(FadeOutDelay());
            OpenDoor();
        }
    }

    IEnumerator FadeOutDelay()
    {
        yield return new WaitForSeconds(1.2f);
        fadeOutText = true;
    }

    // 🚪 DOOR
    void OpenDoor()
    {
        if (doorOpening) return;

        doorOpening = true;
        doorAudio.PlayOneShot(doorMoveSound);
    }

    void HandleDoor()
    {
        if (!doorOpening) return;

        door.position = Vector3.MoveTowards(
            door.position,
            doorTarget,
            doorMoveSpeed * Time.deltaTime
        );
    }

    // 🎨 UI HELPERS
    void FadeImage(RawImage img, bool show)
    {
        Color c = img.color;
        float target = show ? 1f : 0f;
        c.a = Mathf.MoveTowards(c.a, target, uiFadeSpeed * Time.deltaTime);
        img.color = c;
    }

    void SetImageAlpha(RawImage img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    void SetTextAlpha(float a)
    {
        Color c = generatorText.color;
        c.a = a;
        generatorText.color = c;
    }
}
