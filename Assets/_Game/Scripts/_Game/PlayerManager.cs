using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Init

    public static PlayerManager GetPlayerManager { get; private set; }

    private void Awake()
    {
        if (GetPlayerManager != null && GetPlayerManager != this)
            Destroy(this);
        else
            GetPlayerManager = this;
    }

    #endregion

    [Header("Controls")]
    public bool pullingData = true;
    [Range(0,39)] public int playerIndex;


    private PlayerObject _focusPlayer;
    public PlayerObject FocusPlayer
    {
        get { return _focusPlayer; }
        set
        {
            if(value != null)
            {
                _focusPlayer = value;
                playerName = value.playerName;
                twitchName = value.twitchName;
                profileImage = value.profileImage;
                flagForCondone = value.flagForCondone;
                wasCorrect = value.wasCorrect;
                eliminated = value.eliminated;

                points = value.points;
                totalCorrect = value.totalCorrect;
                currentBid = value.currentBid;
                maxPoints = value.maxPoints;
                submission = value.submission;
                submissionTime = value.submissionTime;
            }
            else
            {
                playerName = "OUT OF RANGE";
                twitchName = "OUT OF RANGE";
                profileImage = null;
                flagForCondone = false;
                wasCorrect = false;
                eliminated = false;

                points = 0;
                totalCorrect = 0;
                currentBid = 0;
                maxPoints = 0;
                submission = "OUT OF RANGE";
                submissionTime = 0;
            }                
        }
    }

    [Header("Fixed Fields")]
    [ShowOnly] public string playerName;
    [ShowOnly] public string twitchName;
    public Texture profileImage;
    [ShowOnly] public bool flagForCondone;
    [ShowOnly] public bool wasCorrect;
    [ShowOnly] public bool eliminated;

    [Header("Variable Fields")]
    public int points;
    public int totalCorrect;
    public int currentBid;
    public int maxPoints;
    public string submission;
    public float submissionTime;

    void UpdateDetails()
    {
        if (playerIndex >= HostManager.GetHost.players.Count)
            FocusPlayer = null;
        else
            FocusPlayer = HostManager.GetHost.players.OrderBy(x => x.playerName).ToList()[playerIndex];
    }

    private void Update()
    {
        if (pullingData)
            UpdateDetails();
    }

    [Button]
    public void SetPlayerDetails()
    {
        if (pullingData)
            return;
        SetDataBack();
    }

    [Button]
    public void RestoreOrEliminatePlayer()
    {
        if (pullingData)
            return;

        if(!FocusPlayer.eliminated)
            FocusPlayer.podium.HardEliminate();
        else
        {
            FocusPlayer.eliminated = false;
            FocusPlayer.podium.RestorePlayer();
            FocusPlayer.podium.responseMesh.text = "";
            SetDataBack();
        }
        pullingData = true;

    }

    void SetDataBack()
    {
        FocusPlayer.podium.R3Iterate(points >= FocusPlayer.points ? true : false, points);
        FocusPlayer.points = points;
        FocusPlayer.totalCorrect = totalCorrect;
        FocusPlayer.currentBid = currentBid;
        FocusPlayer.maxPoints = maxPoints;
        FocusPlayer.submission = submission;
        FocusPlayer.submissionTime = submissionTime;
        pullingData = true;
    }
}
