using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SerialPortUtility;

public class DevListPanel : MonoBehaviour
{
    [SerializeField]
    private SerialReceive serial;
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private GameObject contents;
    [SerializeField]
    private Dropdown deviceType;


    public void OnUpdateDeviceList()
    {
        int devType = deviceType.value;
        string devTypeName = deviceType.options[devType].text;
        SerialPortUtilityPro.OpenSystem openSystem = SerialPortUtilityPro.OpenSystem.USB;
        if (devTypeName == "USB")
        {
            openSystem = SerialPortUtilityPro.OpenSystem.USB;
        }
        if (devTypeName == "Bluetooth")
        {
            openSystem = SerialPortUtilityPro.OpenSystem.BluetoothSSP;
        }

        /* ˆê——‚ÌƒNƒŠƒA */
        int count = contents.transform.childCount;
        for (int ii = 0; ii < count; ii++)
        {
            var chld = contents.transform.GetChild(0);
            chld.gameObject.SetActive(false);
            chld.gameObject.transform.SetParent(null);
            Destroy(chld.gameObject);
        }

        var _list = SerialPortUtilityPro.GetConnectedDeviceList(openSystem);
        foreach (var devInfo in _list)
        {
            GameObject _go = Instantiate(prefab);
            _go.transform.parent = this.contents.transform;
            _go.transform.localScale = this.contents.transform.localScale;
            var dev = _go.GetComponent<Dev>();

            dev.serial = this.serial;
            dev.openSystem = openSystem;
            dev.info = devInfo;
            //if( openSystem ==SerialPortUtilityPro.OpenSystem.USB )

            string strInfoText = "";
            switch (openSystem)
            {
                case SerialPortUtilityPro.OpenSystem.USB:
                    {
                        strInfoText = "PortName=[" + devInfo.PortName + "] Vendor=[" + devInfo.Vendor + "] Product=[" + devInfo.Product + "]";
                    }
                    break;

                case SerialPortUtilityPro.OpenSystem.BluetoothSSP:
                    {
                        strInfoText = "SerialNumber=[" + devInfo.SerialNumber + "]";
                    }
                    break;
            }

            dev.m_text.text = strInfoText;
        }

    } /* OnUpdateDeviceList */
}
