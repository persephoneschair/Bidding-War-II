using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TopGun : RoundType
{
    public override void BiddingRunning()
    {
        base.BiddingRunning();
        int maxVal = GameControl.nextQuestionIndex + 4;
        foreach (PlayerObject p in HostManager.GetHost.players.Where(x => !x.eliminated))
        {
            int playerMax = Mathf.Min(p.points, maxVal);
            string range = "";
            for (int i = 0; i < playerMax; i++)
            {
                range += (i + 1).ToString();
                if (i + 1 != playerMax)
                    range += "|";
            }
            HostManager.GetHost.SendPayloadToClient(p, EventLibrary.HostEventType.MultipleChoiceQuestion,
                $"<size=50%>{GameControl.nextQuestionIndex}/{QuestionManager.GetRoundQCount()}</size>\n{currentQuestion.category}|7|{range}");
        }

        foreach (PlayerObject p in HostManager.GetHost.players.Where(x => x.eliminated))
            HostManager.GetHost.SendPayloadToClient(p, EventLibrary.HostEventType.Information, "You are eliminated and cannot bid on this question...");
    }

    public override void OnBiddingEnded()
    {
        foreach (PlayerObject po in HostManager.GetHost.players)
            HostManager.GetHost.SendPayloadToClient(po, EventLibrary.HostEventType.Information, "Bidding ended");

        foreach (PlayerObject po in HostManager.GetHost.players.Where(x => x.currentBid == 0 && !x.eliminated))
        {
            po.currentBid = 1;
            //HostManager.GetHost.SendPayloadToClient(po, "CURRENTBID", "1");
        }
        base.OnBiddingEnded();
    }

    public override void DisplayAnswer()
    {
        base.DisplayAnswer();
        GameControl.GetGameControl.currentStage = GameControl.GameplayStage.R2JustEliminations;
    }

    public void DoEliminations()
    {
        questionLozengeAnim.SetTrigger("toggle");

        string concat = "";
        foreach (PlayerObject po in HostManager.GetHost.players.Where(x => !x.eliminated && x.flagForCondone))
        {
            po.podium.IteratePoints(false, po.points - po.currentBid, false);
            if (po.points <= 0)
                concat += po.playerName + ", ";
        }
        if(concat.Length > 0)
            TTSManager.GetTTS.Speak(concat + " you have been eliminated");

        GameControl.GetGameControl.currentStage = GameControl.GameplayStage.R2DisplayPassageGranted;
    }

    public void DisplayPassageGranted()
    {
        //Filter players according to whether they were right and not already through
        //Then order by high bid, then by fastest time
        List<PlayerObject> orderedList = HostManager.GetHost.players
            .Where(x => x.wasCorrect && !x.passageGranted)
            .OrderByDescending(x => x.currentBid)
            .ThenBy(x => x.submissionTime).ToList();

        PlayerObject po = orderedList.FirstOrDefault();
        if (po == null)
            TTSManager.GetTTS.Speak("Nobody obtained passage");
        else
        {
            LobbyManager.GetLobby.permaCodeAnim.SetTrigger("toggle");
            po.passageGranted = true;
            po.passageJustGranted = true;
            po.podium.UpdateAlertLights(Podium.LightOption.AccessGranted);
            po.podium.ZoomOnPodium();
            po.podium.responseMesh.text = $"Bid: {po.currentBid}\nTime: {po.submissionTime.ToString("##.00")}s";
            StartCoroutine(ClearPodiumResponseMesh(po));
            TTSManager.GetTTS.Speak($"Passage has been granted to {po.playerName}, with a bid of {po.currentBid} and a response time of {po.submissionTime.ToString("##.00")} seconds");
        }
        //HostManager.GetHost.UpdateLeaderboards();
        GameControl.GetGameControl.currentStage = GameControl.GameplayStage.ResetPostQuestion;
    }

    public override void ResetForNewQuestion()
    {        
        base.ResetForNewQuestion();
    }

    public override void BespokeEndOfRoundLogic()
    {
        string concat = "";
        foreach (PlayerObject p in HostManager.GetHost.players.Where(x => !x.passageGranted && !x.eliminated))
        {
            p.eliminated = true;
            p.podium.HardEliminate();
            concat += p.playerName + ", ";
        }
        if (HostManager.GetHost.players.Count(x => !x.passageGranted && !x.eliminated) > 0)
            TTSManager.GetTTS.Speak(concat + " you have been eliminated");
    }

    IEnumerator ClearPodiumResponseMesh(PlayerObject po)
    {
        yield return new WaitForSeconds((CameraLerpController.GetCam.defaultZoomOut * 2) + CameraLerpController.GetCam.defaultCrashZoom);
        po.podium.responseMesh.text = "";
        po.passageJustGranted = false;
        LobbyManager.GetLobby.permaCodeAnim.SetTrigger("toggle");
    }
}
