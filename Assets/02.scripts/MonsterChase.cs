using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class MonsterChase : MonoBehaviour
{
    public float moveSpeed = 1.1f;
    public float moveDistance = 20f;
    public float bgmFadeSpeed = 1f;

    public Animator animator;
    public string runBool = "isRunning";
    public string jumpscareTrigger = "Jumpscare";
    public float jumpscareAnimSpeed = 0.6f;
    public MonoBehaviour playerController;

    public Transform player;
    public Camera playerCam;
    public MonoBehaviour cameraController;
    public Transform facePoint;

    public float stopDistance = 1f;
    public float camMoveSpeed = 4f;
    public float camDistanceFromFace = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpscareSound;
    public AudioClip clawSound;
    public AudioSource bgmSource;

    public AudioSource[] soundsToDisable;

    public RawImage clawImage;
    public RawImage blackImage;
    public float uiFadeSpeed = 6f;
    public float clawDelay = 0.7f;

    public float fallDelay = 1f;
    public float fallSpeed = 3f;
    public float fallRotateSpeed = 2f;
    public float clawFadeInSpeed = 10f;
    public float clawFadeOutSpeed = 4f;
    public float clawStayTime = 0.3f;

    [Header("Post Processing")]
    public PostProcessVolume volume;

    Vignette vignette;
    Grain grain;
    ChromaticAberration chromatic;

    [Header("Chase Effects")]
    public float maxEffectDistance = 10f;
    public float maxVignette = 0.45f;
    public float maxGrain = 0.3f;
    public float maxChromatic = 0.15f;
    public float effectLerpSpeed = 2f;

    Vector3 startPos;
    Vector3 targetPos;

    bool moving = false;
    bool jumpscareTriggered = false;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + transform.forward * moveDistance;

        moving = true;

        if (animator != null)
            animator.SetBool(runBool, true);

        if (clawImage != null)
        {
            SetAlpha(clawImage, 0f);
            clawImage.gameObject.SetActive(false);
        }

        if (blackImage != null)
        {
            SetAlpha(blackImage, 0f);
            blackImage.gameObject.SetActive(false);
        }

        if (volume != null)
        {
            volume.profile.TryGetSettings(out vignette);
            volume.profile.TryGetSettings(out grain);
            volume.profile.TryGetSettings(out chromatic);
        }
    }

    void Update()
    {
        if (jumpscareTriggered) return;

        if (moving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                moving = false;

                if (animator != null)
                    animator.SetBool(runBool, false);
            }
        }

        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);

            if (dist < stopDistance)
            {
                TriggerJumpscare();
            }

            ApplyChaseEffects(dist);
            ApplyCameraShake(dist);
        }
    }

    void ApplyChaseEffects(float dist)
    {
        if (vignette == null) return;

        float t = Mathf.Clamp01(1f - (dist / maxEffectDistance));

        float pulse = Mathf.Sin(Time.time * 8f) * 0.05f * t;

        vignette.intensity.value = Mathf.Lerp(
            vignette.intensity.value,
            t * maxVignette + pulse,
            Time.deltaTime * effectLerpSpeed
        );

        grain.intensity.value = Mathf.Lerp(
            grain.intensity.value,
            t * maxGrain,
            Time.deltaTime * effectLerpSpeed
        );

        chromatic.intensity.value = Mathf.Lerp(
            chromatic.intensity.value,
            t * maxChromatic,
            Time.deltaTime * effectLerpSpeed
        );
    }

    void ApplyCameraShake(float dist)
    {
        float t = Mathf.Clamp01(1f - (dist / maxEffectDistance));
        float shake = 0.05f * t;

        playerCam.transform.position += new Vector3(
            Random.Range(-shake, shake),
            Random.Range(-shake, shake),
            0
        );
    }

    void OnTriggerEnter(Collider other)
    {
        if (jumpscareTriggered) return;

        if (other.CompareTag("Player"))
        {
            TriggerJumpscare();
        }
    }

    void TriggerJumpscare()
    {
        jumpscareTriggered = true;
        moving = false;

        foreach (AudioSource a in soundsToDisable)
        {
            if (a != null)
                a.Stop();
        }

        Vector3 dir = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir);

        if (animator != null)
        {
            animator.SetBool(runBool, false);
            animator.SetTrigger(jumpscareTrigger);
            animator.speed = jumpscareAnimSpeed;
        }

        if (playerController != null)
            playerController.enabled = false;

        if (bgmSource != null)
            StartCoroutine(FadeOutBGM());

        if (audioSource != null && jumpscareSound != null)
            audioSource.PlayOneShot(jumpscareSound);

        // 🔥 MAX OUT EFFECTS
        if (vignette != null) vignette.intensity.value = 0.6f;
        if (grain != null) grain.intensity.value = 0.5f;
        if (chromatic != null) chromatic.intensity.value = 0.3f;

        StartCoroutine(JumpscareUI());
        StartCoroutine(JumpscareCamera());
    }

    IEnumerator JumpscareCamera()
    {
        if (cameraController != null)
            cameraController.enabled = false;

        float t = 0f;

        Vector3 startPos = playerCam.transform.position;
        Quaternion startRot = playerCam.transform.rotation;

        Vector3 targetPos = facePoint.position + facePoint.forward * camDistanceFromFace;
        Quaternion targetRot = Quaternion.LookRotation((facePoint.position - targetPos).normalized);

        while (t < 1f)
        {
            t += Time.deltaTime * camMoveSpeed;

            playerCam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            playerCam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        yield return new WaitForSeconds(fallDelay);
        yield return StartCoroutine(FallCamera());
    }


IEnumerator FallCamera()
{
    if (blackImage != null)
        blackImage.gameObject.SetActive(true);

    float fadeT = 0f;
    bool loaded = false; // prevents multiple loads

    Quaternion fallRot = playerCam.transform.rotation * Quaternion.Euler(80f, 0f, 20f);

    while (true)
    {
        playerCam.transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        playerCam.transform.rotation = Quaternion.Slerp(
            playerCam.transform.rotation,
            fallRot,
            Time.deltaTime * fallRotateSpeed
        );

        // Fade to black
        if (blackImage != null && fadeT < 1f)
        {
            fadeT += Time.deltaTime * (uiFadeSpeed * 0.5f);
            SetAlpha(blackImage, fadeT);
        }

        // ✅ When fully black → wait 3 sec → load scene
        if (!loaded && fadeT >= 1f)
        {
            loaded = true;
            StartCoroutine(LoadHomeAfterDelay());
        }

        yield return null;
    }
}


    IEnumerator JumpscareUI()
    {
        if (clawImage != null)
            clawImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(clawDelay);

        if (audioSource != null && clawSound != null)
            audioSource.PlayOneShot(clawSound);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * clawFadeInSpeed;
            SetAlpha(clawImage, t);
            yield return null;
        }

        yield return new WaitForSeconds(clawStayTime);

        while (t > 0f)
        {
            t -= Time.deltaTime * clawFadeOutSpeed;
            SetAlpha(clawImage, t);
            yield return null;
        }

        clawImage.gameObject.SetActive(false);
    }

    void SetAlpha(RawImage img, float a)
    {
        if (img == null) return;

        Color c = img.color;
        c.a = a;
        img.color = c;
    }

    IEnumerator FadeOutBGM()
    {
        while (bgmSource.volume > 0f)
        {
            bgmSource.volume -= Time.deltaTime * bgmFadeSpeed;
            yield return null;
        }

        bgmSource.Stop();
    }
    IEnumerator LoadHomeAfterDelay()
{
    yield return new WaitForSeconds(3f);
    UnityEngine.SceneManagement.SceneManager.LoadScene("GameHomeScreen");
}

}
