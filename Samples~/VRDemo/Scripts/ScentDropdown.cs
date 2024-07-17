using System;
using System.Collections;
using System.Collections.Generic;
using Scentient;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class ScentDropdown : MonoBehaviour
{
    [SerializeField] Dropdown m_dropdown;
    [SerializeField] ScentientDevice m_scentientDevice;

    public UnityEvent<string> ScentSelectedEvent;
    void Reset()
    {
        m_dropdown = GetComponent<Dropdown>();
        m_scentientDevice = GameObject.FindAnyObjectByType<ScentientDevice>();    
    }

    void Start()
    {
        m_scentientDevice.OnChannelScentsUpdatedEvent += OnChannelScentsUpdated;
        m_dropdown.ClearOptions();
        m_dropdown.onValueChanged.AddListener(OnScentSelected);
    }

    private void OnScentSelected(int optionIndex)
    {
        int channelIndex = System.Array.IndexOf(m_scentientDevice.ChannelScentNames,m_dropdown.options[optionIndex]);
        ScentSelectedEvent.Invoke(m_dropdown.options[optionIndex].text);
    }

    private void OnChannelScentsUpdated(int arg1, string scentName)
    {
        m_dropdown.AddOptions(new List<Dropdown.OptionData>{
            new Dropdown.OptionData(scentName)
        });
        if(m_dropdown.options.Count==1){
            m_dropdown.value = 0;
            ScentSelectedEvent.Invoke(scentName);
        }
    }
}
