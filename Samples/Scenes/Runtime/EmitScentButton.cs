using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scentient 
{
    public class EmitScentButton : MonoBehaviour
    {
        [SerializeField] byte channel;
        [SerializeField] ushort duration;
        [SerializeField] ScentientDevice _scentientDevice;

        public void Emit()
        {
            var scentChannelView = GetComponentInParent<ChannelScentView>();
            _scentientDevice.SendScentMessage(scentChannelView.channel, 255, duration);
        }
    }
}