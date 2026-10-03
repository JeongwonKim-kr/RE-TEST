using UnityEngine;
using System.Collections;

public class BGMResetTrigger : MonoBehaviour
{
    public AudioSource bgmSource;
    public AudioClip normalBGMClip;
    public float fadeSpeed = 1.5f;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(FadeToNormalBGM());
    }

    IEnumerator FadeToNormalBGM()
    {
        if (bgmSource == null || normalBGMClip == null) yield break;

        while (bgmSource.volume > 0f)
        {
            bgmSource.volume -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        bgmSource.clip = normalBGMClip;
        bgmSource.Play();

        while (bgmSource.volume < 1f)
        {
            bgmSource.volume += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        bgmSource.volume = 1f;
    }
}
