using System.Collections;
using System.Collections.Generic;
using Scentient;
using UnityEngine;
using UnityEngine.UI;

public class ChannelScentView : MonoBehaviour
{
    public bool Interactable
    {
        set {
            m_scentButton.interactable = value;
        }
    }

    public byte channel = 0;

    [SerializeField] Button m_scentButton;
    [SerializeField] Button m_changeScentButton;

    [SerializeField] ScentListView m_scentList;

    [SerializeField] ScentientDevice m_scentDevice;

    void Awake()
    {
        Interactable = false;
        m_scentDevice.OnRecievedScentNamesEvent += OnRecievedScentNames;
    }

    public void RefreshScentNames()
    {
        
    }

    public void OpenChangeScent()
    {
        ScentListView.channelToSet = channel;
    }

    void OnRecievedScentNames(int channelIndex, string name)
    {
        if(channelIndex!=channel){
            return;
        }
        m_scentDevice.GetChannelScentNames();
        var textField = m_scentButton.GetComponentInChildren<Text>();
        textField.text = name;
        m_scentButton.interactable = true;
    }
}
