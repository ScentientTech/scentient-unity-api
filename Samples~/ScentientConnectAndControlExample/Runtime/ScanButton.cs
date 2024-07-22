using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scentient;
using System;
using UnityEngine.UI;

[DefaultExecutionOrder(-50)]
public class ScanButton : MonoBehaviour
{
    [SerializeField] ScentientDevice _scentientDevice;
    [SerializeField] Button _button;
    
    void Start()
    {
        _scentientDevice.OnStateChangedEvent += OnStateChanged;
    }

    private void OnStateChanged(ScentientDevice.States state)
    {
        Debug.Log($"ScanButton.OnStateChanged : state={state}");
        _button.interactable = state==ScentientDevice.States.None;
    }
}
