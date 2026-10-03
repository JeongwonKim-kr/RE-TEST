using UnityEngine;
using System.Collections;

public class LightGameController : MonoBehaviour
{
    [Header("Light")]
    public GameObject lightObject;
    public Transform lightTarget;
[Header("Random Speed")]
[Header("Random Speed")]
public float minSpeedMultiplier = 0.8f;
public float maxSpeedMultiplier = 1.8f;

private float currentSpeedMultiplier = 1.8f;
private float speedTimer = 0f;
private float currentDuration = 1f;


    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip lightOnSound;
    public AudioClip machineSound;

    [Header("Timing")]
    public float startDelay = 3f;
    public float moveDuration = 27f;
    public float fadeOutTime = 2f;

    private bool activated = false;
    private bool canMove = false;

    private Vector3 startPos;
    private float moveTimer = 0f;
    private bool reachedTarget = false;

    void Start()
    {
        if (lightObject != null)
            lightObject.SetActive(false);
    }

    void Update()
    {
        if (!activated || !canMove || reachedTarget) return;

        MoveLight();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (activated) return;

        activated = true;

        if (lightObject != null)
        {
            lightObject.SetActive(true);
            startPos = lightObject.transform.position;
        }

        // 🔊 Turn ON sound (start)
        if (audioSource && lightOnSound)
            audioSource.PlayOneShot(lightOnSound);

        StartCoroutine(StartAfterDelay());
    }

    IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);

        canMove = true;

        // 🔊 Start machine loop
        if (audioSource && machineSound)
        {
            audioSource.clip = machineSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

void MoveLight()
{
    if (lightObject == null || lightTarget == null) return;

    speedTimer += Time.deltaTime;

    if (speedTimer >= currentDuration)
    {
        speedTimer = 0f;

        // 25% chance to go slow briefly
        if (Random.value < 0.25f)
        {
            currentSpeedMultiplier = minSpeedMultiplier;
            currentDuration = Random.Range(0.1f, 0.3f); // VERY short slow
        }
        else
        {
            currentSpeedMultiplier = Random.Range(1.2f, maxSpeedMultiplier);
            currentDuration = Random.Range(0.6f, 1.5f); // longer fast
        }
    }

    moveTimer += Time.deltaTime * currentSpeedMultiplier;
    float t = moveTimer / moveDuration;

    lightObject.transform.position = Vector3.Lerp(
        startPos,
        lightTarget.position,
        t
    );

    if (t >= 1f)
    {
        reachedTarget = true;
        StartCoroutine(EndSequence());
    }
}



    IEnumerator EndSequence()
    {
        // 🔉 Fade out machine sound
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutTime);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;

        if (audioSource && lightOnSound)
            audioSource.PlayOneShot(lightOnSound);

        yield return new WaitForSeconds(0.2f);

        if (lightObject != null)
            lightObject.SetActive(false);
    }
}
