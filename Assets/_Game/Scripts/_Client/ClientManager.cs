using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Control;
using TMPro;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class ClientManager : MonoBehaviour
{
    public Client client;
    public LandingPageControl landingPage;
    public ClientMainGame mainGame;

    public GameObject loadingBox;
    public TextMeshProUGUI loadingText;

    public GameObject alertBox;
    public TextMeshProUGUI alertText;
    public TextMeshProUGUI alertButtonText;

    [Header("OTP Fields")]
    public GameObject otpAlert;
    public TextMeshProUGUI otpMesh;

    #region Init

    public static ClientManager GetClient { get; private set; }
    private void Awake()
    {
        if (GetClient != null && GetClient != this)
            Destroy(this);
        else
            GetClient = this;
    }

    #endregion

    #region Connection

    public void AttemptToConnectToRoom(string name, string roomCode)
    {
        client.Connect(name + "¬BIDDINGWAR", roomCode);
        landingPage.gameObject.SetActive(false);
        loadingBox.SetActive(true);
        loadingText.text = "Attempting connection to host server...";
    }

    public void OnConnected(string roomCode)
    {
        Invoke("CheckConnection", 2f);
    }

    void CheckConnection()
    {
        if (!client.Connected)
        {
            loadingBox.SetActive(false);
            alertButtonText.text = "OK";
            alertBox.SetActive(true);
            alertText.text = "<color=red>Connection could not be established.\n\nPlease check the room code and try again.";
        }
    }

    public void OnCloseAlert()
    {
        alertBox.SetActive(false);
        alertButtonText.text = "OK";
        if (!client.Connected)
            landingPage.RefreshLandingPage();

        /*else if(client.Connected && mainGame.playerNameMesh.text.Length > 0)
        {
            mainGame.gameObject.SetActive(true);
            LeaderboardManager.GetLeaderboard.holder.SetActive(true);
            LeaderboardManager.GetLeaderboard.RefreshScrollRect();
        }*/
    }

    public void OnValidateAccount(string data)
    {
        loadingBox.SetActive(false);
        otpMesh.text = otpMesh.text.Replace("[ABCD]", data);
        otpAlert.SetActive(true);
    }

    #endregion

    public void OnPayloadReceived(DataMessage dm)
    {
        string data = (string)dm.Data;

        switch (dm.Key)
        {
            case "VALIDATE":
                OnValidateAccount(data);
                break;

            case "KILLOTP":
                /*alertButtonText.text = "PLAY";
                alertBox.SetActive(true);
                alertText.text = "Connection established.\n\nWelcome to\n<color=red><font=LogoFont>Bidding War</font></color>...";*/
                mainGame.gameObject.SetActive(true);
                GetClient.otpAlert.SetActive(false);
                mainGame.KillOTPBox(data.Split('|'));
                break;

            case "LEADERBOARD":
                string[] splitData = data.Split('¬');
                LeaderboardManager.GetLeaderboard.UpdateLeaderboard(splitData);
                break;


            case "COUNTDOWNBID":
                StartCoroutine(mainGame.GeneralCountdown(true));
                break;

            case "COUNTDOWNQ":
                StartCoroutine(mainGame.GeneralCountdown(false));
                break;


            case "R1BIDELIM":
                mainGame.ProcessR1BidOpeningElim(data.Split('|'));
                break;

            case "R1BID":
                mainGame.ProcessR1BidOpening(data.Split('|'));
                break;

            case "R2BID":
                mainGame.ProcessR2BidOpening(data.Split('|'));
                break;

            case "R3BID":
                mainGame.ProcessR3BidOpening(data.Split('|'));
                break;

            case "R4BID":
                mainGame.ProcessR4BidOpening(data.Split('|'));
                break;


            case "R1KILLBIDBOX":
                mainGame.KillR1BidBox(data);
                break;

            case "R2KILLBIDBOX":
                mainGame.KillR2BidBox(data);
                break;

            case "R3KILLBIDBOX":
                mainGame.KillR3BidBox(data);
                break;


            case "CURRENTBID":
                mainGame.UpdateCurrentBid(data);
                break;

            case "QUES":
                mainGame.DisplayQuestion(data);
                break;

            case "DISPLAYRESPONSE":
                mainGame.DisplayResponse(data.Split('|'));
                break;

            case "KILLANSWERARRAY":
                mainGame.KillAnswerArray();
                break;

            case "DISPLAYANSWER":
                mainGame.DisplayAnswer(data.Split('|'));
                break;

            case "CURRENTSCORE":
                mainGame.UpdateCurrentScore(data);
                break;

            case "CURRENTCORRECT":
                mainGame.UpdateCurrentCorrect(data);
                break;

            case "RESETFORNEWQ":
                mainGame.ResetForNewQuestion();
                break;


            case "ALTCONNECTION":
                mainGame.NewInstanceOpened();
                break;

            case "ENDOFGAME":
                mainGame.EndOfGameAlert(data);
                break;

            case "WRONGAPP":
            case "WrongApp":
                WrongApp();
                break;
        }
    }

    public void WrongApp()
    {
        mainGame.fixedMessageObj.SetActive(true);
        mainGame.fixedMessageMesh.text = "YOU ARE ATTEMPTING TO PLAY THE GAME USING THE WRONG CONTROLLER APP.\n\n" +
            "PLEASE CHECK THE GAME PAGE FOR THE CORRECT LINK TO THE CURRENT GAME.";
    }

    public void SendPayloadToHost(int payload)
    {
        var js = JsonConvert.SerializeObject(payload.ToString());
        JObject j = new JObject
        {
            { "BID", js }
        };
        client.SendEvent("BID", j);
    }

    public void SendPayloadToHost(string payload)
    {
        var js = JsonConvert.SerializeObject(payload);
        JObject j = new JObject
        {
            { "ANSWER", js }
        };
        client.SendEvent("ANSWER", j);
    }
}
