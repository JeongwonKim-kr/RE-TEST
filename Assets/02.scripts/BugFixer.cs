using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BugFixer : MonoBehaviour
{
    [Header("UI Images (RawImage, Image, etc.)")]
    public Graphic[] uiGraphics;

    [Header("TMP Texts")]
    public TMP_Text[] tmpTexts;

    void Awake()
    {
        // 🔹 Handle UI Graphics
        foreach (Graphic g in uiGraphics)
        {
            if (g != null)
            {
                SetAlpha(g, 0f);
            }
        }

        // 🔹 Handle TMP Text separately
        foreach (TMP_Text t in tmpTexts)
        {
            if (t != null)
            {
                SetAlphaTMP(t, 0f);
            }
        }
    }

    void SetAlpha(Graphic g, float a)
    {
        Color c = g.color;
        c.a = a;
        g.color = c;
    }

    void SetAlphaTMP(TMP_Text t, float a)
    {
        Color c = t.color;
        c.a = a;
        t.color = c;
    }
}
