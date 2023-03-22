using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SubtitleManager : MonoBehaviour
{
    //public TextMeshProUGUI subtitleText; // Reference to the TextMeshPro component
    public Text subtitleText;

    private IEnumerator currentSubtitleCoroutine; // Used to stop the current subtitle coroutine

    public void DisplaySubtitle(string text, float duration)
    {
        // Stop the current subtitle coroutine if it's already running
        if (currentSubtitleCoroutine != null)
        {
            StopCoroutine(currentSubtitleCoroutine);
        }

        // Set the text of the TextMeshPro component to the subtitle text
        subtitleText.text = text;

        // Start a new coroutine to hide the subtitle after a specified duration
        currentSubtitleCoroutine = HideSubtitle(duration);
        StartCoroutine(currentSubtitleCoroutine);
    }

    private IEnumerator HideSubtitle(float duration)
    {
        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        // Clear the text of the TextMeshPro component
        subtitleText.text = "";
    }
}