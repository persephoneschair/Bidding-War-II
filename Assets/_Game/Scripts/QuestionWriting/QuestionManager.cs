using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using UnityEngine.PlayerLoop;

public static class QuestionManager
{
    public static Pack currentPack = null;

    public static void DecompilePack(TextAsset tx)
    {
        currentPack = JsonConvert.DeserializeObject<Pack>(tx.text);
    }

    public static int GetRoundQCount()
    {
        switch (GameControl.GetGameControl.currentRound)
        {
            case GameControl.Round.OpeningGambits:
                return currentPack.r1Questions.Count;

            case GameControl.Round.TopGun:
                return currentPack.r2Questions.Count;

            case GameControl.Round.SurvivalOfTheFittest:
                return currentPack.r3Questions.Count;

            case GameControl.Round.Domination:
                return currentPack.r4Questions.Count;

            default:
                return 0;
        }
    }

    public static Question GetQuestion(int qNum)
    {
        switch (GameControl.GetGameControl.currentRound)
        {
            case GameControl.Round.OpeningGambits:
                return currentPack.r1Questions[qNum];

            case GameControl.Round.TopGun:
                return currentPack.r2Questions[qNum];

            case GameControl.Round.SurvivalOfTheFittest:
                return currentPack.r3Questions[qNum];

            case GameControl.Round.Domination:
                return currentPack.r4Questions[qNum];

            default:
                return null;
        }
    }
}
