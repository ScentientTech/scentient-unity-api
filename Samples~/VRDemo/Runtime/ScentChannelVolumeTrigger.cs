using System.Collections;
using System.Collections.Generic;
using Scentient;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ScentChannelVolumeTrigger : MonoBehaviour
{
    [SerializeField] ScentientDevice m_scentientDevice;
    [SerializeField] [Tooltip("see https://api.scentient.tech/scent-table_en.csv")] int scentChannel;
    [SerializeField] [Tooltip("Seconds")] float duration;
    [Range(0,1)] [SerializeField] float intensity = 1f;

    [SerializeField] bool emitScentOnEnter=true;
    [SerializeField] bool stopScentOnExit=true;
 

    void OnTriggerEnter()
    {
        if(!emitScentOnEnter){
            return;
        }
        Debug.Log($"ScentChannelVolumeTrigger.OnTriggerEnter {scentChannel} {intensity} {duration}");
        m_scentientDevice.SendScentMessage (scentChannel,intensity,duration);
    }

    void OnTriggerExit()
    {
        if(!stopScentOnExit){
            return;
        }
        Debug.Log($"ScentChannelVolumeTrigger.OnTriggerExit {scentChannel}");
        m_scentientDevice.SendScentMessage(scentChannel,1,0);
    }

}