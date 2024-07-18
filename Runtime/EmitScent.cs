using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scentient
{

    /// <summary>
    /// Simple class for emitting scents. Designed to be called by game scripts, and UnityEvent actions. 
    /// 
    /// </summary>
    public class EmitScent : MonoBehaviour
    {
        [Tooltip("see https://api.scentient.tech/scent-table_en.csv")] [SerializeField] string m_scentName;

        [Tooltip("Time in seconds")] [SerializeField] float m_duration = 0.25f;

        [Range(0,1)] [SerializeField] float m_intensity = 1f;

        /// <summary>
        /// The name of the scent, can include spaces, should be one of the names in the following table https://api.scentient.tech/scent-table_en.csv
        /// </summary>
        public string ScentName {
            set {
                m_scentName = value;
            }
            get {
                return m_scentName;
            }
        }

        /// <summary>
        /// Controls the intensity of the scent emitted by the device.
        /// valid range is 0 to 1
        /// </summary>
        public float Intensity {
            set {
                this.m_intensity = value;
            }
            get {
                return m_intensity;
            }
        }

        /// <summary>
        /// Controls duration that the scent is emitted by the device. 
        /// Calling Emit with duration 0 is equivilent to calling Stop.
        /// Value is in seconds, valid range is 0 to 60.
        /// </summary>
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