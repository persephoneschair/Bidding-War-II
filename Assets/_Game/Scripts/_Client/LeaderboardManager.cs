using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    public LeaderboardStrap[] straps;
    public GameObject holder;
    public ScrollRect scrollRect;

    #region Init

    public static LeaderboardManager GetLeaderboard { get; private set; }
    private void Awake()
    {
        if (GetLeaderboard != null && GetLeaderboard != this)
            Destroy(this);
        else
            GetLeaderboard = this;
    }

    #endregion

    public void Start()
    {
        for (int i = 0; i < straps.Length; i++)
            straps[i].Initialise(i + 1);
        this.gameObject.transform.SetParent(ClientManager.GetClient.mainGame.gameObject.transform, false);
        holder.gameObject.SetActive(false);
    }

    public void UpdateLeaderboard(string[] data)
    {
        for (int i = 0; i < data.Length; i++)
            straps[i].UpdateStrap(data[i]);
    }

    public void RefreshScrollRect()
    {
        scrollRect.GetComponent<RectTransform>().localPosition = new Vector3(0, -650f, 0);
    }
}
