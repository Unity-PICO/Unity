using TMPro;
using UnityEngine;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;

    public void ShowSubtitle(string content, float duration = 3f)
    {
        StopAllCoroutines();
        StartCoroutine(ShowSubtitleCoroutine(content, duration));
    }

    private IEnumerator ShowSubtitleCoroutine(string content, float duration)
    {
        subtitleText.text = content;
        subtitleText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(false);
    }
}
