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
        [SerializeField][Tooltip("see https://api.scentient.tech/scent-table_en.csv")] string m_scentName;
        /// <summary>
        /// Duration to emit for in seconds
        /// </summary>
        [SerializeField][Tooltip("Time in seconds")] float m_duration;

        [SerializeField] Scentient.ScentientDevice m_scentientDevice;

        public void Reset()
        {
            CheckDevice();
        }

        public void Emit()
        {
            if (CheckDevice())
            {
                m_scentientDevice.EmitScent(m_scentName, m_duration);
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