using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectSerializable
{
    public string playerClientID;
    public string playerName;

    public string twitchName;

    public bool eliminated;
    public bool passageGranted;

    public int points;
    public int maxPoints;
    public int totalCorrect;
}
