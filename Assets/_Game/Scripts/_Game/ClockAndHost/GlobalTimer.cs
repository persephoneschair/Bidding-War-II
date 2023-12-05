using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTimer : MonoBehaviour
{

    #region Init

    public static GlobalTimer GetTimer { get; private set; }
    private void Awake()
    {
        if (GetTimer != null && GetTimer != this)
            Destroy(this);
        else
            GetTimer = this;
    }

    #endregion

    public bool questionClockRunning;
    [ShowOnly] public float elapsedTime;

    private void Update()
    {
        if (questionClockRunning)
            QuestionTimer();
        else
            elapsedTime = 0;
    }

    void QuestionTimer()
    {
        elapsedTime += (1f * Time.deltaTime);
    }
}
