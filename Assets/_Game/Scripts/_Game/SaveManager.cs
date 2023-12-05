using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SaveManager
{
    public static List<PlayerObjectSerializable> backupDataList = new List<PlayerObjectSerializable>();
    public static GameplayDataSerializable gameplayData = new GameplayDataSerializable();

    public static void BackUpData()
    {
        backupDataList.Clear();
        foreach(PlayerObject plO in HostManager.GetHost.players)
            backupDataList.Add(NewPlayer(plO));

        var playerData = JsonConvert.SerializeObject(backupDataList);

        File.WriteAllText(Application.persistentDataPath + "\\BackUpData.txt", playerData);

        GameplayDataSerializable gpd = new GameplayDataSerializable()
        {
            nextQuestionNumber = GameControl.nextQuestionIndex,
            currentRound = GameControl.GetGameControl.currentRound,
            roundsPlayed = GameControl.GetGameControl.roundsPlayed
        };

        var gameStateData = JsonConvert.SerializeObject(gpd);

        File.WriteAllText(Application.persistentDataPath + "\\GameplayData.txt", gameStateData);
    }
    public static PlayerObjectSerializable NewPlayer(PlayerObject playerObject)
    {
        PlayerObjectSerializable pl = new PlayerObjectSerializable();
        pl.playerName = playerObject.playerName;
        pl.playerClientID = playerObject.playerClientID;
        pl.twitchName = playerObject.twitchName;
        pl.eliminated = playerObject.eliminated;
        pl.passageGranted = playerObject.passageGranted;
        pl.points = playerObject.points;
        pl.maxPoints = playerObject.maxPoints;
        pl.totalCorrect = playerObject.totalCorrect;
        return pl;
    }

    public static void RestoreData()
    {
        backupDataList.Clear();

        if(File.Exists(Application.persistentDataPath + "\\BackUpData.txt"))
            backupDataList = JsonConvert.DeserializeObject<List<PlayerObjectSerializable>>(File.ReadAllText(Application.persistentDataPath + "\\BackUpData.txt"));
        if (File.Exists(Application.persistentDataPath + "\\GameplayData.txt"))
            gameplayData = JsonConvert.DeserializeObject<GameplayDataSerializable>(File.ReadAllText(Application.persistentDataPath + "\\GameplayData.txt"));
    }

    public static void RestorePlayer(PlayerObject po)
    {
        PlayerObjectSerializable rc = backupDataList.FirstOrDefault(x => x.playerClientID.ToLowerInvariant() == po.playerClientID.ToLowerInvariant());

        if(rc != null)
        {
            po.playerName = rc.playerName;
            po.twitchName = rc.twitchName;
            po.eliminated = rc.eliminated;
            po.passageGranted = rc.passageGranted;

            po.points = rc.points;
            po.maxPoints = rc.maxPoints;
            po.totalCorrect = rc.totalCorrect;
        }
    }

    public static void UpdateAllPlayerPodia()
    {
        TTSManager.GetTTS.Speak("PLAYER RECOVERY COMPLETE");
        foreach(PlayerObject obj in HostManager.GetHost.players)
        {
            if(!string.IsNullOrEmpty(obj.twitchName))
            {
                obj.podium.playerNameMesh.text = obj.playerName;
                if (obj.eliminated)
                    obj.podium.HardEliminate();
                if (obj.passageGranted)
                    obj.podium.UpdateAlertLights(Podium.LightOption.AccessGranted);
            }
        }
        //HostManager.GetHost.UpdateLeaderboards();
        BackUpData();
    }

    public static void RestoreGameplayState()
    {
        if(gameplayData != null)
        {
            GameControl.nextQuestionIndex = gameplayData.nextQuestionNumber;
            GameControl.GetGameControl.currentRound = gameplayData.currentRound;
            GameControl.GetGameControl.roundsPlayed = gameplayData.roundsPlayed;
        }
        Operator.GetOperator.recoveryMode = false;
        HostManager.GetHost.host.ReloadHost = false;
    }
}
