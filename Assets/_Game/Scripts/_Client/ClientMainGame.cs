using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class ClientMainGame : MonoBehaviour
{
    [Header("Data Fields")]
    public TextMeshProUGUI playerNameMesh;
    public TextMeshProUGUI scoreMesh;
    public TextMeshProUGUI currentBidMesh;
    public TextMeshProUGUI responseMesh;
    public TextMeshProUGUI timeMesh;

    [Header("R1 Bidding")]
    public GameObject r1BidArray;
    public Button[] r1BidButtons;

    [Header("R2 Bidding")]
    public GameObject r2BidArray;
    public Button[] r2BidButtons;

    [Header("R3 Bidding")]
    public R3BidManager r3BidManager;

    [Header("General Question")]
    public GameObject countdownObj;
    public TextMeshProUGUI countdownMesh;
    public GameObject catObj;
    public TextMeshProUGUI catMesh;
    public GameObject qObj;
    public TextMeshProUGUI qMesh;
    public GameObject ansInputObj;
    public TMP_InputField answerInput;
    public bool enterSubmits;
    public GameObject submitObj;
    public Button submitButton;
    public Animator timerAnim;
    public GameObject ansObj;
    public TextMeshProUGUI ansMesh;

    public Image answerBackground;
    public Image answerBorder;
    public Color[] backgroundCols;
    public Color[] borderCols;

    [Header("Misc")]
    public GameObject fixedMessageObj;
    public TextMeshProUGUI fixedMessageMesh;

    private void Update()
    {
        submitButton.interactable = answerInput.text.Length <= 0 ? false : true;

        if (enterSubmits && answerInput.text.Length > 0 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            OnSubmitAnswer();
    }

    public void KillOTPBox(string[] otpArr)
    {
        LeaderboardManager.GetLeaderboard.holder.SetActive(true);
        LeaderboardManager.GetLeaderboard.RefreshScrollRect();

        r1BidArray.SetActive(false);
        r2BidArray.SetActive(false);
        r3BidManager.gameObject.SetActive(false);
        countdownObj.gameObject.SetActive(false);
        catObj.gameObject.SetActive(false);
        qObj.SetActive(false);
        ansInputObj.SetActive(false);
        submitObj.SetActive(false);
        ansObj.SetActive(false);

        playerNameMesh.text = otpArr[0];
        scoreMesh.text = "Score: " + otpArr[1];
    }

    public IEnumerator GeneralCountdown(bool bid)
    {
        countdownObj.SetActive(true);
        for(int i = 3; i >= 1; i--)
        {
            countdownMesh.text = bid ? "BIDDING OPENS IN " + i.ToString() + "..." : "QUESTION OPENS IN " + i.ToString() + "...";
            yield return new WaitForSeconds(1f);
        }
        yield return new WaitForSeconds(0.25f);
        countdownObj.SetActive(false);
        timerAnim.SetTrigger("toggle");
        timerAnim.speed = bid ? 2 : 1;
    }

    public void ProcessR1BidOpening(string[] data)
    {
        r1BidArray.SetActive(true);
        if (int.TryParse(data[0], out int score))
            for (int i = 0; i < 3; i++)
                r1BidButtons[i].interactable = i < score ? true : false;
        catObj.SetActive(true);
        catMesh.text = $"<size=50%><u>CATEGORY {data[2]}</u></size>\n{data[1]}";
    }

    public void ProcessR2BidOpening(string[] data)
    {
        r2BidArray.SetActive(true);

        //Hide buttons lower than maximum bid
        if (int.TryParse(data[2], out int maxBid))
        {
            for (int i = 0; i < r2BidButtons.Length; i++)
                r2BidButtons[i].gameObject.SetActive(i < maxBid ? true : false);
        }

        //Now disable buttons if the player score is less than that option
        if (int.TryParse(data[0], out int score) && int.TryParse(data[2], out int max2))
            for (int i = max2 - 1; i >= 0; i--)
                r2BidButtons[i].interactable = score < i + 1 ? false : true;

        catObj.SetActive(true);
        catMesh.text = $"<size=50%><u>CATEGORY {data[3]}</u></size>\n{data[1]}";
    }

    public void ProcessR3BidOpening(string[] data)
    {
        r3BidManager.gameObject.SetActive(true);
        if (int.TryParse(data[0], out int score))
            r3BidManager.OnNewCategory(score);

        catObj.SetActive(true);
        catMesh.text = $"<size=50%><u>CATEGORY {data[2]}</u></size>\n{data[1]}";
    }

    public void ProcessR4BidOpening(string[] data)
    {
        r1BidArray.SetActive(true);
        for (int i = 0; i < 3; i++)
            r1BidButtons[i].interactable = true;
        catObj.SetActive(true);
        catMesh.text = $"<size=50%><u>CATEGORY  {data[2]} </u></size>\n{data[1]}";
    }

    public void ProcessR1BidOpeningElim(string[] data)
    {
        catObj.SetActive(true);
        catMesh.text = $"<size=50%><u>CATEGORY{data[1]}</u></size>\n{data[0]}";
    }

    public void OnPlaceBid(int bidValue)
    {
        r1BidArray.SetActive(false);
        r2BidArray.SetActive(false);
        currentBidMesh.text = "Bid: " + bidValue.ToString();
        ClientManager.GetClient.SendPayloadToHost(bidValue);
    }

    public void OnPlaceBidR3()
    {
        currentBidMesh.text = "Bid: " + r3BidManager.slider.value.ToString();
        ClientManager.GetClient.SendPayloadToHost((int)r3BidManager.slider.value);
        r3BidManager.gameObject.SetActive(false);
    }

    public void KillR1BidBox(string data)
    {
        r1BidArray.SetActive(false);
    }

    public void KillR2BidBox(string data)
    {
        r2BidArray.SetActive(false);
    }
    public void KillR3BidBox(string data)
    {
        r3BidManager.gameObject.SetActive(false);
    }

    public void UpdateCurrentBid(string data)
    {
        currentBidMesh.text = $"Bid: {data}";
    }

    public void DisplayQuestion(string data)
    {
        enterSubmits = true;
        qObj.SetActive(true);
        qMesh.text = data;
        ansInputObj.SetActive(true);
        answerInput.ActivateInputField();
        submitObj.SetActive(true);
    }

    public void OnSubmitAnswer()
    {
        ClientManager.GetClient.SendPayloadToHost(answerInput.text.Trim());
        KillAnswerArray();
    }

    public void KillAnswerArray()
    {
        enterSubmits = false;
        submitObj.SetActive(false);
        ansInputObj.SetActive(false);
        answerInput.text = "";
    }

    public void DisplayResponse(string[] data)
    {
        responseMesh.text = data[0];
        if (float.TryParse(data[1], out float result))
            timeMesh.text = result.ToString("##.00") + "s";
    }

    public void DisplayAnswer(string[] data)
    {
        ansObj.SetActive(true);
        ansMesh.text = data[0];
        SetAnswerBoxColor(data[1].ToUpperInvariant() == "TRUE" ? true : false);
    }

    public void UpdateCurrentScore(string data)
    {
        scoreMesh.text = $"Score: {data}";
        currentBidMesh.text = "<color=#14008C>CURRENT BID";
    }

    public void UpdateCurrentCorrect(string data)
    {
        scoreMesh.text = $"Correct: {data}";
        currentBidMesh.text = "<color=red>You're out of the main game, but keep playing along to earn Pennys!";
    }

    public void ResetForNewQuestion()
    {
        catObj.SetActive(false);
        qObj.SetActive(false);
        ansObj.SetActive(false);
        currentBidMesh.text = "<color=#14008C>CURRENT BID";
        responseMesh.text = "<color=#14008C>RESPONSE";
        timeMesh.text = "<color=#14008C>RESPONSE TIME";
    }

    public void NewInstanceOpened()
    {
        fixedMessageObj.SetActive(true);
        fixedMessageMesh.text = "THE TWITCH ACCOUNT ASSOCIATED WITH THIS CONTROLLER HAS BEEN ASSOCIATED WITH ANOTHER CONTROLLER.\n\n" +
            "THIS CONTROLLER WILL NOT RECEIVE FURTHER DATA FROM THE GAME AND CAN NOW BE CLOSED.\n\n" +
            "IF YOU DID  NOT VALIDATE A NEW CONTROLLER, PLEASE CONTACT THE HOST.";
    }

    public void EndOfGameAlert(string pennyValue)
    {
        fixedMessageObj.SetActive(true);
        fixedMessageMesh.text = "THE GAME HAS NOW CONCLUDED AND THIS CONTROLLER CAN BE CLOSED.\n\n" +
            $"YOU WILL RECEIVE A TOTAL OF {pennyValue} PENNYS.\n\n" +
            "THANKS FOR PLAYING\n" +
            "<font=LogoFont><color=red>BIDDING WAR</color></font>.";
    }

    public void SetAnswerBoxColor(bool wasCorrect)
    {
        answerBackground.color = backgroundCols[wasCorrect ? 0 : 1];
        answerBorder.color = borderCols[wasCorrect ? 0 : 1];
    }
}
