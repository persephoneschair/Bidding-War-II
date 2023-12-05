using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OpeningGambits : RoundType
{
    public override void BiddingRunning()
    {
        base.BiddingRunning();
        foreach (PlayerObject p in HostManager.GetHost.players.Where(x => !x.eliminated))
        {
            string range = "1|2|3";
            switch(p.points)
            {
                case 1:
                    range = "1";
                    break;

                case 2:
                    range = "1|2";
                    break;
            }
            HostManager.GetHost.SendPayloadToClient(p, EventLibrary.HostEventType.MultipleChoiceQuestion,
                $"<size=50%>{GameControl.nextQuestionIndex}/{QuestionManager.GetRoundQCount()}</size>\n{currentQuestion.category}|7|{range}");
        }
            
        foreach (PlayerObject p in HostManager.GetHost.players.Where(x => x.eliminated))
            HostManager.GetHost.SendPayloadToClient(p, EventLibrary.HostEventType.Information, "You are eliminated and cannot bid on this question...");
        //HostManager.GetHost.SendPayloadToClient(p, "R1BIDELIM", currentQuestion.category + "|" + $"({GameControl.nextQuestionIndex}/{QuestionManager.GetRoundQCount()})");
    }

    public override void OnBiddingEnded()
    {
        foreach (PlayerObject po in HostManager.GetHost.players)
            HostManager.GetHost.SendPayloadToClient(po, EventLibrary.HostEventType.Information, "Bidding ended");
            //HostManager.GetHost.SendPayloadToClient(po, "R1KILLBIDBOX", "1");

        foreach (PlayerObject po in HostManager.GetHost.players.Where(x => x.currentBid == 0 && !x.eliminated))
            po.currentBid = 1;
            //HostManager.GetHost.SendPayloadToClient(po, "CURRENTBID", "1");

        base.OnBiddingEnded();
    }

    public override void DisplayAnswer()
    {
        base.DisplayAnswer();
        GameControl.GetGameControl.currentStage = GameControl.GameplayStage.ResetPostQuestion;
    }

    public override void ResetForNewQuestion()
    {
        questionLozengeAnim.SetTrigger("toggle");

        foreach (PlayerObject po in HostManager.GetHost.players.Where(x => !x.eliminated && x.flagForCondone))
            po.podium.IteratePoints(false, po.points - po.currentBid, true);

        base.ResetForNewQuestion();
    }
}
