using Control;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerObject
{
    public PlayerObject(Player pl)
    {
        playerClientRef = pl;
        otp = OTPGenerator.GenerateOTP();
        playerName = pl.Name;
        points = 5;
        maxPoints = 5;
        podium = Podiums.GetPodiums.podia.FirstOrDefault(x => x.containedPlayer == null);
        podium.containedPlayer = this;
    }

    public void ApplyProfilePicture(string name, Texture tx, bool bypassSwitchAccount = false)
    {
        //Player refreshs and rejoins the same game
        if(HostManager.GetHost.players.Count(x => (!string.IsNullOrEmpty(x.twitchName)) && x.twitchName.ToLowerInvariant() == name.ToLowerInvariant()) > 0 && !bypassSwitchAccount)
        {
            PlayerObject oldPlayer = HostManager.GetHost.players.FirstOrDefault(x => x.twitchName.ToLowerInvariant() == name.ToLowerInvariant());
            if (oldPlayer == null)
                return;

            HostManager.GetHost.SendPayloadToClient(oldPlayer, EventLibrary.HostEventType.SecondInstance, "");

            oldPlayer.playerClientID = playerClientID;
            oldPlayer.playerClientRef = playerClientRef;
            oldPlayer.playerName = playerName;
            oldPlayer.podium.playerNameMesh.text = playerName;

            otp = "";
            podium.containedPlayer = null;
            podium = null;
            playerClientRef = null;
            playerName = "";

            HostManager.GetHost.players.Remove(this);
            HostManager.GetHost.SendPayloadToClient(oldPlayer, EventLibrary.HostEventType.Validated, $"{oldPlayer.playerName}|Points: {oldPlayer.points.ToString()}|{oldPlayer.twitchName.ToString()}");
            //HostManager.GetHost.UpdateLeaderboards();
            return;
        }
        otp = "";
        twitchName = name.ToLowerInvariant();
        profileImage = tx;
        podium.avatarRend.material.mainTexture = profileImage;
        if(!LobbyManager.GetLobby.lateEntry)
            podium.InitialisePodium();
        else
        {
            points = 0;
            eliminated = true;
        }
        HostManager.GetHost.SendPayloadToClient(this, EventLibrary.HostEventType.Validated, $"{playerName}|Points: {points.ToString()}|{twitchName.ToString()}");
        HostManager.GetHost.players.Add(this);
        HostManager.GetHost.waitingRoom.Remove(this);
        //HostManager.GetHost.UpdateLeaderboards();
    }

    public string playerClientID;
    public Player playerClientRef;
    public Podium podium;
    public string otp;
    public string playerName;

    public string twitchName;
    public Texture profileImage;

    public bool eliminated;
    public bool passageGranted;
    public bool passageJustGranted;
    public bool wasCorrect;

    public int points;
    public int maxPoints;
    public int totalCorrect;
    public int currentBid;
    public string submission;
    public float submissionTime;
    public bool flagForCondone;
}
