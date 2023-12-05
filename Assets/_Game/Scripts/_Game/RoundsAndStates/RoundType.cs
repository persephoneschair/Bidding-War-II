using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RoundType : MonoBehaviour
{
    public Question currentQuestion = null;
    public Animator questionLozengeAnim;
    public TextMeshProUGUI questionMesh;

    public virtual void LoadQuestion(int qNum)
    {
        currentQuestion = QuestionManager.GetQuestion(qNum);
        foreach (PlayerObject po in HostManager.GetHost.players)
            po.currentBid = 0;
        
        OpenBidding();
    }

    public virtual void OpenBidding()
    {
        foreach (PlayerObject po in HostManager.GetHost.players)
            HostManager.GetHost.SendPayloadToClient(po, EventLibrary.HostEventType.Information, "Get ready to bid...");
            //HostManager.GetHost.SendPayloadToClient(po, "COUNTDOWNBID", currentQuestion.question);

        ClockManager.GetClockManager.WarningFlash(ClockArm.NodeColor.Blue);
        TTSManager.GetTTS.Speak("Get ready for the category");
        Invoke("BiddingRunning", 3.25f);
    }

    public virtual void BiddingRunning()
    {
        ClockManager.GetClockManager.RunFastClock();
        TTSManager.GetTTS.Speak($"The category is {currentQuestion.category}");
        questionLozengeAnim.SetTrigger("toggle");
        questionMesh.text = $"<size=50%><u>CATEGORY ({GameControl.nextQuestionIndex}/{QuestionManager.GetRoundQCount()})</u></size>\n{currentQuestion.category}";
    }

    public virtual void OnBiddingEnded()
    {
        questionLozengeAnim.SetTrigger("toggle");

        TTSManager.GetTTS.Speak("Time");
        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.QStartAndEnd);

        //Some sort of stagger for the podiums?
        List<PlayerObject> orderedBids = HostManager.GetHost.players.Where(x => !x.eliminated).OrderByDescending(x => x.currentBid).ToList();
        foreach (PlayerObject po in orderedBids)
        {
            if(GameControl.GetGameControl.currentRound != GameControl.Round.Domination)
            {
                po.podium.responseMesh.text = "Bid: " + po.currentBid.ToString();
                po.podium.UpdateLockInLights(Podium.LightOption.Default);
            }
            else
            {
                var pod = Domination.GetDomination.GetFinalistPodium(po);
                int index = Domination.GetDomination.finalistsPodia[0] == pod ? 1 : 2;
                pod.SetBidLights(po.currentBid, index);
                pod.SetRingColor(CentralPodiumManager.RingColor.Default);
            }
            DebugLog.Print($"{po.playerName}: {po.currentBid}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Yellow);
        }
        Invoke("RunQuestion", 5f);
    }

    public virtual void RunQuestion()
    {
        foreach (PlayerObject po in HostManager.GetHost.players)
            HostManager.GetHost.SendPayloadToClient(po, EventLibrary.HostEventType.Information, $"Get ready for the question on {currentQuestion.category}...");
            //HostManager.GetHost.SendPayloadToClient(po, "COUNTDOWNQ", currentQuestion.question);

        TTSManager.GetTTS.Speak("Get ready for the question");
        ClockManager.GetClockManager.WarningFlash(ClockArm.NodeColor.Red);
        Invoke("QuestionRunning", 3.25f);
    }

    public virtual void QuestionRunning()
    {
        foreach (PlayerObject p in HostManager.GetHost.players)
            HostManager.GetHost.SendPayloadToClient(p, EventLibrary.HostEventType.SimpleQuestion, $"<size=50%><u>{currentQuestion.category})</u></size>\n{currentQuestion.question}|14");
            //HostManager.GetHost.SendPayloadToClient(p, "QUES", currentQuestion.question);

        GlobalTimer.GetTimer.questionClockRunning = true;
        ClockManager.GetClockManager.RunRegularClock();
        questionLozengeAnim.SetTrigger("toggle");
        questionMesh.text = $"<size=50%><u>{currentQuestion.category} ({GameControl.nextQuestionIndex}/{QuestionManager.GetRoundQCount()})</u></size>\n{currentQuestion.question}";
    }

    public virtual void OnQuestionEnded()
    {
        TTSManager.GetTTS.Speak("Time");
        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.QStartAndEnd);
        questionLozengeAnim.SetTrigger("toggle");

        foreach (PlayerObject po in HostManager.GetHost.players)
            HostManager.GetHost.SendPayloadToClient(po, EventLibrary.HostEventType.Information, "Question ended");
        //HostManager.GetHost.SendPayloadToClient(po, "KILLANSWERARRAY", "1");

        foreach (PlayerObject po in HostManager.GetHost.players.Where(x => string.IsNullOrEmpty(x.submission)))
        {
            po.submission = "NO ANSWER";
            //HostManager.GetHost.SendPayloadToClient(po, "DISPLAYRESPONSE", "NO ANSWER|NA");
            if(!po.eliminated)
            {
                if(GameControl.GetGameControl.currentRound != GameControl.Round.Domination)
                    po.podium.UpdateLockInLights(Podium.LightOption.LockedIn);
                else
                    Domination.GetDomination.GetFinalistPodium(po).SetRingColor(CentralPodiumManager.RingColor.LockedIn);
            }
        }
        Invoke("DisplayAnswer", 2f);
    }

    public virtual void DisplayAnswer()
    {
        GlobalTimer.GetTimer.questionClockRunning = false;
        questionLozengeAnim.SetTrigger("toggle");
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

        //Some sort of stagger for the podiums?
        foreach (PlayerObject po in HostManager.GetHost.players.Where(x => !x.eliminated))
        {
            po.podium.responseMesh.text = po.submission.ToString();
            if (Extensions.Spellchecker(po.submission, currentQuestion.validAnswers))
            {
                po.wasCorrect = true;
                po.podium.IteratePoints(true, po.points + po.currentBid, false);
                po.podium.UpdateLockInLights(Podium.LightOption.Correct);
            }
            else
            {
                po.flagForCondone = true;
                po.podium.UpdateLockInLights(Podium.LightOption.IncorrectAndWarning);
            }                
        }

        foreach(PlayerObject po in HostManager.GetHost.players)
        {
            HostManager.GetHost.SendPayloadToClient(po, EventLibrary.HostEventType.SingleAndMultiResult, currentQuestion.validAnswers[0] + "|" + (po.wasCorrect ? "Correct" : "Incorrect"));
            //HostManager.GetHost.SendPayloadToClient(po, "DISPLAYANSWER", currentQuestion.validAnswers[0] + "|" + po.wasCorrect.ToString().ToUpperInvariant());

            //Display eliminated players' correct count rather than their score
            if (po.eliminated)
                HostManager.GetHost.SendPayloadToClient(po, EventLibrary.HostEventType.UpdateScore, $"Total: {po.totalCorrect}");
            else
                HostManager.GetHost.SendPayloadToClient(po, EventLibrary.HostEventType.UpdateScore, $"Points: {po.points}");
            //HostManager.GetHost.SendPayloadToClient(po, "CURRENTCORRECT", po.totalCorrect.ToString());
        }

        //HostManager.GetHost.UpdateLeaderboards();
    }

    public virtual void ResetForNewQuestion()
    {
        ResetPlayerVariables();


        //Out of questions
        if (GameControl.nextQuestionIndex == QuestionManager.GetRoundQCount())
        {
            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.Impact);
            AudioManager.GetAudioManager.StopLoop();
            GameControl.GetGameControl.currentStage = GameControl.GameplayStage.RevealInstructions;
            GameControl.GetGameControl.roundsPlayed++;
            GameControl.nextQuestionIndex = 0;
            TTSManager.GetTTS.Speak($"End of round {GameControl.GetGameControl.roundsPlayed}");
            GameControl.GetGameControl.hostManager.TogglePodium();
            if (HostManager.GetHost.players.Count < 39)
                GameControl.GetGameControl.secondWebcam.SetTrigger("toggle");

            foreach(PlayerObject p in  HostManager.GetHost.players)
                HostManager.GetHost.SendPayloadToClient(p, EventLibrary.HostEventType.Information, "End of round");

            BespokeEndOfRoundLogic();
        }
        else
            GameControl.GetGameControl.currentStage = GameControl.GameplayStage.RunQuestion;

        //HostManager.GetHost.UpdateLeaderboards();
    }

    public virtual void BespokeEndOfRoundLogic()
    {

    }

    public virtual void ResetPlayerVariables()
    {
        foreach (PlayerObject po in HostManager.GetHost.players)
        {
            po.currentBid = 0;
            po.submission = "";
            po.submissionTime = 0;
            po.flagForCondone = false;
            po.wasCorrect = false;

            if (!po.eliminated)
            {
                if (!po.passageJustGranted)
                    po.podium.responseMesh.text = "";

                po.podium.UpdateLockInLights(Podium.LightOption.Default);
            }
            //HostManager.GetHost.SendPayloadToClient(po, "RESETFORNEWQ", "");
        }
    }
}
