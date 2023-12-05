using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockManager : MonoBehaviour
{
    public ClockArm[] arms;
    private bool arrayRunning;
    private bool timeExpired;
    public static bool fastTimer;
    public static bool inFinal;

    #region Init

    public static ClockManager GetClockManager { get; private set; }
    private void Awake()
    {
        if (GetClockManager != null && GetClockManager != this)
            Destroy(this);
        else
            GetClockManager = this;
    }

    #endregion

    [Button]
    public void RunFastClock()
    {
        if (arrayRunning || timeExpired)
            return;
        StartCoroutine(Countdown(0.5f));
    }

    [Button]
    public void RunRegularClock()
    {
        if (arrayRunning || timeExpired)
            return;
        StartCoroutine(Countdown(1f));
    }

    [Button]
    public void ResetClock()
    {
        if (arrayRunning || !timeExpired)
            return;
        StartCoroutine(Reset());
    }

    private IEnumerator Countdown(float delay)
    {
        fastTimer = delay == 1f ? true : false;
        arrayRunning = true;
        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.QStartAndEnd);
        for (int i = 14; i >= 0; i--)
        {
            if(!inFinal)
                arms[i].animatingLineRend.material = arms[i].lineMats[delay == 0.5f ? 0 : 1];

            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.ClockArm);
            arms[i].armAnim.speed = delay == 0.5f ? 2 : 1;
            arms[i].armAnim.SetTrigger("toggle");
            yield return new WaitForSeconds(delay);
            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.ClockTick);
        }
        if (delay == 0.5f)
            Invoke("LockBidding", 1f);
        else
            Invoke("LockQuestion", 1f);
        timeExpired = true;
        arrayRunning = false;
    }

    private IEnumerator Reset()
    {
        arrayRunning = true;
        for (int i = 0; i < 15; i++)
        {
            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.ClockArm);
            arms[i].armAnim.speed = 1;
            arms[i].armAnim.SetTrigger("toggle");
            yield return new WaitForSeconds(0.1f);
        }
        timeExpired = false;
        arrayRunning = false;
    }

    public void LockBidding()
    {
        ResetClock();
        GameControl.GetGameControl.rounds[(int)GameControl.GetGameControl.currentRound].OnBiddingEnded();
    }

    public void LockQuestion()
    {
        ResetClock();
        GameControl.GetGameControl.rounds[(int)GameControl.GetGameControl.currentRound].OnQuestionEnded();
    }

    public void WarningFlash(ClockArm.NodeColor col)
    {
        if (!inFinal)
            StartCoroutine(WarningFlashRoutine(col));
        else
            StartCoroutine(FinalWarningFlashRoutine());
    }

    IEnumerator WarningFlashRoutine(ClockArm.NodeColor col)
    {
        for(int i = 0; i < 3; i++)
        {
            foreach (var ar in arms)
                ar.node.Color = ar.colors[(int)ClockArm.NodeColor.Off];
            yield return new WaitForSeconds(0.2f);

            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.ClockTick);
            foreach (var ar in arms)
                ar.node.Color = ar.colors[(int)col];
            yield return new WaitForSeconds(0.8f);
        }
        foreach (var ar in arms)
            ar.node.Color = ar.colors[(int)ClockArm.NodeColor.Off];
    }

    IEnumerator FinalWarningFlashRoutine()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.2f);
            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.ClockTick);
            yield return new WaitForSeconds(0.8f);
        }
    }

    public void SetUpClockForFinal()
    {
        foreach (ClockArm arm in arms)
            arm.SetNodeColor(ClockArm.NodeColor.Off);

        StartCoroutine(PopulateFinalArray());
    }

    IEnumerator PopulateFinalArray()
    {
        inFinal = true;
        yield return new WaitForSeconds(1f);
        for(int i = 0; i < 8; i++)
        {
            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.ClockArm);
            arms[i].SetLineColor(ClockArm.LineColor.Blue);
            arms[i].armAnim.SetTrigger("toggle");
            arms[i].armAnim.speed = 2f;
            yield return new WaitForSeconds(0.5f);
            arms[i].SetNodeColor(ClockArm.NodeColor.Blue);
            arms[i].currentFinalOccupancy = ClockArm.LineColor.Blue;
        }
        for (int i = 8; i < 15; i++)
        {
            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.ClockArm);
            arms[i].SetLineColor(ClockArm.LineColor.Red);
            arms[i].armAnim.SetTrigger("toggle");
            arms[i].armAnim.speed = 2f;
            yield return new WaitForSeconds(0.5f);
            arms[i].SetNodeColor(ClockArm.NodeColor.Red);
            arms[i].currentFinalOccupancy = ClockArm.LineColor.Red;
        }
    }

    public void RetractArmsForFinal()
    {
        StartCoroutine(RetractArms());
    }

    IEnumerator RetractArms()
    {
        for (int i = 0; i < 15; i++)
        {
            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.ClockArm);
            arms[i].armAnim.SetTrigger("toggle");
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ChangeArms(List<int> newScores)
    {
        for(int i = 0; i < newScores[0]; i++)
            if (arms[i].currentFinalOccupancy != ClockArm.LineColor.Blue)
                StartCoroutine(arms[i].FinalArmChange(ClockArm.NodeColor.Blue, ClockArm.LineColor.Blue));

        for (int i = newScores[0]; i < 15; i++)
            if (arms[i].currentFinalOccupancy != ClockArm.LineColor.Red)
                StartCoroutine(arms[i].FinalArmChange(ClockArm.NodeColor.Red, ClockArm.LineColor.Red));
    }
}
