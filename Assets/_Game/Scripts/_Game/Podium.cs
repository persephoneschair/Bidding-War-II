using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Podium : MonoBehaviour
{
    public PlayerObject containedPlayer;
    public TextMeshPro playerNameMesh;
    public TextMeshPro responseMesh;
    public TextMeshPro scoreMesh;

    public GameObject avatar;
    public Renderer avatarRend;

    public Animator anim;

    public Renderer[] podiumLockInLights;
    public Renderer[] podiumAlertLights;

    public GameObject zoomedAnswerObj;
    public TextMeshPro zoomedAnswerMesh;

    public enum LightOption { Default = 0, LockedIn, Correct, IncorrectAndWarning, AccessGranted };
    public Material[] podiumColors;

    public void InitialisePodium()
    {
        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.PodiumAnim);
        anim.SetTrigger("toggle");
        playerNameMesh.text = containedPlayer.playerName;
        responseMesh.text = "";
        scoreMesh.text = containedPlayer.points.ToString();
    }

    public void OnMouseOver()
    {
        if (containedPlayer == null || ClockManager.inFinal)
            return;

        if (!string.IsNullOrEmpty(containedPlayer.twitchName) && !containedPlayer.eliminated)
            avatar.SetActive(true);

        if ((containedPlayer.flagForCondone && !containedPlayer.wasCorrect) || (!containedPlayer.flagForCondone && containedPlayer.wasCorrect))
        {
            if (GameControl.GetGameControl.currentRound == GameControl.Round.SurvivalOfTheFittest && GameControl.GetGameControl.currentStage == GameControl.GameplayStage.ResetPostQuestion)
                return;

            zoomedAnswerObj.SetActive(true);
            zoomedAnswerMesh.text = containedPlayer.submission;
        }
    }

    public void OnMouseExit()
    {
        avatar.SetActive(false);
        zoomedAnswerMesh.text = "";
        zoomedAnswerObj.SetActive(false);
    }

    public void OnMouseDown()
    {
        if (containedPlayer == null)
            return;

        if (GameControl.GetGameControl.currentRound == GameControl.Round.SurvivalOfTheFittest && GameControl.GetGameControl.currentStage == GameControl.GameplayStage.ResetPostQuestion)
            return;

        if (containedPlayer.flagForCondone && !containedPlayer.wasCorrect)
        {
            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.PlayerAnswer);
            containedPlayer.flagForCondone = false;
            containedPlayer.wasCorrect = true;
            containedPlayer.totalCorrect++;
            UpdateLockInLights(LightOption.Correct);
            IteratePoints(true, containedPlayer.points + containedPlayer.currentBid, false);
            //HostManager.GetHost.UpdateLeaderboards();
            //HostManager.GetHost.SendPayloadToClient(containedPlayer, "DISPLAYANSWER", GameControl.GetGameControl.rounds[(int)GameControl.GetGameControl.currentRound].currentQuestion.validAnswers[0] + "|" + containedPlayer.wasCorrect.ToString().ToUpperInvariant());
            HostManager.GetHost.SendPayloadToClient(containedPlayer, EventLibrary.HostEventType.SingleAndMultiResult, GameControl.GetGameControl.rounds[(int)GameControl.GetGameControl.currentRound].currentQuestion.validAnswers[0] + "|" + (containedPlayer.wasCorrect ? "Correct" : "Incorrect"));
        }
        else if(!containedPlayer.flagForCondone && containedPlayer.wasCorrect)
        {
            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.WrongInFinal);
            containedPlayer.flagForCondone = true;
            containedPlayer.wasCorrect = false;
            containedPlayer.totalCorrect--;
            UpdateLockInLights(LightOption.IncorrectAndWarning);
            IteratePoints(false, containedPlayer.points - containedPlayer.currentBid, true);
            //HostManager.GetHost.UpdateLeaderboards();
            //HostManager.GetHost.SendPayloadToClient(containedPlayer, "DISPLAYANSWER", GameControl.GetGameControl.rounds[(int)GameControl.GetGameControl.currentRound].currentQuestion.validAnswers[0] + "|" + containedPlayer.wasCorrect.ToString().ToUpperInvariant());
            HostManager.GetHost.SendPayloadToClient(containedPlayer, EventLibrary.HostEventType.SingleAndMultiResult, GameControl.GetGameControl.rounds[(int)GameControl.GetGameControl.currentRound].currentQuestion.validAnswers[0] + "|" + (containedPlayer.wasCorrect ? "Correct" : "Incorrect"));
        }
    }

    public void UpdateLockInLights(LightOption targetCol)
    {
        foreach (Renderer r in podiumLockInLights)
            r.material = podiumColors[(int)targetCol];
    }
    public void UpdateAlertLights(LightOption targetCol)
    {
        foreach (Renderer r in podiumAlertLights)
            r.material = podiumColors[(int)targetCol];
    }

    public void IteratePoints(bool up, int target, bool doTTS)
    {
        //Set new points first prior to animation
        int starterPoints = containedPlayer.points;
        containedPlayer.points = target;

        if (target > containedPlayer.maxPoints)
            containedPlayer.maxPoints = target;

        if(containedPlayer.points <= 0)
        {
            containedPlayer.eliminated = true;
            if(doTTS)
                TTSManager.GetTTS.Speak(containedPlayer.playerName + " has been eliminated");
        }
        if (containedPlayer.eliminated)
            HostManager.GetHost.SendPayloadToClient(containedPlayer, EventLibrary.HostEventType.UpdateScore, $"Total: {containedPlayer.totalCorrect}");
        else
            HostManager.GetHost.SendPayloadToClient(containedPlayer, EventLibrary.HostEventType.UpdateScore, $"Points: {containedPlayer.points}");

        StartCoroutine(Iterator(up, target, starterPoints));
    }

    public void R3Iterate(bool up, int target)
    {
        int starterPoints = containedPlayer.points;
        containedPlayer.points = target;

        if (target > containedPlayer.maxPoints)
            containedPlayer.maxPoints = target;

        if (containedPlayer.eliminated)
            HostManager.GetHost.SendPayloadToClient(containedPlayer, EventLibrary.HostEventType.UpdateScore, $"Total: {containedPlayer.totalCorrect}");
        else
            HostManager.GetHost.SendPayloadToClient(containedPlayer, EventLibrary.HostEventType.UpdateScore, $"Points: {containedPlayer.points}");

        StartCoroutine(Iterator(up, target, starterPoints));
    }

    IEnumerator Iterator(bool up, int target, int starterPoints)
    {
        if(CameraLerpController.GetCam.performingAZoom)
        {
            yield return new WaitUntil(() => !CameraLerpController.GetCam.performingAZoom);
            yield return new WaitForSeconds(1f);
        }
        
        if (up)
            while (starterPoints < containedPlayer.points)
            {
                starterPoints++;
                scoreMesh.text = starterPoints.ToString();
                AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.PointTick);
                yield return new WaitForSeconds(0.03f);
            }
        else
            while (starterPoints > containedPlayer.points)
            {
                starterPoints--;
                scoreMesh.text = starterPoints.ToString();
                AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.PointTick);
                yield return new WaitForSeconds(0.03f);
            }        

        if (containedPlayer.eliminated)
            HardEliminate();
    }

    public void HardEliminate()
    {
        containedPlayer.eliminated = true;
        containedPlayer.points = 0;
        UpdateAlertLights(LightOption.IncorrectAndWarning);
        UpdateLockInLights(LightOption.IncorrectAndWarning);
        DebugLog.Print(containedPlayer.playerName + " WAS ELIMINATED", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
        HostManager.GetHost.SendPayloadToClient(containedPlayer, EventLibrary.HostEventType.UpdateScore, $"Total: {containedPlayer.totalCorrect}");
        anim.SetTrigger("toggle");
        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.PodiumAnim);
        AudioManager.GetAudioManager.PlayUnique(AudioManager.OneShotClip.WrongInFinal);
        playerNameMesh.text = "";
        responseMesh.text = "";
    }

    public void ZoomOnPodium()
    {
        CameraLerpController.GetCam.ZoomOnPodium(this);
    }

    public void RestorePlayer()
    {
        anim.SetTrigger("toggle");
        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.PodiumAnim);
        UpdateAlertLights(LightOption.Default);
        UpdateLockInLights(LightOption.Default);
        responseMesh.text = $"TOTAL";
        scoreMesh.text = containedPlayer.totalCorrect.ToString();
        HostManager.GetHost.SendPayloadToClient(containedPlayer, EventLibrary.HostEventType.UpdateScore, $"Points: {containedPlayer.points}");
        playerNameMesh.text = containedPlayer.playerName;
        //HostManager.GetHost.UpdateLeaderboards();
    }
}