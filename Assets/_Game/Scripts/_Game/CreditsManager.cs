using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreditsManager : MonoBehaviour
{
    #region Init

    public static CreditsManager GetCreditsManager { get; private set; }
    private void Awake()
    {
        if (GetCreditsManager != null && GetCreditsManager != this)
            Destroy(this);
        else
            GetCreditsManager = this;
    }

    #endregion

    public GameObject divider;
    public GameObject sceneBlocker;
    public GameObject endCard;
    public Animator[] titlesAnim;

    public TextMeshProUGUI[] meshes;

    [TextArea(2, 5)] public string[] topTitleOptions;
    [TextArea(2, 5)] public string[] bottomTitleOptions;
    public int[] lineCount;

    private void Start()
    {
        this.gameObject.SetActive(false);
    }

    [Button]
    public void RollCredits()
    {
        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.Impact);
        AudioManager.GetAudioManager.Play(AudioManager.LoopClip.Credits, false);
        this.gameObject.SetActive(true);
        sceneBlocker.SetActive(true);
        StartCoroutine(Credits());
    }

    IEnumerator Credits()
    {
        for (int i = 0; i < topTitleOptions.Length; i++)
        {
            yield return new WaitForSeconds(3.6f);
            if (i == 0)
                divider.SetActive(false);
            ToggleAnimator();
            
            yield return new WaitForSeconds(0.75f);
            
            if (i == 0)
                meshes[0].fontSizeMax = 150f;
            else if(i == topTitleOptions.Length - 1)
                meshes[0].fontSizeMax = 250f;

            foreach (TextMeshProUGUI tx in meshes)
                tx.text = "";

            meshes[lineCount[i] * 2].text = topTitleOptions[i];
            meshes[(lineCount[i] * 2) + 1].text = bottomTitleOptions[i];

            ToggleAnimator();

            if (i == topTitleOptions.Length - 1)
            {
                yield return new WaitForSeconds(0.25f);
                divider.SetActive(true);
            }
        }
        yield return new WaitForSeconds(4.2f);
        endCard.SetActive(true);
    }

    void ToggleAnimator()
    {
        foreach (Animator a in titlesAnim)
            a.SetTrigger("toggle");
    }
}
