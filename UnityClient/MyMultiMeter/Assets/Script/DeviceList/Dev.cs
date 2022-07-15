using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SerialPortUtility;

public class Dev : MonoBehaviour
{
    [HideInInspector]
    public Text m_text;

    [HideInInspector]
    public SerialPortUtilityPro.OpenSystem openSystem;

    [HideInInspector]
    public SerialPortUtilityPro.DeviceInfo info;
    
    public void OnSelected()
    {
        
    } /* OnSelected */
}
