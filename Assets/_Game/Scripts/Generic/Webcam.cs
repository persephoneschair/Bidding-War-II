using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Webcam : MonoBehaviour
{
    public Renderer otherCam;

    void Start()
    {
        SwitchCamOn();
    }

    public void SwitchCamOn()
    {
        WebCamTexture webcamTexture = new WebCamTexture();
        GetComponent<Renderer>().material.mainTexture = webcamTexture;
        otherCam.material.mainTexture = webcamTexture;
        webcamTexture.Play();
    }
}
