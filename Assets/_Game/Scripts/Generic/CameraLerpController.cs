using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLerpController : MonoBehaviour
{
    private Camera cam;

    public Transform[] angles;
    
    private Vector3 defaultStartPos;
    private Vector3 defaultStartRot;
    private float defaultStartFov;

    public Transform fullLobbyStartTransform;
    public float fullLobbyStartFov = 69;

    public float defaultZoomOut = 5f;
    public float defaultCrashZoom = 0.5f;

    private float elapsedTime;
    private Vector3 startPos;
    private Vector3 startRot;
    private float startFov;
    private Vector3 endPos;
    private Vector3 endRot;
    private float endFov;
    private bool isMoving;
    private float duration = 0.5f;

    public bool performingAZoom;

    #region Init

    public static CameraLerpController GetCam { get; private set; }

    private void Awake()
    {
        if (GetCam != null && GetCam != this)
            Destroy(this);
        else
            GetCam = this;

        cam = GetComponent<Camera>();
        defaultStartPos = cam.transform.position;
        defaultStartRot = cam.transform.localEulerAngles;
        defaultStartFov = cam.fieldOfView;
    }

    #endregion

    [Button]
    public void TopRowOccupied()
    {
        defaultStartPos = fullLobbyStartTransform.localPosition;
        defaultStartRot = fullLobbyStartTransform.localEulerAngles;
        defaultStartFov = fullLobbyStartFov;

        duration = defaultZoomOut;
        endPos = defaultStartPos;
        endRot = defaultStartRot;
        endFov = defaultStartFov;

        startPos = this.transform.position;
        startRot = this.transform.localEulerAngles;
        startFov = cam.fieldOfView;

        elapsedTime = 0;
        isMoving = true;
        Invoke("EndLock", duration);
        Invoke("EndZoom", duration);
    }

    public void ZoomOnPodium(Podium pod)
    {
        performingAZoom = true;
        cam.gameObject.transform.parent = pod.gameObject.transform;
        
        cam.gameObject.transform.localPosition = new Vector3(0, 0.3f, -2f);
        cam.gameObject.transform.localEulerAngles = new Vector3(10, 0, 0);
        //set fov

        cam.gameObject.transform.parent = null;        
        endPos = cam.transform.localPosition;
        endRot = cam.transform.localEulerAngles;
        endFov = cam.fieldOfView;

        cam.gameObject.transform.localPosition = defaultStartPos;
        cam.gameObject.transform.localEulerAngles = defaultStartRot;
        cam.fieldOfView = defaultStartFov;

        startPos = this.transform.localPosition;
        startRot = this.transform.localEulerAngles;
        startFov = cam.fieldOfView;

        elapsedTime = 0;
        duration = defaultCrashZoom;
        isMoving = true;
        Invoke("EndLock", duration);
        Invoke("ZoomToDefault", duration + defaultZoomOut + defaultZoomOut);
    }

    [Button]
    public void ZoomToFinal()
    {
        performingAZoom = true;

        startPos = defaultStartPos;
        startRot = defaultStartRot;
        startFov = defaultStartFov;

        endPos = angles[0].transform.localPosition;
        endRot = angles[0].transform.localEulerAngles;
        endFov = 71;

        elapsedTime = 0;
        duration = 10f;
        isMoving = true;
        Invoke("EndLock", duration);
        Invoke("EndZoom", duration);
    }

    [Button]
    public void ZoomToChampion()
    {
        performingAZoom = true;

        startPos = this.transform.localPosition;
        startRot = this.transform.localEulerAngles;
        startFov = cam.fieldOfView;

        endPos = angles[1].transform.localPosition;
        endRot = angles[1].transform.localEulerAngles;
        endFov = 37;

        elapsedTime = 0;
        duration = 4f;
        isMoving = true;
        Invoke("EndLock", duration);
        Invoke("EndZoom", duration);
    }

    [Button]
    private void ZoomToDefault()
    {
        duration = defaultZoomOut;
        endPos = startPos;
        endRot = startRot;
        endFov = startFov;

        startPos = this.transform.position;
        startRot = this.transform.localEulerAngles;
        startFov = cam.fieldOfView;

        elapsedTime = 0;
        isMoving = true;
        Invoke("EndLock", duration);
        Invoke("EndZoom", duration);
    }

    private void Update()
    {
        if (isMoving)
            PerformLerp();
    }

    private void PerformLerp()
    {
        elapsedTime += Time.deltaTime;

        float percentageComplete = elapsedTime / duration;

        this.gameObject.transform.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, percentageComplete));

        float x = Mathf.LerpAngle(startRot.x, endRot.x, Mathf.SmoothStep(0, 1, percentageComplete));
        float y = Mathf.LerpAngle(startRot.y, endRot.y, Mathf.SmoothStep(0, 1, percentageComplete));
        float z = Mathf.LerpAngle(startRot.z, endRot.z, Mathf.SmoothStep(0, 1, percentageComplete));
        this.gameObject.transform.localEulerAngles = new Vector3(x, y, z);//Vector3.Lerp(startRot, endRot, Mathf.SmoothStep(0, 1, percentageComplete));

        cam.fieldOfView = Mathf.Lerp(startFov, endFov, Mathf.SmoothStep(0, 1, percentageComplete));
    }

    public void EndLock()
    {
        isMoving = false;
    }

    public void EndZoom()
    {
        performingAZoom = false;
    }
}
