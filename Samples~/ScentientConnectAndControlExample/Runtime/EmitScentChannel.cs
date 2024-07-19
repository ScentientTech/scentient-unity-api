using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scentient 
{
    public class EmitScentChannel : MonoBehaviour
    {
        [SerializeField] byte channel;
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
            ushort duration = (ushort)Mathf.RoundToInt(Mathf.Clamp(m_duration*1000f,0f,60f));
            byte intensity = (byte)Mathf.RoundToInt( Mathf.Clamp01(m_intensity) );
            _scentientDevice.SendScentMessage(scentChannelView.channel, intensity, duration);
        }
    }
}