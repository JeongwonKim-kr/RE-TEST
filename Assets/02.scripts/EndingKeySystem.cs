using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EndingKeySystem : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera playerCam;

    [Header("Key")]
    public GameObject keyObject;

    [Header("Key Slot")]
    public Transform keySlot;
    public float interactDistance = 1f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Player Swap")]
    public GameObject originalPlayer;
    public GameObject deadPlayer;

    [Header("UI")]
    public RawImage eUI;
    public RawImage warningImage;
    public Vector3 uiOffset = Vector3.zero;

    [Header("Black Screen")]
    public RawImage blackImage;
    public float blackFadeSpeed = 4f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip pickupSound;
    public AudioClip insertSound;
    public AudioClip creditsStartSound;

    [Header("Stop Sounds On Black")]
    public AudioSource[] soundsToStop;

    [Header("Ending Credits")]
    public TMP_Text creditsText;
    public float waitBeforeCredits = 5f;   // Wait 5 sec after black screen before moving credits
    public float creditsDuration = 25f;    // Duration of credits scroll
    public float waitAfterCredits = 5f;    // Wait 5 sec after credits before scene change
    public string nextSceneName;

    private bool hasKey = false;
    private bool keyUsed = false;
    private bool isBlackScreen = false;
    private float warningAlpha = 0f;
void Awake()
{
    // Force everything invisible BEFORE first frame

    if (blackImage != null)
    {
        SetAlpha(blackImage, 0f);
        blackImage.gameObject.SetActive(false);
    }

    if (creditsText != null)
    {
        creditsText.gameObject.SetActive(false);
    }

    if (eUI != null)
    {
        SetAlpha(eUI, 0f);
    }

    if (warningImage != null)
    {
        SetAlpha(warningImage, 0f);
    }
}

    void Start()
    {
        // UI setup
        if (eUI != null) SetAlpha(eUI, 0f);
        if (warningImage != null) SetAlpha(warningImage, 0f);
        if (blackImage != null) { SetAlpha(blackImage, 0f); blackImage.gameObject.SetActive(false); }
        if (creditsText != null) creditsText.gameObject.SetActive(false);

        // Player setup
        if (originalPlayer != null) originalPlayer.SetActive(true);
        if (deadPlayer != null) deadPlayer.SetActive(false);
    }

    void Update()
    {
        if (isBlackScreen) return; // disable interaction during black screen
        if (player == null) return;

        bool showUI = false;
        Vector3 targetPos = Vector3.zero;

        // 🔑 KEY INTERACTION
        if (!hasKey && keyObject != null && keyObject.activeSelf)
        {
            float keyDist = Vector3.Distance(player.position, keyObject.transform.position);
            if (keyDist < interactDistance)
            {
                showUI = true;
                targetPos = keyObject.transform.position;
                if (Input.GetKeyDown(interactKey)) PickupKey();
            }
        }
        // 🔐 SLOT INTERACTION
        else if (!keyUsed && keySlot != null)
        {
            float slotDist = Vector3.Distance(player.position, keySlot.position);
            if (slotDist < interactDistance)
            {
                showUI = true;
                targetPos = keySlot.position;
                if (Input.GetKeyDown(interactKey))
                {
                    if (hasKey) StartCoroutine(InsertKey());
                    else warningAlpha = 1f; // ❗ NO KEY WARNING

                    // Hide the E UI immediately after interaction
                    if (eUI != null) SetAlpha(eUI, 0f);
                }
            }
        }

        // 🔥 UPDATE E UI
        if (eUI != null)
        {
            Vector3 worldPos = targetPos + uiOffset;
            Vector3 screenPos = playerCam.WorldToScreenPoint(worldPos);
            FadeUI(eUI, showUI && screenPos.z > 0);
            if (showUI) eUI.transform.position = screenPos;
        }

        UpdateWarningUI();
    }

    void PickupKey()
    {
        hasKey = true;
        if (keyObject != null) keyObject.SetActive(false);
        if (audioSource && pickupSound) audioSource.PlayOneShot(pickupSound);

        // Hide E UI after picking up key
        if (eUI != null) SetAlpha(eUI, 0f);
    }

    IEnumerator InsertKey()
    {
        keyUsed = true;
        isBlackScreen = true;

        // Disable player and hide E UI
        if (player != null) player.gameObject.SetActive(false);
        if (eUI != null) SetAlpha(eUI, 0f);

        // Play insert sound
        if (audioSource && insertSound) audioSource.PlayOneShot(insertSound);

        // ⚫ Fade in black screen
        if (blackImage != null)
        {
            blackImage.gameObject.SetActive(true);
            Color c = blackImage.color;
            c.a = 0f;
            blackImage.color = c;

            while (c.a < 1f)
            {
                c.a += Time.deltaTime * blackFadeSpeed;
                blackImage.color = c;
                yield return null;
            }
        }

        // 🔇 Stop other sounds
        foreach (AudioSource src in soundsToStop)
            if (src != null) src.Stop();

        // ⏳ Wait before credits
        yield return new WaitForSeconds(waitBeforeCredits);

        // 🔊 Play credits sound
        if (audioSource != null && creditsStartSound != null)
            audioSource.PlayOneShot(creditsStartSound);

        // 🎬 Show credits text and scroll
        if (creditsText != null)
        {
            creditsText.gameObject.SetActive(true);
            creditsText.rectTransform.anchoredPosition = new Vector2(0f, -250f); // bottom
            Vector2 endPos = new Vector2(0f, 2250f); // top

            float timer = 0f;
            while (timer < creditsDuration)
            {
                timer += Time.deltaTime;
                float t = timer / creditsDuration;
                creditsText.rectTransform.anchoredPosition = Vector2.Lerp(
                    new Vector2(0f, -250f),
                    endPos,
                    t
                );
                yield return null;
            }
        }

        // ⏳ Wait after credits
        yield return new WaitForSeconds(waitAfterCredits);

        // 🔄 Load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    void UpdateWarningUI()
    {
        if (warningImage == null) return;
        Color c = warningImage.color;
        warningAlpha = Mathf.MoveTowards(warningAlpha, 0f, 2f * Time.deltaTime);
        c.a = Mathf.Lerp(c.a, warningAlpha, Time.deltaTime * 10f);
        warningImage.color = c;
    }

    void FadeUI(RawImage img, bool show)
    {
        if (img == null) return;
        Color c = img.color;
        float target = show ? 1f : 0f;
        c.a = Mathf.MoveTowards(c.a, target, 6f * Time.deltaTime);
        img.color = c;
    }

    void SetAlpha(RawImage img, float a)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
