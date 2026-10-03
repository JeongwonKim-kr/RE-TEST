using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeadBodyInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera playerCam;

    [Header("Interaction")]
    public float interactDistance = 1f;
    public KeyCode interactKey = KeyCode.E;

    public RawImage eUI;
    public Vector3 uiOffset = new Vector3(0, 0f, 0); // 🔥 offset above body

    [Header("Monster")]
    public GameObject monsterPrefab;
    public Transform spawnPoint;

    [Header("Monster Root")]
    public GameObject monsterRoot;

    private bool alreadyUsed = false;

    void Start()
    {
        if (eUI != null)
        {
            SetAlpha(eUI, 0f);
        }

        if (monsterRoot != null)
        {
            monsterRoot.SetActive(false);
        }
    }

    void Update()
    {
        if (alreadyUsed || player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        bool canInteract = dist < interactDistance;

        // 🔥 ALWAYS update UI position (makes it follow smoothly)
        if (eUI != null)
        {
            Vector3 worldPos = transform.position + uiOffset;
            Vector3 screenPos = playerCam.WorldToScreenPoint(worldPos);

            if (screenPos.z > 0)
            {
                eUI.transform.position = screenPos;
                FadeUI(eUI, canInteract);
            }
            else
            {
                FadeUI(eUI, false);
            }
        }

        // Press E
        if (canInteract && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    void Interact()
    {
        alreadyUsed = true;

        if (eUI != null)
            eUI.gameObject.SetActive(false);

        // Spawn monster
        if (monsterPrefab != null && spawnPoint != null)
        {
            Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        if (monsterRoot != null)
        {
            monsterRoot.SetActive(true);
        }
    }

    void FadeUI(RawImage img, bool show)
    {
        Color c = img.color;
        float target = show ? 1f : 0f;
        c.a = Mathf.MoveTowards(c.a, target, 6f * Time.deltaTime);
        img.color = c;
    }

    void SetAlpha(RawImage img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
