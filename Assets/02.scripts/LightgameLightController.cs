using UnityEngine;

public class LightGameLightController : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Monster")]
    public GameObject monster;
    public float maxOutTime = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip flickerSound;

    private bool playerInLight = false;
    private float outTimer = 0f;
    private bool monsterActive = false;

    void Start()
    {
        if (monster != null)
            monster.SetActive(false);
    }

    void Update()
    {
        if (!playerInLight)
        {
            outTimer += Time.deltaTime;



            if (outTimer >= maxOutTime && !monsterActive)
            {
                ActivateMonster();
            }
        }
        else
        {
            outTimer = 0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInLight = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInLight = false;
        }
    }

    void ActivateMonster()
    {
        monsterActive = true;

        if (monster != null)
            monster.SetActive(true);


}
}
