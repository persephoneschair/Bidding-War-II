using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class SurvivalOfTheFittest : RoundType
{
    public void SetPlayerInPeril()
    {
        string tts = "";
        List<PlayerObject> lowScoringPlayers = GetLowScoringPlayers();
        foreach(PlayerObject p in lowScoringPlayers)
        {
            tts += p.playerName + ", ";
            p.podium.UpdateAlertLights(Podium.LightOption.IncorrectAndWarning);
        }
        WindowsVoice.speak(tts + " you are in peril", 0f);

        //HostManager.GetHost.UpdateLeaderboards();
    }

    public void ClearPerilLights()
    {
        foreach (PlayerObject po in HostManager.GetHost.players.Where(x => !x.eliminated))
        {
            po.podium.UpdateLockInLights(Podium.LightOption.Default);
            po.podium.UpdateAlertLights(Podium.LightOption.Default);
        }            
    }

    public List<PlayerObject> GetLowScoringPlayers()
    {
        return HostManager.GetHost.players.Where(x => !x.eliminated).Where(x => x.points == HostManager.GetHost.players.Where(x => !x.eliminated).Select(x => x.points).Min()).ToList();
    }

    public override void BiddingRunning()
    {
        base.BiddingRunning();
        foreach (PlayerObject p in HostManager.GetHost.players.Where(x => !x.eliminated))
            if(p.points <= 5)
                HostManager.GetHost.SendPayloadToClient(p, EventLibrary.HostEventType.Information, "Your bid will default to your current score...");
        else
            HostManager.GetHost.SendPayloadToClient(p, EventLibrary.HostEventType.NumericalQuestion,
            $"<size=50%>{GameControl.nextQuestionIndex}/{QuestionManager.GetRoundQCount()}\n(Bid from 5-{p.points})</size>\n{currentQuestion.category}|7");
        //HostManager.GetHost.SendPayloadToClient(p, "R3BID", p.points.ToString() + "|" + currentQuestion.category + "|" + $"({GameControl.nextQuestionIndex}/{QuestionManager.GetRoundQCount()})");


        foreach (PlayerObject p in HostManager.GetHost.players.Where(x => x.eliminated))
            HostManager.GetHost.SendPayloadToClient(p, EventLibrary.HostEventType.Information, "You are eliminated and cannot bid on this question...");
    }

    public override void OnBiddingEnded()
    {
        foreach (PlayerObject po in HostManager.GetHost.players)
            HostManager.GetHost.SendPayloadToClient(po, EventLibrary.HostEventType.Information, "Bidding ended");

        foreach (PlayerObject po in HostManager.GetHost.players.Where(x => x.currentBid == 0 && !x.eliminated))
        {
            po.currentBid = po.points < 5 ? po.points : 5;
            //HostManager.GetHost.SendPayloadToClient(po, "CURRENTBID", $"{po.currentBid.ToString()}");
        }
        base.OnBiddingEnded();
    }

    public override void DisplayAnswer()
    {
        base.DisplayAnswer();
        GameControl.GetGameControl.currentStage = GameControl.GameplayStage.R3UpdatePodiaPostQ;
    }

    public void UpdatePodiumScoresPostQuestion()
    {
        questionLozengeAnim.SetTrigger("toggle");

        foreach (PlayerObject po in HostManager.GetHost.players.Where(x => !x.eliminated && x.flagForCondone))
            po.podium.R3Iterate(false, po.points - po.currentBid);

        GameControl.GetGameControl.currentStage = GameControl.GameplayStage.ResetPostQuestion;

        //HostManager.GetHost.UpdateLeaderboards();
    }

    public override void ResetForNewQuestion()
    {
        List<PlayerObject> playersToEliminate = GetLowScoringPlayers();
        int lowScore = playersToEliminate.FirstOrDefault().points;
        string tts = "";

        foreach(PlayerObject po in playersToEliminate)
        {
            tts += po.playerName + ", ";
            po.podium.HardEliminate();
        }
        WindowsVoice.speak($"With a score of {lowScore}, {tts}, you have been eliminated", 0f);

        //Out of questions or two players remain

        if (HostManager.GetHost.players.Count(x => !x.eliminated) < 2)
            GameControl.GetGameControl.currentStage = GameControl.GameplayStage.R3RestorePlayers;
        else if (GameControl.nextQuestionIndex == QuestionManager.GetRoundQCount() || HostManager.GetHost.players.Count(x => !x.eliminated) == 2)
            EndRound();
        else
            GameControl.GetGameControl.currentStage = GameControl.GameplayStage.RunQuestion;

        base.ResetPlayerVariables();
        //HostManager.GetHost.UpdateLeaderboards();

        ClearPerilLights();
        
        if(GameControl.GetGameControl.currentStage == GameControl.GameplayStage.RunQuestion)
            Invoke("SetPlayerInPeril", 7f);
    }

    public void RestorePlayersToLife()
    {
        //We have fewer than two players - after eliminations, bring some player(s) back
        List<PlayerObject> orderedPlayers = HostManager.GetHost.players.Where(x => x.eliminated).OrderByDescending(x => x.totalCorrect).ThenBy(x => x.playerName).ToList();
        while(HostManager.GetHost.players.Count(x => !x.eliminated) < 2)
        {
            if (orderedPlayers.Count == 0)
                break;

            PlayerObject playerToRestore = orderedPlayers.FirstOrDefault();
            playerToRestore.eliminated = false;
            playerToRestore.podium.RestorePlayer();
            DebugLog.Print(playerToRestore.playerName + " WAS RESTORED", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
            WindowsVoice.speak(playerToRestore.playerName + " HAS BEEN RESTORED");
            orderedPlayers.Remove(playerToRestore);
        }
        EndRound();
    }

    public void EndRound()
    {
        //HostManager.GetHost.UpdateLeaderboards();
        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.Impact);
        AudioManager.GetAudioManager.StopLoop();
        GameControl.GetGameControl.currentStage = GameControl.GameplayStage.SetUpR4;
        GameControl.GetGameControl.roundsPlayed++;
        GameControl.nextQuestionIndex = 0;
        TTSManager.GetTTS.Speak($"End of round {GameControl.GetGameControl.roundsPlayed}");
        GameControl.GetGameControl.hostManager.TogglePodium();
        if (HostManager.GetHost.players.Count < 39)
            GameControl.GetGameControl.secondWebcam.SetTrigger("toggle");

        foreach (PlayerObject p in HostManager.GetHost.players)
            HostManager.GetHost.SendPayloadToClient(p, EventLibrary.HostEventType.Information, "End of round");
    }
}
