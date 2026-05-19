using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scentient;
using System;
using UnityEngine.UI;

[DefaultExecutionOrder(-50)]
public class ForgetButton : MonoBehaviour
{
    [SerializeField] Button _button;
    void OnEnable()
    {
        _button.interactable = ScentientDevice.HasSavedDevice;
    }
    void Start()
    {
        _button.onClick.AddListener( OnButtonPressed );
    }

    private void OnButtonPressed()
    {
        ScentientDevice.Forget();
        _button.interactable = ScentientDevice.HasSavedDevice;
    }

}
