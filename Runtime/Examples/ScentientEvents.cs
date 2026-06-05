using System;
using Scentient;
using UnityEngine;
using UnityEngine.Events;

namespace Scentient.Samples
{
    public class ScentientEvents : MonoBehaviour
    {
        [SerializeField] ScentientDevice m_scentientDevice;
        [SerializeField] UnityEvent m_connectedEvent;
        [SerializeField] UnityEvent m_disconnectedEvent;

        void Start()
        {
            m_scentientDevice.OnStateChangedEvent += OnStateChanged;
            m_disconnectedEvent.Invoke();
        }

        private void OnStateChanged(ScentientDevice.States states)
        {
            switch( states)
            {
                case ScentientDevice.States.None:
                    m_disconnectedEvent.Invoke();
                break;
                case ScentientDevice.States.Ready:
                    m_connectedEvent.Invoke();
                break;
            }
        }
    }
}
