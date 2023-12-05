using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Podium;

public class CentralPodiumManager : MonoBehaviour
{
    public enum RingColor { Default, LockedIn, Correct, Incorrect };
    public Material[] ringColors;
    public Renderer ringRend;

    public Renderer[] bidLights;
    public Material[] bidMats;

    public Animator podiumAnim;
    public Animator lidAnim;
    public GameObject steam;

    public Animator hostCamAnim;

    public PlayerObject containedPlayer;
    public Renderer avatarRend;
    public TextMeshPro playerNameMesh;
    public TextMeshProUGUI answerMesh;

    public Collider col;

    public void OnEndOfTitles()
    {
        StartCoroutine(Emerge());
    }

    private void Start()
    {
        if(bidLights.Length > 0)
            SetBidLights(3);
    }

    public IEnumerator Emerge()
    {
        yield return new WaitForSeconds(1f);
        steam.SetActive(true);
        lidAnim.SetTrigger("toggle");
        TogglePodium();
    }

    public void TogglePodium()
    {
        podiumAnim.SetTrigger("toggle");
    }

    public void SetUpPlayerPodium(PlayerObject po)
    {
        containedPlayer = po;
        avatarRend.material.mainTexture = po.profileImage;
        playerNameMesh.text = po.playerName;
        col.enabled = true;
        StartCoroutine(Emerge());
        SetBidLights(0);
    }

    public void OnMouseDown()
    {
        if (containedPlayer == null)
            return;

        if (GameControl.GetGameControl.currentStage != GameControl.GameplayStage.R4CalculateLightArray)
            return;

        if (containedPlayer.flagForCondone && !containedPlayer.wasCorrect)
        {
            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.PlayerAnswer);
            containedPlayer.flagForCondone = false;
            containedPlayer.wasCorrect = true;
            containedPlayer.totalCorrect++;
            SetRingColor(RingColor.Correct);
        }
        else if (!containedPlayer.flagForCondone && containedPlayer.wasCorrect)
        {
            AudioManager.GetAudioManager.Play(AudioManager.OneShotClip.WrongInFinal);
            containedPlayer.flagForCondone = true;
            containedPlayer.wasCorrect = false;
            containedPlayer.totalCorrect--;
            SetRingColor(RingColor.Incorrect);
        }
        //HostManager.GetHost.UpdateLeaderboards();
    }

    public void SetRingColor(RingColor col)
    {
        ringRend.material = ringColors[(int)col];
    }

    public void SetBidLights(int bid, int playerIndex = 1)
    {
        for (int i = 0; i < bid; i++)
            bidLights[i].material = bidMats[playerIndex];

        for(int i = bid; i < bidLights.Length; i++)
            bidLights[i].material = bidMats[0];
    }
}
