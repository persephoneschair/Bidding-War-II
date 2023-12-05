using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSManager : MonoBehaviour
{
    #region Init

    public static TTSManager GetTTS { get; private set; }
    private void Awake()
    {
        if (GetTTS != null && GetTTS != this)
            Destroy(this);
        else
            GetTTS = this;
    }

    #endregion

    public string testMessage;
    [Range(0, 10f)] public float testDelay;

    [Button]
    public void TestSpeak()
    {
        if(!string.IsNullOrEmpty(testMessage))
            Speak(testMessage, testDelay);
    }

    public void Speak(string message, float delay = 0f)
    {
        WindowsVoice.speak(message, delay);
    }
}
