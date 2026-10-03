using UnityEngine;
using System.Collections;

public class MonsterChase2 : MonoBehaviour
{
    public GameObject monster;
    public Animator animator;

    public float moveSpeed = 1.1f;
    public float moveDistance = 20f;

    public AudioSource bgmSource;   
    public AudioClip normalBGMClip;
    public AudioClip chaseBGMClip;
    public AudioClip growlClip;
    public AudioClip spawnClip;

    public float fadeSpeed = 1.5f;

    AudioSource growlSource;
    AudioSource spawnSource;

    Vector3 targetPos;
    bool moving = false;

    void Awake()
    {
        growlSource = monster.AddComponent<AudioSource>();
        spawnSource = monster.AddComponent<AudioSource>();

        growlSource.spatialBlend = 0f;
        spawnSource.spatialBlend = 0f;
    }

    void Start()
    {
        monster.SetActive(false);

        if (bgmSource != null)
        {
            bgmSource.loop = true;
            bgmSource.clip = normalBGMClip;
            bgmSource.Play();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ActivateMonster();

        Debug.Log("CHASE STARTED");

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!moving) return;

        monster.transform.position = Vector3.MoveTowards(
            monster.transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(monster.transform.position, targetPos) < 0.05f)
        {
            moving = false;

            if (animator != null)
                animator.SetBool("isRunning", false);
        }
    }

    void ActivateMonster()
    {
        monster.SetActive(true);

        Vector3 forwardDir = monster.transform.forward.normalized;
        targetPos = monster.transform.position + forwardDir * moveDistance;

        moving = true;

        if (animator != null)
            animator.SetBool("isRunning", true);

        if (spawnClip != null)
            spawnSource.PlayOneShot(spawnClip);

        if (growlClip != null)
            growlSource.PlayOneShot(growlClip);

        StartCoroutine(SwitchToChaseBGM());
        bgmSource.Stop();

        bgmSource.clip = chaseBGMClip;
        bgmSource.time = 0f;
        bgmSource.volume = 1f;
        bgmSource.loop = true;
        bgmSource.Play();

        Debug.Log("Now playing: " + bgmSource.clip.name);
    }

    IEnumerator SwitchToChaseBGM()
    {
        yield return StartCoroutine(FadeOut(bgmSource));

        bgmSource.clip = chaseBGMClip;
        bgmSource.Play();

        yield return StartCoroutine(FadeIn(bgmSource));
    }

    public void StopChase()
    {
        StartCoroutine(SwitchToNormalBGM());
    }

    IEnumerator SwitchToNormalBGM()
    {
        yield return StartCoroutine(FadeOut(bgmSource));

        bgmSource.clip = normalBGMClip;
        bgmSource.Play();

        yield return StartCoroutine(FadeIn(bgmSource));
    }

    IEnumerator FadeOut(AudioSource source)
    {
        while (source.volume > 0f)
        {
            source.volume -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        source.volume = 0f;
    }

    IEnumerator FadeIn(AudioSource source)
    {
        while (source.volume < 1f)
        {
            source.volume += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        source.volume = 1f;
    }
}
