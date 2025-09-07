using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlairText : MonoBehaviour
{
    public static FlairText flair;
    public Text flairText;
    public string currentText = "";
    float typingSpeed = 0.05f; // Delay between each character


    private void Start()
    {
        if(flair == null)
        {
            flair = this;
        }
        else { Destroy(gameObject); }
        Enable();
    }

    public void Enable()
    {
        flairText = GameObject.FindGameObjectWithTag("FlairText").GetComponent<Text>();
    }

    public void TypeText(string text)
    {
        StopAllCoroutines();
        flairText.text = "";
        StartCoroutine(ShowText(text));
    }

    IEnumerator ShowText(string textToType)
    {
        currentText = "";
        for (int i = 0; i <= textToType.Length; i++)
        {
            currentText = textToType.Substring(0, i);
            flairText.text = currentText + "▪"; ;
            yield return new WaitForSeconds(typingSpeed);
            if (currentText.Length == textToType.Length)
            {
                flairText.text = currentText;
            }
        }
    }
}
