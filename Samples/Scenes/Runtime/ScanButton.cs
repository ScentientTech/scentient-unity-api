using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scentient;
using System;
using UnityEngine.UI;

public class ScanButton : MonoBehaviour
{
    [SerializeField] ScentientDevice _scentientDevice;
    [SerializeField] Button _button;
    
    void Start(){
        _scentientDevice.OnStateChangedEvent += OnStateChanged;
    }

    private void OnStateChanged(ScentientDevice.States state)
    {
        Debug.Log($"OnStateChanged: state={state}");
        if(state==ScentientDevice.States.None){
            _button.interactable = true;
        }
    }
}
