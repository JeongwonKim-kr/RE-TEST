using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MonsterEndingChase_Simple : MonoBehaviour
{
    [Header("Monster Root")]
public GameObject monsterRoot;
[Header("Player Control")]
public MonoBehaviour playerController;

    [Header("Movement")]
    public float moveSpeed = 70f;
    public float stopDistance = 1f;
    public Transform player;

    [Header("Black Screen")]
    public RawImage blackImage;
    public float fadeOutSpeed = 1.5f;
    public float blackHoldTime = 5f;

    [Header("Sound")]
    public AudioSource blackAudioSource;
    public AudioClip killSound;

    [Header("BGM")]
    public AudioSource bgmSource;
    public AudioClip newBGM;
    public float bgmFadeSpeed = 1f;

    [Header("Objects To Enable")]
    public GameObject[] objectsToEnable;

    private bool isChasing = true;
    private bool sequenceStarted = false;

    void Start()
    {
        if (blackImage != null)
        {
            SetAlpha(blackImage, 0f);
            blackImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isChasing || sequenceStarted) return;
        if (player == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * Time.deltaTime
        );

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < stopDistance)
        {
            StartCoroutine(KillSequence());
        }
    }

    IEnumerator KillSequence()
    {
        sequenceStarted = true;
        isChasing = false;

        Vector3 dir = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir);

        // Black screen instantly
blackImage.gameObject.SetActive(true);
SetAlpha(blackImage, 1f);

// 👻 Disable monster root immediately
monsterRoot.SetActive(false);

if (playerController != null)
    playerController.enabled = false;

        // Play kill sound
        if (blackAudioSource && killSound)
            blackAudioSource.PlayOneShot(killSound);

        // Fade out BGM
        if (bgmSource != null)
            StartCoroutine(FadeOutBGM());

        yield return new WaitForSeconds(blackHoldTime);

        // 👻 ENABLE objects instead of spawning
        EnableObjects();
if (playerController != null)
    playerController.enabled = true;
        // Fade out black screen
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * fadeOutSpeed;
            SetAlpha(blackImage, t);
            yield return null;
        }

        blackImage.gameObject.SetActive(false);

        // Start new BGM
        if (bgmSource != null && newBGM != null)
        {
            bgmSource.clip = newBGM;
            bgmSource.volume = 0.4f;
            bgmSource.Play();
        }
// ✅ Re-enable player control


        this.enabled = false;
    }

    void EnableObjects()
    {
        if (objectsToEnable == null) return;

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }

    IEnumerator FadeOutBGM()
    {
        while (bgmSource != null && bgmSource.volume > 0f)
        {
            bgmSource.volume -= Time.deltaTime * bgmFadeSpeed;
            yield return null;
        }

        if (bgmSource != null)
            bgmSource.Stop();
    }

    void SetAlpha(RawImage img, float a)
    {
        if (img == null) return;

        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
