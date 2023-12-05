using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;
using System.Linq;
using Control;

public class GameControl : MonoBehaviour
{
    [Header("Managers")]
    public CentralPodiumManager hostManager;

    [Header("Rounds")]
    public LobbyManager lobbyManager;
    public InstructionsManager instructionsManager;
    public RoundType[] rounds;

    [Header("Question Data")]
    public static int nextQuestionIndex = 0;

    [Header("Misc")]
    public Animator secondWebcam;

    public enum GameplayStage
    {
        RunTitles,
        OpenLobby,
        LockLobby,
        RevealInstructions,
        HideInstructions,
        RunQuestion,

        R2JustEliminations,
        R2DisplayPassageGranted,
        
        R3UpdatePodiaPostQ,
        R3RestorePlayers,

        SetUpR4,
        R4CalculateLightArray,

        ResetPostQuestion,
        DisplayFinalLeaderboard,
        HideFinalLeaderboard,
        RollCredits,
        DoNothing
    };

    [Header("Gameplay Data")]
    public GameplayStage currentStage = GameplayStage.DoNothing;

    public enum Round { OpeningGambits, TopGun, SurvivalOfTheFittest, Domination, None };
    public Round currentRound = Round.None;
    public int roundsPlayed = 0;
    private readonly string[] ttsRoundNames = new string[4] { "Opening Gambits", "Top Gun", "Survival of the Fittest", "Domination" };

    public Pack decompiledPack;

    #region Init

    public static GameControl GetGameControl { get; private set; }
    private void Awake()
    {
        if (GetGameControl != null && GetGameControl != this)
            Destroy(this);
        else
            GetGameControl = this;
    }

    #endregion

    public void LaunchGame()
    {

    }

    [Button]
    public void ProgressGameplay()
    {
        switch (currentStage)
        {
            case GameplayStage.RunTitles:
                TitlesManager.GetTitlesManager.RunTitleSequence();
                if (Operator.GetOperator.recoveryMode)
                    SaveManager.UpdateAllPlayerPodia();
                break;

            case GameplayStage.OpenLobby:
                lobbyManager.OnOpenLobby();
                currentStage = GameplayStage.LockLobby;
                break;

            case GameplayStage.LockLobby:
                lobbyManager.OnLockLobby();
                AudioManager.GetAudioManager.StopLoop();
                AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.Impact);
                currentStage = GameplayStage.RevealInstructions;
                if (Operator.GetOperator.recoveryMode)
                    SaveManager.RestoreGameplayState();
                else
                    SaveManager.BackUpData();
                break;

            case GameplayStage.RevealInstructions:
                AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.Impact);
                AudioManager.GetAudioManager.Play(AudioManager.LoopClip.GameplayLoop);
                int roundNum = roundsPlayed + 1;
                TTSManager.GetTTS.Speak($"Round {roundNum}. {ttsRoundNames[roundsPlayed]}");
                currentRound = (Round)roundsPlayed;

                if (currentRound == Round.SurvivalOfTheFittest)
                    (rounds[(int)currentRound] as SurvivalOfTheFittest).ClearPerilLights();

                instructionsManager.OnShowInstructions();
                currentStage = GameplayStage.HideInstructions;
                break;

            case GameplayStage.HideInstructions:
                instructionsManager.OnHideInstructions();

                AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.Impact);

                if (currentRound != Round.Domination)
                {
                    AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.Rotation);
                    hostManager.TogglePodium();
                    if (HostManager.GetHost.players.Count < 39)
                        secondWebcam.SetTrigger("toggle");

                }

                if (currentRound == Round.SurvivalOfTheFittest)
                    (rounds[(int)currentRound] as SurvivalOfTheFittest).SetPlayerInPeril();

                else if (currentRound == Round.Domination)
                    ClockManager.GetClockManager.RetractArmsForFinal();

                currentStage = GameplayStage.RunQuestion;
                break;

            case GameplayStage.RunQuestion:
                SaveManager.BackUpData();
                rounds[(int)currentRound].LoadQuestion(nextQuestionIndex);
                nextQuestionIndex++;
                currentStage = GameplayStage.DoNothing;
                break;

            case GameplayStage.R2JustEliminations:
                if (currentRound == Round.TopGun)
                    (rounds[(int)currentRound] as TopGun).DoEliminations();
                break;

            case GameplayStage.R2DisplayPassageGranted:
                if (currentRound == Round.TopGun)
                    (rounds[(int)currentRound] as TopGun).DisplayPassageGranted();
                break;

            case GameplayStage.R3UpdatePodiaPostQ:
                if (currentRound == Round.SurvivalOfTheFittest)
                    (rounds[(int)currentRound] as SurvivalOfTheFittest).UpdatePodiumScoresPostQuestion();
                break;

            case GameplayStage.R3RestorePlayers:
                if (currentRound == Round.SurvivalOfTheFittest)
                    (rounds[(int)currentRound] as SurvivalOfTheFittest).RestorePlayersToLife();
                break;

            case GameplayStage.SetUpR4:
                currentRound = (Round)roundsPlayed;

                if (currentRound == Round.Domination)
                    (rounds[(int)currentRound] as Domination).SetUpR4();
                break;

            case GameplayStage.R4CalculateLightArray:

                if (currentRound == Round.Domination)
                    (rounds[(int)currentRound] as Domination).CalculateNewLightArray();
                break;

            case GameplayStage.ResetPostQuestion:
                rounds[(int)currentRound].ResetForNewQuestion();
                break;

            case GameplayStage.DisplayFinalLeaderboard:
                GlobalLeaderboardManager.GetLeaderboard.PopulateLeaderboard();
                currentStage = GameplayStage.HideFinalLeaderboard;
                break;

            case GameplayStage.HideFinalLeaderboard:
                GlobalLeaderboardManager.GetLeaderboard.ToggleLeaderboard();
                currentStage = GameplayStage.RollCredits;
                break;

            case GameplayStage.RollCredits:
                CreditsManager.GetCreditsManager.RollCredits();
                currentStage = GameplayStage.DoNothing;
                GameplayPennys.GetPennys.UpdatePennysAndMedals();
                HostManager.GetHost.SendEndAlertToAllPlayers();
                break;

            case GameplayStage.DoNothing:
                break;
        }
    }
}
