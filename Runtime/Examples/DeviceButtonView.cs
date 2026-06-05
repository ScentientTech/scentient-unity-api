using System;
using System.Collections;
using System.Collections.Generic;
using Scentient;
using UnityEngine;

namespace Scentient.Samples
{
    public class DeviceButtonView : MonoBehaviour
    {
        [SerializeField] ScentientDevice m_scentientDevice;
        [SerializeField] GameObject m_buttonPressedIndicator;
        [SerializeField] float m_displayTime = 1f;

        void Start()
        {
            m_scentientDevice.OnButtonPressedEvent += OnButtonPressed;
        }

        private void OnButtonPressed()
        {
            StopAllCoroutines();
            StartCoroutine(ButtonPressedRoutine());
        }

        private IEnumerator ButtonPressedRoutine()
        {
            m_buttonPressedIndicator.SetActive(true);
            yield return new WaitForSeconds(m_displayTime);
            m_buttonPressedIndicator.SetActive(false);
        }


    }
}
