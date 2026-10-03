using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PaperInteract : MonoBehaviour
{
    public Transform paperUIPos;
    public Transform player;
    public Camera cam;

    public RawImage pressEImage;
    public RawImage paperImage;
    public RawImage backgroundImage;

    public TMP_Text exitText;

    public float fadeSpeed = 4f;
    public float interactDistance = 1f;

    public AudioSource audioSource;
    public AudioClip paperOpenSound;

    public MonoBehaviour playerMovement;
    public MonoBehaviour playerLook;

    public float inputCooldown = 0.5f;
    private float nextInputTime = 0f;

    private bool paperOpened = false;

    void Start()
    {
        SetAlpha(pressEImage, 0f);
        SetAlpha(paperImage, 0f);
        SetAlpha(backgroundImage, 0f);
        SetTextAlpha(exitText, 0f);
    }

    void Update()
    {
        float dist = Vector3.Distance(player.position, paperUIPos.position);
        bool showUI = dist < interactDistance && !paperOpened;

        if (showUI)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(paperUIPos.position);
            pressEImage.transform.position = screenPos;
        }

        if (showUI && Input.GetKeyDown(KeyCode.E) && Time.time >= nextInputTime)
        {
            nextInputTime = Time.time + inputCooldown;

            paperOpened = true;
            audioSource.PlayOneShot(paperOpenSound);

            playerMovement.enabled = false;
            playerLook.enabled = false;
        }

        if (paperOpened && Input.GetKeyDown(KeyCode.Space) && Time.time >= nextInputTime)
        {
            nextInputTime = Time.time + inputCooldown;

            paperOpened = false;

            playerMovement.enabled = true;
            playerLook.enabled = true;
        }

        FadeUI(pressEImage, showUI);
        FadeUI(paperImage, paperOpened);
        FadeUI(backgroundImage, paperOpened, 1f);

        FadeText(exitText, paperOpened);
    }

    void FadeUI(RawImage img, bool show, float maxAlpha = 1f)
    {
        Color c = img.color;
        float target = show ? maxAlpha : 0f;
        c.a = Mathf.MoveTowards(c.a, target, fadeSpeed * Time.deltaTime);
        img.color = c;
    }

    void SetAlpha(RawImage img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    void FadeText(TMP_Text txt, bool show)
    {
        Color c = txt.color;
        float target = show ? 1f : 0f;
        c.a = Mathf.MoveTowards(c.a, target, fadeSpeed * Time.deltaTime);
        txt.color = c;
    }

    void SetTextAlpha(TMP_Text txt, float a)
    {
        Color c = txt.color;
        c.a = a;
        txt.color = c;
    }
}
