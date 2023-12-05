using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardStrap : MonoBehaviour
{
    public int posRef;
    public TextMeshProUGUI posMesh;
    public TextMeshProUGUI nameMesh;
    public TextMeshProUGUI scoreMesh;

    public Image background;
    public Image border;
    public enum ColorOption { Gold = 0, Silver, Bronze, Blue, Inactive, BorderDefault, BorderHighlight, PassageGranted, InDanger };
    public Color[] colors;

    public void Initialise(int pos)
    {
        posMesh.text = "";
        nameMesh.text = "";
        scoreMesh.text = "";
        posRef = pos;
        background.color = colors[(int)ColorOption.Inactive];
        border.color = colors[(int)ColorOption.BorderDefault];
    }

    public void UpdateStrap(string data)
    {
        //[0] = PlayerName
        //[1] = Score
        //[2] = Eliminated
        //[3] = Passage Granted
        //[4] = Danger
        string[] splitData = data.Split('|');
        nameMesh.text = splitData[0];
        posMesh.text = Extensions.AddOrdinal(posRef);

        if (splitData[2] == "TRUE")
            background.color = colors[(int)ColorOption.Inactive];
        else if (splitData[3] == "TRUE")
            background.color = colors[(int)ColorOption.PassageGranted];
        else if (splitData[4] == "TRUE")
            background.color = colors[(int)ColorOption.InDanger];
        else
        {
            switch (posRef)
            {
                case 1:
                    background.color = colors[(int)ColorOption.Gold];
                    break;

                case 2:
                    background.color = colors[(int)ColorOption.Silver];
                    break;

                case 3:
                    background.color = colors[(int)ColorOption.Bronze];
                    break;

                default:
                    background.color = colors[(int)ColorOption.Blue];
                    break;
            }
        }
        scoreMesh.text = splitData[1];

        border.color =
            ClientManager.GetClient.mainGame.playerNameMesh.text.ToUpperInvariant() == splitData[0].ToUpperInvariant()
            ? colors[(int)ColorOption.BorderHighlight]
            : border.color = colors[(int)ColorOption.BorderDefault];
    }
}
