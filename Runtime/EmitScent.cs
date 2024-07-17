using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scentient
{

    public class EmitScent : MonoBehaviour
    {
        /// <summary>
        /// The name of the scent, can include spaces, should be one of the names in the following table https://api.scentient.tech/scent-table_en.csv
        /// </summary>
        [Tooltip("see https://api.scentient.tech/scent-table_en.csv")] [SerializeField] string m_scentName;
        /// <summary>
        /// Duration to emit for in seconds
        /// </summary>
        [Tooltip("Time in seconds")] [SerializeField] float m_duration = 0.25f;

        [Range(0,1)] [SerializeField] float m_intensity = 1f;

        public string ScentName {
            set {
                m_scentName = value;
            }
            get {
                return m_scentName;
            }
        }

        public float Intensity {
            set {
                this.m_intensity = value;
            }
            get {
                return m_intensity;
            }
        }

        public float Duration {
            set {
                this.m_intensity = value;
            }
            get {
                return m_intensity;
            }
        }

        [SerializeField] Scentient.ScentientDevice m_scentientDevice;

        public void Reset()
        {
            CheckDevice();
        }

        public void Emit()
        {
            if (CheckDevice())
            {
                m_scentientDevice.EmitScent(m_scentName, m_intensity, m_duration);
            }
        }

        public void Stop()
        {
            if (CheckDevice())
            {
                m_scentientDevice.EmitScent(m_scentName, 0);
            }
        }

        private bool CheckDevice()
        {
            if (!m_scentientDevice)
            {
                m_scentientDevice = GameObject.FindAnyObjectByType<ScentientDevice>();
                if (!m_scentientDevice)
                {
                    return false;
                }
            }
            return true;
        }
    }
}