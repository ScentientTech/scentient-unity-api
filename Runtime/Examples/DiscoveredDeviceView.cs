using System;
using System.Collections;
using System.Collections.Generic;
using Scentient;
using UnityEngine;
using UnityEngine.UI;

namespace Scentient.Samples
{
    public class DiscoveredDeviceView : MonoBehaviour
    {
        [SerializeField] Button m_connectButton;

        [SerializeField] Text m_nameLabel;
        [SerializeField] Text m_addrLabel;
        [SerializeField] ScentientDevice m_scentDevice;

        private string m_address;
        private string m_name;

        public void Init(string address,string name)
        {
            gameObject.SetActive(true);
            m_address = address;
            m_name = name;
            m_nameLabel.text = name;
            m_addrLabel.text = address;
        }

        void Start()
        {
            m_connectButton.onClick.AddListener(OnConnectButtonPressed);
            m_scentDevice.OnStateChangedEvent += OnDeviceStateChanged;
        }

        private void OnConnectButtonPressed()
        {
            m_scentDevice.Connect(m_address);
            m_connectButton.interactable = false;
        }

        private void OnDeviceStateChanged(ScentientDevice.States states)
        {
            m_connectButton.interactable = states!=ScentientDevice.States.Ready;
        }
    }
}
