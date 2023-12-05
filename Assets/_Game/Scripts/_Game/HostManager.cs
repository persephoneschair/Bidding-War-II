using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Control;
using Newtonsoft.Json;
using System.Linq;
using TwitchLib.Api.Core.Models.Undocumented.ChannelPanels;

public class HostManager : MonoBehaviour
{
    public Host host;
    public List<PlayerObject> players = new List<PlayerObject>();
    public List<PlayerObject> waitingRoom = new List<PlayerObject>();

    #region Init

    public static HostManager GetHost { get; private set; }
    private void Awake()
    {
        if (GetHost != null && GetHost != this)
            Destroy(this);
        else
            GetHost = this;
    }

    #endregion

    public void OnRoomConnected()
    {
        DebugLog.Print($"PLAYERS MAY NOW JOIN THE ROOM WITH THE CODE {host.RoomCode}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Blue);
    }

    public void OnPlayerJoins(Player joinedPlayer)
    {
        /*string[] name = joinedPlayer.Name.Split('¬');
        if (name.Length != 2 || (name.Length == 2 && name[1] != "BIDDINGWAR"))
        {
            SendPayloadToClient(joinedPlayer, "WrongApp", "");
            return;
        }*/

        if (players.Count >= 40)
        {
            //Do something slightly better than this
            return;
        }


        PlayerObject pl = new PlayerObject(joinedPlayer);
        pl.playerClientID = joinedPlayer.UserID;
        waitingRoom.Add(pl);

        if (waitingRoom.Count == 31)
            CameraLerpController.GetCam.TopRowOccupied();

        if (Operator.GetOperator.recoveryMode)
        {
            DebugLog.Print($"{joinedPlayer.Name} HAS BEEN RECOVERED", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Orange);
            SaveManager.RestorePlayer(pl);
            if (pl.twitchName != null)
                StartCoroutine(RecoveryValidation(pl));
            else
            {
                pl.otp = "";
                pl.podium.containedPlayer = null;
                pl.podium = null;
                pl.playerClientRef = null;
                pl.playerName = "";
                players.Remove(pl);
                DebugLog.Print($"{joinedPlayer.Name} HAS BEEN CLEARED", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
                return;
            }
        }
        else if (Operator.GetOperator.fastValidation)
            StartCoroutine(FastValidation(pl));

        DebugLog.Print($"{joinedPlayer.Name} HAS JOINED THE LOBBY", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
        SendPayloadToClient(pl, EventLibrary.HostEventType.Validate, $"{pl.otp}");
    }

    private IEnumerator FastValidation(PlayerObject pl)
    {
        yield return new WaitForSeconds(1f);
        TwitchControl.GetTwitchControl.testUsername = pl.playerName;
        TwitchControl.GetTwitchControl.testMessage = pl.otp;
        TwitchControl.GetTwitchControl.SendTwitchWhisper();
        TwitchControl.GetTwitchControl.testUsername = "";
        TwitchControl.GetTwitchControl.testMessage = "";
    }

    private IEnumerator RecoveryValidation(PlayerObject pl)
    {
        yield return new WaitForSeconds(1f);
        TwitchControl.GetTwitchControl.RecoveryValidation(pl.twitchName, pl.otp);
    }

    /*public void UpdateLeaderboards()
    {
        List<PlayerObject> lb = players.OrderBy(x => x.eliminated).ThenByDescending(x => x.points).ThenByDescending(x => x.totalCorrect).ThenByDescending(x => x.maxPoints).ThenBy(x => x.playerName).ToList();

        string payload = "";
        for (int i = 0; i < lb.Count; i++)
        {
            string eliminated = lb[i].eliminated ? "TRUE" : "FALSE";
            string score = lb[i].eliminated ? lb[i].totalCorrect.ToString() : lb[i].points.ToString();
            string passageGranted = lb[i].passageGranted ? "TRUE" : "FALSE";
            string danger = "FALSE";
            if(GameControl.GetGameControl.currentRound == GameControl.Round.SurvivalOfTheFittest)
            {
                var x = (GameControl.GetGameControl.rounds[(int)GameControl.GetGameControl.currentRound] as SurvivalOfTheFittest).GetLowScoringPlayers();
                if (x.Contains(lb[i]))
                    danger = "TRUE";
            }

            payload += $"{lb[i].playerName.ToUpperInvariant()}|{score}|{eliminated}|{passageGranted}|{danger}";
            if (i + 1 != lb.Count)
                payload += "¬";
        }

        foreach (PlayerObject pl in lb)
            SendPayloadToClient(pl.playerClientRef, "LEADERBOARD", payload);
    }*/


    public void SendPayloadToClient(PlayerObject pl, EventLibrary.HostEventType e, string data)
    {
        host.UpdatePlayerData(pl.playerClientRef, EventLibrary.GetHostEventTypeString(e), data);
    }


    #region REMOVE THESE

    public void SendEndAlertToAllPlayers()
    {
        /*foreach (PlayerObject p in players)
            SendPayloadToClient(p, "ENDOFGAME", (p.totalCorrect * 10).ToString());*/
    }

    #endregion

    public void OnReceivePayloadFromClient(EventMessage e)
    {
        PlayerObject p = GetPlayerFromEvent(e);
        EventLibrary.ClientEventType eventType = EventLibrary.GetClientEventType(e.EventName);

        string st = (string)e.Data[e.EventName];
        var data = JsonConvert.DeserializeObject<string>(st);

        /*switch(e.EventName)
        {
            case "BID":
                string s = (string)e.Data["BID"];
                var ds = JsonConvert.DeserializeObject<string>(s);

                if (int.TryParse(ds, out int value))
                {
                    if (!p.eliminated)
                    {
                        if (GameControl.GetGameControl.currentRound != GameControl.Round.Domination)
                            p.podium.UpdateLockInLights(Podium.LightOption.LockedIn);
                        else
                            Domination.GetDomination.GetFinalistPodium(p).SetRingColor(CentralPodiumManager.RingColor.LockedIn);
                    }

                    if (!p.eliminated)
                        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.PlayerAnswer);

                    p.currentBid = value;
                }
                break;

            case "ANSWER":
                s = (string)e.Data["ANSWER"];
                ds = JsonConvert.DeserializeObject<string>(s);

                if (!p.eliminated)
                {
                    if (GameControl.GetGameControl.currentRound != GameControl.Round.Domination)
                        p.podium.UpdateLockInLights(Podium.LightOption.LockedIn);
                    else
                        Domination.GetDomination.GetFinalistPodium(p).SetRingColor(CentralPodiumManager.RingColor.LockedIn);
                }

                if (!p.eliminated)
                    AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.PlayerAnswer);

                p.submission = ds;
                p.submissionTime = GlobalTimer.GetTimer.elapsedTime;

                if (Extensions.Spellchecker(p.submission, GameControl.GetGameControl.rounds[(int)GameControl.GetGameControl.currentRound].currentQuestion.validAnswers))
                {
                    if (p.eliminated)
                        DebugLog.Print($"{p.playerName}: {p.submission} ({p.submissionTime})", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
                    else
                        DebugLog.Print($"{p.playerName}: {p.submission} ({p.submissionTime})", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
                }
                else
                {
                    if (p.eliminated)
                        DebugLog.Print($"{p.playerName}: {p.submission} ({p.submissionTime})", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Red);
                    else
                        DebugLog.Print($"{p.playerName}: {p.submission} ({p.submissionTime})", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
                }

                //SendPayloadToClient(p, "DISPLAYRESPONSE", ds + "|" + GlobalTimer.GetTimer.elapsedTime.ToString());
                break;
        }*/

        switch (eventType)
        {
            case EventLibrary.ClientEventType.StoredValidation:
                string[] str = data.Split('|').ToArray();
                TwitchControl.GetTwitchControl.testUsername = str[0];
                TwitchControl.GetTwitchControl.testMessage = str[1];
                TwitchControl.GetTwitchControl.SendTwitchWhisper();
                TwitchControl.GetTwitchControl.testUsername = "";
                TwitchControl.GetTwitchControl.testMessage = "";
                break;

            case EventLibrary.ClientEventType.MultipleChoiceQuestion:
                //string s = (string)e.Data["BID"];
                string s = data.Split('|').ToArray().FirstOrDefault();
                //var ds = JsonConvert.DeserializeObject<string>(s);

                if (int.TryParse(s, out int value))
                {
                    if (!p.eliminated)
                    {
                        if (GameControl.GetGameControl.currentRound != GameControl.Round.Domination)
                            p.podium.UpdateLockInLights(Podium.LightOption.LockedIn);
                        else
                            Domination.GetDomination.GetFinalistPodium(p).SetRingColor(CentralPodiumManager.RingColor.LockedIn);
                    }

                    if (!p.eliminated)
                        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.PlayerAnswer);

                    p.currentBid = value;
                }
                break;

            case EventLibrary.ClientEventType.NumericalQuestion:
                s = data.Split('|').ToArray().FirstOrDefault();
                if (int.TryParse(s, out value))
                {
                    if (!p.eliminated)
                    {
                        if(value < 5 || value > p.points)
                        {
                            SendPayloadToClient(p, EventLibrary.HostEventType.NumericalQuestion,
                                $"<size=50%>{GameControl.nextQuestionIndex}/{QuestionManager.GetRoundQCount()}\n(Bid from 5-{p.points})</size>\n{GameControl.GetGameControl.rounds[(int)GameControl.GetGameControl.currentRound].currentQuestion.category}|7");
                            return;
                        }
                        if (GameControl.GetGameControl.currentRound != GameControl.Round.Domination)
                            p.podium.UpdateLockInLights(Podium.LightOption.LockedIn);
                        else
                            Domination.GetDomination.GetFinalistPodium(p).SetRingColor(CentralPodiumManager.RingColor.LockedIn);
                    }

                    if (!p.eliminated)
                        AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.PlayerAnswer);

                    p.currentBid = value;
                }
                break;

            case EventLibrary.ClientEventType.SimpleQuestion:
                //s = (string)e.Data["ANSWER"];
                s = data.Split('|').ToArray().FirstOrDefault();
                //ds = JsonConvert.DeserializeObject<string>(s);

                if (!p.eliminated)
                {
                    if (GameControl.GetGameControl.currentRound != GameControl.Round.Domination)
                        p.podium.UpdateLockInLights(Podium.LightOption.LockedIn);
                    else
                        Domination.GetDomination.GetFinalistPodium(p).SetRingColor(CentralPodiumManager.RingColor.LockedIn);
                }

                if (!p.eliminated)
                    AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.PlayerAnswer);

                p.submission = s;
                p.submissionTime = GlobalTimer.GetTimer.elapsedTime;

                if (Extensions.Spellchecker(p.submission, GameControl.GetGameControl.rounds[(int)GameControl.GetGameControl.currentRound].currentQuestion.validAnswers))
                {
                    if (p.eliminated)
                        DebugLog.Print($"{p.playerName}: {p.submission} ({p.submissionTime})", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
                    else
                        DebugLog.Print($"{p.playerName}: {p.submission} ({p.submissionTime})", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
                }
                else
                {
                    if (p.eliminated)
                        DebugLog.Print($"{p.playerName}: {p.submission} ({p.submissionTime})", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Red);
                    else
                        DebugLog.Print($"{p.playerName}: {p.submission} ({p.submissionTime})", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Red);
                }

                //SendPayloadToClient(p, "DISPLAYRESPONSE", ds + "|" + GlobalTimer.GetTimer.elapsedTime.ToString());
                SendPayloadToClient(p, EventLibrary.HostEventType.Information, $"Answer received...");
                break;

            case EventLibrary.ClientEventType.PasteAlert:
                //Silent alarm indicating some text has been pasted into an answer box
                DebugLog.Print($"A PASTE ALERT WAS RAISED BY {p.playerName} ({p.twitchName}): {data}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Purple);

                string currentQ = "";
                switch (GameControl.GetGameControl.currentRound)
                {
                    case GameControl.Round.None:
                        currentQ = "No live question";
                        break;

                    case GameControl.Round.OpeningGambits:
                    case GameControl.Round.TopGun:
                    case GameControl.Round.SurvivalOfTheFittest:
                    case GameControl.Round.Domination:
                        if (GameControl.GetGameControl.rounds[(int)GameControl.GetGameControl.currentRound].currentQuestion != null)
                            currentQ = GameControl.GetGameControl.rounds[(int)GameControl.GetGameControl.currentRound].currentQuestion.question;
                        break;
                }
                PasteAlertEvent.Log(p, data, currentQ);
                EventLogger.PrintPasteLog();
                break;
        }
    }

    public PlayerObject GetPlayerFromEvent(EventMessage e)
    {
        return players.FirstOrDefault(x => x.playerClientRef == e.Player);
    }
}
