using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using NaughtyAttributes;
using System.Linq;

public class Operator : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("Supresses Twitch chat messages and will store Pennys and medals in a separate test file")]
    public bool testMode;
    [Tooltip("Skips opening titles")]
    public bool skipOpeningTitles;
    [Tooltip("Players must join the room with valid Twitch username as their name; this will skip the process of validation")]
    public bool fastValidation;
    [Tooltip("Start the game in recovery mode to restore any saved data from a previous game crash")]
    public bool recoveryMode;

    [Header("Quesion Data")]
    public TextAsset questionPack;

    #region Init
    public static Operator GetOperator { get; private set; }
    private void Awake()
    {
        if (GetOperator != null && GetOperator != this)
            Destroy(this);
        else
            GetOperator = this;

        if (recoveryMode)
            skipOpeningTitles = true;
    }

    #endregion

    private void Start()
    {
        if (questionPack != null)
            QuestionManager.DecompilePack(questionPack);
        else
        {
            DebugLog.Print("NO QUESTION PACK LOADED; PLEASE ASSIGN ONE AND RESTART THE BUILD", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
            return;
        }            
        HostManager.GetHost.host.ReloadHost = recoveryMode;
        if (recoveryMode)
            SaveManager.RestoreData();

        DataStorage.CreateDataPath();
        GameControl.GetGameControl.LaunchGame();
    }

    [Button]
    public void ProgressGameplay()
    {
        GameControl.GetGameControl.ProgressGameplay();
    }
}
