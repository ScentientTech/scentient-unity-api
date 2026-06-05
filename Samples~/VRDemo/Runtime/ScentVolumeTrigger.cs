using System.Collections;
using System.Collections.Generic;
using Scentient;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ScentVolumeTrigger : MonoBehaviour
{
    [SerializeField] ScentientDevice m_scentientDevice;
    [SerializeField] [Tooltip("see https://api.scentient.tech/scent-table_en.csv")] string scentName;
    [SerializeField] [Tooltip("Seconds")] float duration;
    [Range(0,1)] [SerializeField] float intensity = 1f;
    [SerializeField] bool emitScentOnEnter=true;
    [SerializeField] bool stopScentOnExit=true;
 

    void OnTriggerEnter()
    {
        if(!emitScentOnEnter){
            return;
        }
        Debug.Log($"ScentVolumeTrigger.OnTriggerEnter {scentName} {duration}");
        m_scentientDevice.EmitScent(scentName,intensity,duration);
    }

    void OnTriggerExit()
    {
        if(!stopScentOnExit){
            return;
        }
        Debug.Log($"ScentVolumeTrigger.OnTriggerExit {scentName}");
        m_scentientDevice.EmitScent(scentName,0);
    }

}