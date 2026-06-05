using System;
using System.Collections;
using System.Collections.Generic;
using Scentient;
using UnityEngine;
using UnityEngine.UI;

namespace Scentient.Samples
{
    public class ReconnectButton : MonoBehaviour
    {
        [SerializeField] ScentientDevice m_scentientDevice;
        [SerializeField] Button m_button;
        [SerializeField] Button m_forgetButton;
        void OnEnable()
        {
            m_button.interactable = m_scentientDevice.State==ScentientDevice.States.None && ScentientDevice.HasSavedDevice;
        }


        void Start()
        {
            m_button.onClick.AddListener( OnButtonClicked );
            m_scentientDevice.OnStateChangedEvent += OnStateChanged;
        }

        private void OnButtonClicked()
        {
            m_scentientDevice.Reconnect();
        }

        private void OnStateChanged(ScentientDevice.States state)
        {
            Debug.Log($"ScanButton.OnStateChanged : state={state}");
        }

    }
}
