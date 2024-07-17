using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class NumberLabelText : MonoBehaviour
{
    [SerializeField] Text m_textField;
    [SerializeField] string m_formatString = "{0:0.0}";

    void Reset()
    {
        m_textField = GetComponent<Text>();
    }

    public void SetValue(float value)
    {
        m_textField.text = string.Format(m_formatString,value);
    }
}
