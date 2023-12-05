using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitlesManager : MonoBehaviour
{

    #region Init

    public static TitlesManager GetTitlesManager { get; private set; }
    private void Awake()
    {
        if (GetTitlesManager != null && GetTitlesManager != this)
            Destroy(this);
        else
            GetTitlesManager = this;
    }

    #endregion

    public CentralPodiumManager hostArray;
    public TextMeshProUGUI titleMesh;
    public GameObject dividingLine;
    public Animator anim;
    public GameObject sceneBlocker;
    [TextArea(2,2)] public string[] titleOptions;

    [Button]
    public void RunTitleSequence()
    {
        if (Operator.GetOperator.skipOpeningTitles)
            EndOfTitleSequence();
        else
        {
            GameControl.GetGameControl.currentStage = GameControl.GameplayStage.DoNothing;
            StartCoroutine(TitleSequence());
        }           
    }

    IEnumerator TitleSequence()
    {
        AudioManager.GetAudioManager.Play(AudioManager.LoopClip.Titles, false);
        for (int i = 0; i < titleOptions.Length; i++)
        {
            yield return new WaitForSeconds(2f);
            titleMesh.text = titleOptions[i];
            anim.SetTrigger("toggle");
            if(i + 1 == titleOptions.Length)
            {
                yield return new WaitForSeconds(2f);
                dividingLine.SetActive(true);
                yield return new WaitForSeconds(1f);
                anim.enabled = false;
                break;
            }
            yield return new WaitForSeconds(6f);
        }
        yield return new WaitForSeconds(3f);
        EndOfTitleSequence();
    }

    void EndOfTitleSequence()
    {
        AudioManager.GetAudioManager.Play(AudioManager.LoopClip.GameplayLoop);
        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.Impact);
        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.Rotation);
        hostArray.OnEndOfTitles();
        sceneBlocker.SetActive(false);
        this.gameObject.SetActive(false);
        GameControl.GetGameControl.currentStage = GameControl.GameplayStage.OpenLobby;
        GameControl.GetGameControl.ProgressGameplay();
    }
}
