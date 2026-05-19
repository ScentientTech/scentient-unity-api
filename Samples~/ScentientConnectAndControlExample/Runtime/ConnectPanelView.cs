using System;
using System.Collections;
using System.Collections.Generic;
using Scentient;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConnectPanelView : MonoBehaviour
{
    [SerializeField] DiscoveredDeviceView m_deviceViewTemplate;
    [SerializeField] GameObject m_scentientDevicePanel;
    [SerializeField] ScentientDevice m_scentientDevice;

    [SerializeField] Button m_scanButton;
    [SerializeField] Button m_reconnectButton;
    [SerializeField] Button m_forgetButton;

    void OnEnable()
    {
        m_reconnectButton.interactable = StateIsNotReady(m_scentientDevice.State) && ScentientDevice.HasSavedDevice;
        m_forgetButton.interactable = ScentientDevice.HasSavedDevice;
    }

    void Start()
    {
        m_scentientDevice.OnDeviceDiscoveredEvent += OnDeviceDiscovered;
        m_scentientDevice.OnConnectedEvent.AddListener( OnDeviceConnected );
        m_scentientDevice.OnStateChangedEvent += OnStateChanged;

        m_deviceViewTemplate.gameObject.SetActive(false);

        m_scentientDevice.OnStateChangedEvent += OnStateChanged;
        m_scanButton.onClick.AddListener( OnScanButtonPressed );
        m_reconnectButton.onClick.AddListener( OnReconnectButtonClicked );
        m_forgetButton.onClick.AddListener( OnForgetButtonClicked );
    }

    private void OnReconnectButtonClicked()
    {
        m_scentientDevice.Reconnect();
        m_forgetButton.interactable = ScentientDevice.HasSavedDevice;
        m_reconnectButton.interactable = StateIsNotReady(m_scentientDevice.State) && ScentientDevice.HasSavedDevice;
    }

    private void OnForgetButtonClicked()
    {
        ScentientDevice.Forget();
        m_forgetButton.interactable = ScentientDevice.HasSavedDevice;
        m_reconnectButton.interactable = StateIsNotReady(m_scentientDevice.State) && ScentientDevice.HasSavedDevice;
    }

    private void OnScanButtonPressed()
    {
        m_scentientDevice.Scan();
    }

    private void OnStateChanged(ScentientDevice.States state)
    {
        m_reconnectButton.interactable = ScentientDevice.HasSavedDevice && StateIsNotReady(state);
        m_scanButton.interactable = state==ScentientDevice.States.None;

        // This was written to prevent duplicate entires, from multiple scans. However a device only 
        //if(state == ScentientDevice.States.Scan)
        //{
            //var previousDiscoveredDeviceViews = gameObject.GetComponentsInChildren<DiscoveredDeviceView>();
            // foreach(var deviceView in previousDiscoveredDeviceViews)
            // {
            //     Destroy(deviceView.gameObject);
            // }
        //}
    }

    private bool StateIsNotReady(ScentientDevice.States state)
    {
        return state!= ScentientDevice.States.Ready;
    }

    private void OnDeviceConnected()
    {
        
    }

    private void OnDeviceDiscovered(string addr, string name)
    {
        var discoveredDevice = Instantiate<DiscoveredDeviceView>( m_deviceViewTemplate,m_deviceViewTemplate.transform.parent);
        discoveredDevice.Init(addr,name);
    }
}
