using System.Collections;
using System.Collections.Generic;
using Scentient;
using UnityEngine;

public class UIStates : MonoBehaviour
{
    [SerializeField] ScentientDevice m_scentientDevice;
    [SerializeField] GameObject m_connectPanel;
    [SerializeField] GameObject m_devicePanel;
    void Start()
    {
        m_scentientDevice.OnStateChangedEvent += OnStateChanged;
        m_connectPanel.SetActive( true );
        m_devicePanel.SetActive( false );
    }

    private void OnStateChanged(ScentientDevice.States states)
    {
        bool connected = states==ScentientDevice.States.Ready || states==ScentientDevice.States.Subscribe;
        m_connectPanel.SetActive( !connected );
        m_devicePanel.SetActive( connected );
    }

}
