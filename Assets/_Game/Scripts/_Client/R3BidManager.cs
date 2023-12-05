using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class R3BidManager : MonoBehaviour
{
    public Slider slider;
    public TMP_InputField currentBidMeshInput;
    public Button submitButton;

    private void Start()
    {
        OnValueChanged();
    }

    private void Update()
    {
        if (int.TryParse(currentBidMeshInput.text, out int value) && slider.value == value)
            submitButton.interactable = true;
        else
            submitButton.interactable = false;

        if ((Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) && submitButton.interactable)
            ClientManager.GetClient.mainGame.OnPlaceBidR3();
    }

    public void OnValueChanged()
    {
        currentBidMeshInput.text = slider.value.ToString();
    }

    public void OnTypeNewValue()
    {
        if (int.TryParse(currentBidMeshInput.text, out int value))
        {
            if (value < 5)
                return;

            else if(value > slider.maxValue)
            {
                slider.value = slider.maxValue;
                currentBidMeshInput.text = slider.maxValue.ToString();
            }
            else
                slider.value = value;
        }                
    }

    public void OnQuickButton(int buttonRef)
    {
        switch(buttonRef)
        {
            case 0:
                slider.value = slider.minValue;
                break;

            case 1:
                slider.value = (slider.minValue + slider.maxValue) / 2;
                break;

            case 2:
                slider.value = slider.maxValue;
                break;
        }
    }

    public void OnNewCategory(int score)
    {
        if (score < 5)
            slider.minValue = score;
        else
            slider.minValue = 5;

        slider.value = slider.minValue;
        slider.maxValue = score;
        OnValueChanged();
        currentBidMeshInput.ActivateInputField();
    }
}
