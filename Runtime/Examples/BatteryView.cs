using System;
using System.Globalization;
using Scentient;
using UnityEngine;
using UnityEngine.UI;

namespace Scentient.Samples
{
    public class BatteryView : MonoBehaviour
    {
        [SerializeField] Image m_batteryImage;
        [SerializeField] NumberLabelText m_batteryLevelText;

        [SerializeField] ScentientDevice m_scentientDevice;
        void Start()
        {
            m_scentientDevice.OnBatteryLevelChangedEvent += OnBatteryChanged;
        }

        void OnEnable()
        {
            OnBatteryChanged( m_scentientDevice.Battery );
        }

        private void OnBatteryChanged(byte level)
        {
            float p = level/100f;
            m_batteryLevelText.SetValue(p);
            m_batteryImage.fillAmount = p;
        }
    }
}
