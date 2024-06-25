using System;
using System.Collections;
using System.Collections.Generic;
using Scentient;
using UnityEngine;
using UnityEngine.UI;

public class ScentListView : MonoBehaviour
{
    [SerializeField] ScentientDevice m_scentDevice;
    [SerializeField] Button m_scentNameButtonTemplate;

    public static int channelToSet;

    void Awake()
    {        
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void PopulateScentNamesList()
    {
        Dictionary<short,string> values = m_scentDevice.GetScentDict();
        foreach(var (id,name) in values){
            var element = Instantiate(m_scentNameButtonTemplate,m_scentNameButtonTemplate.transform.parent);
            var textField = element.GetComponentInChildren<Text>();
            textField.text = name;
            element.onClick.AddListener(()=>OnScentButtonClicked(id));
            element.gameObject.SetActive(true);
        }
    }

    private void OnScentButtonClicked(short scentId)
    {
        m_scentDevice.SetChannelScent(channelToSet,scentId);
        Hide();
        
    }
}
