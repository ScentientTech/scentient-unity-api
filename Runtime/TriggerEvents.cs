using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Scentient
{

    public class TriggerEvents : MonoBehaviour
    {
        [SerializeField] UnityEvent TriggerEnterEvent;
        [SerializeField] UnityEvent TriggerExitEvent;
        void OnTriggerExit()
        {
            TriggerExitEvent.Invoke();
        }

        void OnTriggerEnter()
        {
            TriggerEnterEvent.Invoke();
        }
    }

}