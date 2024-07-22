using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scentient 
{
    public class EmitScentChannel : MonoBehaviour
    {
        [SerializeField] float m_duration = 0.25f;
        [SerializeField] float m_intensity = 1;
        [SerializeField] ScentientDevice _scentientDevice;

        public float Intensity {
            set {
                m_intensity = value;
            }
            get {
                return m_intensity;
            }
        }

        public float Duration {
            set {
                m_duration = value;
            }
            get {
                return m_duration;
            }
        }

        public void Emit()
        {
            var scentChannelView = GetComponentInParent<ChannelScentView>();
            ushort duration = (ushort)Mathf.RoundToInt(1000*Mathf.Clamp(m_duration,0f,60));
            byte intensity = (byte)Mathf.RoundToInt( 255*Mathf.Clamp01(m_intensity) );
            _scentientDevice.SendScentMessage(scentChannelView.channel, intensity, duration);
        }
    }
}