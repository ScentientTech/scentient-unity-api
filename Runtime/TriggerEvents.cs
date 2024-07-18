using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Scentient
{

    /// <summary>
    /// Simple helper class for triggering UnityEvent actions on Trigger enter and exit
    /// </summary>
    public class TriggerEvents : MonoBehaviour
    {
        [SerializeField] UnityEvent TriggerEnterEvent;
        [SerializeField] UnityEvent TriggerExitEvent;
        void OnTriggerExit(Collider collider)
        {
            TriggerExitEvent.Invoke();
        }

        void OnTriggerEnter(Collider other)
        {
            TriggerEnterEvent.Invoke();
        }
    }

}