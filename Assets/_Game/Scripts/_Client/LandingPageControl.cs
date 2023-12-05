using System.Collections;
using System.Collections.Generic;
using TMPro;
using TwitchLib.Client.Models;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LandingPageControl : MonoBehaviour
{
    public ClientManager client;

    public TextMeshProUGUI versionMesh;
    public TMP_InputField nameInput;
    public TMP_InputField roomCodeInput;
    public Button joinButton;

    public TextMeshProUGUI downloadButtonMesh;
    private string downloadLink;

    private void Update()
    {
        joinButton.interactable = (nameInput.text == "" || roomCodeInput.text.Length != 4) ? false : true;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (nameInput.isFocused)
                roomCodeInput.ActivateInputField();
            else if(roomCodeInput.isFocused)
                nameInput.ActivateInputField();
        }

        if (joinButton.interactable && (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)))
            OnJoinRoom();
    }

    private void Start()
    {
        versionMesh.text = versionMesh.text.Replace("[##]", Application.version);
        ChangeLabel();
    }

    public void OnJoinRoom()
    {
        client.AttemptToConnectToRoom(nameInput.text.ToUpperInvariant(), roomCodeInput.text.ToUpperInvariant());
        roomCodeInput.text = "";
        joinButton.interactable = false;
    }

    public void RefreshLandingPage()
    {
        this.gameObject.SetActive(true);
        nameInput.text = "";
        roomCodeInput.text = "";
    }

    public void OnDownloadStandaloneBuild()
    {
        Application.OpenURL(downloadLink);
    }

    public void ChangeLabel()
    {
#if UNITY_STANDALONE
        downloadButtonMesh.text = "GO TO BROWSER VERSION";
        downloadLink = "https://persephoneschair.itch.io/biddingwar";
#endif

#if UNITY_WEBGL || UNITY_EDITOR
        downloadButtonMesh.text = "DOWNLOAD PC VERSION";
        downloadLink = "https://persephoneschair.itch.io/biddingwarpc";
#endif
    }
}
