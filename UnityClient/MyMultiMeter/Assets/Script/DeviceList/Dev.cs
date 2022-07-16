using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SerialPortUtility;

public class Dev : MonoBehaviour
{
    [HideInInspector]
    public SerialReceive serial;

    [HideInInspector]
    public Text m_text;

    [HideInInspector]
    public SerialPortUtilityPro.OpenSystem openSystem;

    [HideInInspector]
    public SerialPortUtilityPro.DeviceInfo info;
    
    public void OnSelected()
    {
        serial.openSystem = openSystem;
        serial.VecderID = info.Vendor;
        serial.ProductID = info.Product;
        serial.SerialNumber = info.SerialNumber;
        serial.Port = info.PortName;
        serial.SaveDeviceInfo();
    } /* OnSelected */
}
