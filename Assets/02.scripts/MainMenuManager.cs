using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene")]
    public string gameSceneName = "GameScene";

    [Header("UI")]
    public RawImage blackScreen;
    public TextMeshProUGUI messageText;

    [Header("Buttons")]
    public RawImage playButton;
    public RawImage exitButton;

    [Header("Fade & Timing")]
    public float fadeSpeed = 1.5f;
    public float waitTime = 5f;

    [Header("Text Messages")]
    public string message1 = "Sending Clean Up Crew...";
    public string message2 = "Getting Confirmation from Manager...";
    public float typingSpeed = 0.05f;
    public float delayBetweenMessages = 1.5f;

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip playSound;

    private bool isBusy = false;

    void Awake()
    {
        // Ensure blackScreen starts fully transparent and inactive
        if (blackScreen != null)
        {
            blackScreen.color = new Color(0, 0, 0, 0f);
            blackScreen.gameObject.SetActive(false);
        }

        if (messageText != null)
            messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 0f);
    }

    void Start()
    {
        AddButtonListener(playButton, OnPlayPressed);
        AddButtonListener(exitButton, OnExitPressed);

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    void AddButtonListener(RawImage img, UnityEngine.Events.UnityAction action)
    {
        if (img == null) return;

        Button btn = img.GetComponent<Button>();
        if (btn == null) btn = img.gameObject.AddComponent<Button>();

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    public void OnPlayPressed()
    {
        if (isBusy) return;
        StartCoroutine(StartGameFlow());
    }

    public void OnExitPressed()
    {
        if (isBusy) return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator StartGameFlow()
    {
        isBusy = true;

        // Hide buttons
        if (playButton != null) playButton.gameObject.SetActive(false);
        if (exitButton != null) exitButton.gameObject.SetActive(false);

        // Play click sound
        if (sfxSource != null && playSound != null)
            sfxSource.PlayOneShot(playSound);

        // Activate blackScreen early but keep alpha 0
        if (blackScreen != null)
            blackScreen.gameObject.SetActive(true);

        // Fade in black screen smoothly while typing messages
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            SetImageAlpha(t);
            SetTextAlpha(t);
            if (musicSource != null)
                musicSource.volume = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        // First message
        yield return StartCoroutine(TypeText(message1));

        yield return new WaitForSeconds(delayBetweenMessages);

        messageText.text = ""; // clear text

        // Second message
        yield return StartCoroutine(TypeText(message2));

        yield return new WaitForSeconds(waitTime);

        // Async load scene properly
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);
        asyncLoad.allowSceneActivation = true;

        yield return asyncLoad;
    }

    IEnumerator TypeText(string text)
    {
        if (messageText == null) yield break;

        messageText.text = "";
        foreach (char c in text)
        {
            messageText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void SetImageAlpha(float a)
    {
        if (blackScreen == null) return;
        Color c = blackScreen.color;
        c.a = Mathf.Clamp01(a);
        blackScreen.color = c;
    }

    void SetTextAlpha(float a)
    {
        if (messageText == null) return;
        Color c = messageText.color;
        c.a = Mathf.Clamp01(a);
        messageText.color = c;
    }
}
