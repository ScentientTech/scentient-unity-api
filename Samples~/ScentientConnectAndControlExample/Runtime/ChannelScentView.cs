using Scentient;
using UnityEngine;
using UnityEngine.UI;

public class ChannelScentView : MonoBehaviour
{
    public bool Interactable
    {
        set {
            m_scentButton.interactable = value;
        }
    }

    public byte channel = 0;

    [SerializeField] Button m_scentButton;
    [SerializeField] Button m_changeScentButton;
    [SerializeField] Image m_scentLevelImage;
    [SerializeField] Text m_fillLevelText;
    [SerializeField] Text m_scentText;
    [SerializeField] ScentListView m_scentList;
    [SerializeField] ScentientDevice m_scentDevice;

    void Awake()
    {
        Interactable = false;
        m_scentDevice.OnChannelScentsUpdatedEvent += OnChannelScentsUpdated;
        m_scentDevice.OnChannelScentLevelChangedEvent += OnChannelScentLevelChanged;
        
    }



    public void RefreshScentNames()
    {
        
    }

    public void OpenChangeScent()
    {
        ScentListView.channelToSet = channel;
        m_scentList.Show();        
    }

    void OnChannelScentsUpdated(int scentChannel, int scentId, string name)
    {
        if(scentChannel!=channel){
            return;
        }
        
        m_scentText.text = name;
        m_scentButton.interactable = scentId!=ScentTable.Nothing;
        m_changeScentButton.interactable = true;
    }

    private void OnChannelScentLevelChanged(int scentChannel, int scentLevel)
    {
        if (scentChannel != channel)
        {
            return;
        }
        if (m_fillLevelText == null)
        {
            m_fillLevelText = m_scentLevelImage.GetComponentInChildren<Text>();
        }
        m_fillLevelText.text = $"{scentLevel/100f:P0}";
        m_scentLevelImage.fillAmount = scentLevel/100f;
    }
    
}
