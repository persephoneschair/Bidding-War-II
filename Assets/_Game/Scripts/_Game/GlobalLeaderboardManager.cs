using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobalLeaderboardManager : MonoBehaviour
{
    #region Init

    public static GlobalLeaderboardManager GetLeaderboard { get; private set; }
    private void Awake()
    {
        if (GetLeaderboard != null && GetLeaderboard != this)
            Destroy(this);
        else
            GetLeaderboard = this;
    }

    #endregion

    public Animator leaderboardAnim;
    public GlobalLeaderboardStrap[] straps;

    private void Start()
    {
        for (int i = 0; i < straps.Length; i++)
            straps[i].Position = i;
    }

    public void PopulateLeaderboard()
    {
        List<PlayerObject> ordered = HostManager.GetHost.players.OrderBy(x => x.eliminated).ThenByDescending(x => x.points).ThenByDescending(x => x.totalCorrect).ThenByDescending(x => x.maxPoints).ToList();
        for(int i = 0; i < straps.Length; i++)
        {
            if (i >= ordered.Count)
                straps[i].KillStrap();
            else
                straps[i].PopulateStrap(ordered[i]);
        }
        ToggleLeaderboard();
        //HostManager.GetHost.UpdateLeaderboards();
    }

    public void ToggleLeaderboard()
    {
        leaderboardAnim.SetTrigger("toggle");
    }
}
