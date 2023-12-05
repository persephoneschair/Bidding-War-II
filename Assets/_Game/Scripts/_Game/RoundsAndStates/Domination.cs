using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Domination : RoundType
{

    #region Init

    public static Domination GetDomination { get; private set; }
    private void Awake()
    {
        if (GetDomination != null && GetDomination != this)
            Destroy(this);
        else
            GetDomination = this;
    }

    #endregion

    public CentralPodiumManager[] finalistsPodia;
    public Animator finalistAnswersAnim;

    public Animator statueLidAnim;
    public Animator statueAnim;
    public TextMeshPro winnerNameMesh;
    public Renderer winnerAvatarRend;

    public CentralPodiumManager GetFinalistPodium(PlayerObject po)
    {
        return finalistsPodia.FirstOrDefault(x => x.containedPlayer == po);
    }
    public CentralPodiumManager GetFinalistOpponentPodium(PlayerObject po)
    {
        return finalistsPodia.FirstOrDefault(x => x.containedPlayer != po);
    }

    public void SetUpR4()
    {
        AudioManager.GetAudioManager.Play(AudioManager.LoopClip.WinTheme, false);
        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.Impact);
        GameControl.GetGameControl.hostManager.hostCamAnim.SetTrigger("toggle");
        CameraLerpController.GetCam.ZoomToFinal();
        List<PlayerObject> po = HostManager.GetHost.players.Where(x => !x.eliminated).OrderByDescending(x => x.points).ThenByDescending(x => x.totalCorrect).ToList();
        if(po.Count() >= finalistsPodia.Length)
        {
            for (int i = 0; i < finalistsPodia.Length; i++)
            {
                po[i].podium.anim.SetTrigger("toggle");
                finalistsPodia[i].SetUpPlayerPodium(po[i]);
                po[i].points = i == 0 ? 8 : 7;
            }
        }
        ClockManager.GetClockManager.SetUpClockForFinal();
        //HostManager.GetHost.UpdateLeaderboards();
        GameControl.GetGameControl.currentStage = GameControl.GameplayStage.RevealInstructions;
    }

    public override void BiddingRunning()
    {
        base.BiddingRunning();
        foreach (PlayerObject p in HostManager.GetHost.players.Where(x => !x.eliminated))
            HostManager.GetHost.SendPayloadToClient(p, EventLibrary.HostEventType.MultipleChoiceQuestion,
                $"<size=50%>{GameControl.nextQuestionIndex}/{QuestionManager.GetRoundQCount()}</size>\n{currentQuestion.category}|7|1|2|3");


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
        int[] bids = new int[2] { finalistsPodia[0].containedPlayer.currentBid, finalistsPodia[1].containedPlayer.currentBid };
        FinalBoxGraph.GetGraph.ShowGraph(finalistsPodia[0].containedPlayer.points, bids);
        base.OnBiddingEnded();
    }

    public override void DisplayAnswer()
    {
        GlobalTimer.GetTimer.questionClockRunning = false;
        questionLozengeAnim.SetTrigger("toggle");
        finalistAnswersAnim.SetTrigger("toggle");
        questionMesh.text = $"<size=50%><u>ANSWER</u></size>\n{currentQuestion.validAnswers[0]}";

        //Update ALL players with their total correct count
        foreach (PlayerObject po in HostManager.GetHost.players)
        {
            if (Extensions.Spellchecker(po.submission, currentQuestion.validAnswers))
            {
                po.totalCorrect++;
                if (po.eliminated)
                    po.wasCorrect = true;
            }
        }

        foreach (PlayerObject po in HostManager.GetHost.players.Where(x => !x.eliminated))
        {
            var pod = GetFinalistPodium(po);
            pod.answerMesh.text = po.submission.ToString();
            if (Extensions.Spellchecker(po.submission, currentQuestion.validAnswers))
            {
                po.wasCorrect = true;
                pod.SetRingColor(CentralPodiumManager.RingColor.Correct);
            }
            else
            {
                po.flagForCondone = true;
                pod.SetRingColor(CentralPodiumManager.RingColor.Incorrect);
            }
        }

        foreach (PlayerObject po in HostManager.GetHost.players)
        {
            HostManager.GetHost.SendPayloadToClient(po, EventLibrary.HostEventType.SingleAndMultiResult, currentQuestion.validAnswers[0] + "|" + (po.wasCorrect ? "Correct" : "Incorrect"));

            if (po.eliminated)
                HostManager.GetHost.SendPayloadToClient(po, EventLibrary.HostEventType.UpdateScore, $"Total: {po.totalCorrect}");
            else
                HostManager.GetHost.SendPayloadToClient(po, EventLibrary.HostEventType.UpdateScore, $"Points: {po.points}");

            /*HostManager.GetHost.SendPayloadToClient(po, "DISPLAYANSWER", currentQuestion.validAnswers[0] + "|" + po.wasCorrect.ToString().ToUpperInvariant());
            //Display eliminated players' correct count rather than their score
            if (po.eliminated)
                HostManager.GetHost.SendPayloadToClient(po, "CURRENTCORRECT", po.totalCorrect.ToString());
            else
                HostManager.GetHost.SendPayloadToClient(po, "CURRENTSCORE", po.points.ToString());*/
        }
        GameControl.GetGameControl.currentStage = GameControl.GameplayStage.R4CalculateLightArray;
    }

    public void CalculateNewLightArray()
    {
        FinalBoxGraph.GetGraph.HideGraph();
        questionLozengeAnim.SetTrigger("toggle");
        finalistAnswersAnim.SetTrigger("toggle");
        foreach (PlayerObject po in HostManager.GetHost.players.Where(x => !x.eliminated))
        {
            var pod = GetFinalistPodium(po);
            pod.SetRingColor(CentralPodiumManager.RingColor.Default);
            pod.SetBidLights(0);
        }

        List<int> newScores = new List<int>();
        foreach(PlayerObject po in finalistsPodia.Select(x => x.containedPlayer).ToList())
        {
            PlayerObject opp = GetFinalistOpponentPodium(po).containedPlayer;
            if(po.wasCorrect)
                po.points += po.currentBid;
            else
                po.points -= po.currentBid;

            if (opp.wasCorrect)
                po.points -= opp.currentBid;
            else
                po.points += opp.currentBid;

            po.points = po.points > 15 ? 15 : po.points;
            po.points = po.points < 0 ? 0 : po.points;

            newScores.Add(po.points);
            HostManager.GetHost.SendPayloadToClient(po, EventLibrary.HostEventType.UpdateScore, $"Points: {po.points}");
        }

        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.WrongInFinal, 0.5f);
        ClockManager.GetClockManager.ChangeArms(newScores);
        //HostManager.GetHost.UpdateLeaderboards();
        GameControl.GetGameControl.currentStage = GameControl.GameplayStage.ResetPostQuestion;
    }

    public override void ResetForNewQuestion()
    {
        ResetPlayerVariables();

        //HostManager.GetHost.UpdateLeaderboards();

        //Out of questions
        if (GameControl.nextQuestionIndex == QuestionManager.GetRoundQCount() || HostManager.GetHost.players.Any(x => x.points == 15))
        {
            AudioManager.GetAudioManager.StopLoop();
            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.Impact);
            PlayerObject winner = HostManager.GetHost.players.Where(x => !x.eliminated).OrderByDescending(x => x.points).FirstOrDefault();            
            GameControl.GetGameControl.roundsPlayed++;
            TTSManager.GetTTS.Speak($"Congratulations to {winner.playerName}. You have won the war.");
            GameControl.GetGameControl.currentStage = GameControl.GameplayStage.DoNothing;
            StartCoroutine(Celebration(winner));
        }
        else
            GameControl.GetGameControl.currentStage = GameControl.GameplayStage.RunQuestion;
    }

    public IEnumerator Celebration(PlayerObject winner)
    {
        foreach (PlayerObject p in HostManager.GetHost.players)
            HostManager.GetHost.SendPayloadToClient(p, EventLibrary.HostEventType.Information, $"The war is over\nYou won {p.totalCorrect * GameplayPennys.GetPennys.multiplyFactor} Pennys");
        yield return new WaitForSeconds(5f);
        LobbyManager.GetLobby.TogglePermaCode();
        AudioManager.GetAudioManager.Play(AudioManager.LoopClip.WinTheme, false);
        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.Rotation, 3f);
        CameraLerpController.GetCam.ZoomToChampion();
        yield return new WaitForSeconds(3f);
        statueLidAnim.SetTrigger("toggle");
        yield return new WaitForSeconds(1f);
        winnerNameMesh.text = winnerNameMesh.text.Replace("[NAME]", winner.playerName);
        winnerAvatarRend.material.mainTexture = winner.profileImage;
        statueAnim.SetTrigger("toggle");
        yield return new WaitForSeconds(7f);
        GameControl.GetGameControl.currentStage = GameControl.GameplayStage.DisplayFinalLeaderboard;
    }
}
