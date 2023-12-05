using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{

    #region Init

    public static LobbyManager GetLobby { get; private set; }
    private void Awake()
    {
        if (GetLobby != null && GetLobby != this)
            Destroy(this);
        else
            GetLobby = this;
    }

    #endregion

    public bool lateEntry;

    public TextMeshProUGUI welcomeMessageMesh;
    public Animator lobbyCodeAnim;
    private const string welcomeMessage = "Welcome to <color=red><font=LogoFont>BIDDING WAR</font></color>. To join the game, please visit <color=green>https://persephoneschair.itch.io/gamenight</color> and join with the room code <color=yellow>[ABCD]</color>."/*\n\n" +
        "In order to qualify for Pennys and medals, you will need to whisper a one-time password to <color=yellow>persephones_twitch_bot</color> upon joining the game room."*/;

    public Animator permaCodeAnim;
    public TextMeshProUGUI permaCodeMesh;

    [Button]
    public void OnOpenLobby()
    {
        TTSManager.GetTTS.Speak($"Welcome to Bidding War. To join the game, please visit persephones chair.itch.io/game night, and join with the room code {HostManager.GetHost.host.RoomCode.ToUpperInvariant()}");
        lobbyCodeAnim.SetTrigger("toggle");
        welcomeMessageMesh.text = welcomeMessage.Replace("[ABCD]", HostManager.GetHost.host.RoomCode.ToUpperInvariant());
    }

    [Button]
    public void OnLockLobby()
    {
        lateEntry = true;
        lobbyCodeAnim.SetTrigger("toggle");
        //HostManager.GetHost.UpdateLeaderboards();
        permaCodeMesh.text = permaCodeMesh.text.Replace("[ABCD]", HostManager.GetHost.host.RoomCode.ToUpperInvariant());
        Invoke("TogglePermaCode", 1f);
    }

    public void TogglePermaCode()
    {
        permaCodeAnim.SetTrigger("toggle");
    }
}
