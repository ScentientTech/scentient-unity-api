using System;
using System.Collections;
using System.Collections.Generic;
using Scentient;
using UnityEngine;
using UnityEngine.UI;

public class ScentientDeviceStatusView : MonoBehaviour
{

    [SerializeField] ScentientDevice m_scentDevice;
    [SerializeField] Text m_statusTextField;
    void Start()
    {
        m_scentDevice.OnStatusChangedEvent += OnStatusChanged;
    }

    private void OnStatusChanged(string text)
    {
        m_statusTextField.text = text;
    }
}
