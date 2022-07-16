using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SerialPortUtility;

public class SerialReceive : MonoBehaviour
{
    public SerialPortUtilityPro serialHandler;

    public MeterUnitController controller = null;

    public SerialPortUtilityPro.OpenSystem openSystem = SerialPortUtilityPro.OpenSystem.NumberOrder;
    public string VecderID;
    public string ProductID;
    public string SerialNumber;
    public string Port;

    private void Start()
    {
        LoadDeviceInfo();
        OpenDevice();
    } /* Start */

    public void OpenDevice()
    {
        if (serialHandler.IsConnected() == false && openSystem != SerialPortUtilityPro.OpenSystem.NumberOrder ) 
        {
            serialHandler.VendorID = VecderID;
            serialHandler.ProductID = ProductID;
            serialHandler.SerialNumber = SerialNumber;
            // serialHandler.Port = Port;
            serialHandler.Open();
        }
    } /* OpenDevice */

    //受信した信号(message)に対する処理
    public void OnDataReceived( object _data)
    {
        try
        {
            var message = _data as string;
            var data = message.Split(
                    new string[] { "\n" }, System.StringSplitOptions.None);
            controller.OnDataReceived(data[0]);

        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);//エラーを表示
        }
    } /* OnDataReceived */

    public void OnSerialEvent(SerialPortUtilityPro _serialPort,string _data)
    {
        Debug.Log("OnSerialEvent : " + _data);


    } /* OnSerialEvent */

    public void SaveDeviceInfo()
    {
        string _path = Application.persistentDataPath + "/" + "setting.xml";

        System.Xml.XmlTextWriter xmlWriter = new System.Xml.XmlTextWriter(_path, System.Text.Encoding.UTF8);
        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("setting");
        {
            xmlWriter.WriteAttributeString("OpenSystem", openSystem.ToString());
            xmlWriter.WriteAttributeString("VendorID", VecderID);
            xmlWriter.WriteAttributeString("ProductID", ProductID);
            xmlWriter.WriteAttributeString("SerialNumber", SerialNumber);
        }
        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
        xmlWriter.Close();

    } /* SaveDeviceInfo */

    public void LoadDeviceInfo()
    {
        string _path = Application.persistentDataPath + "/" + "setting.xml";
        System.Xml.XmlTextReader xmlReader = new System.Xml.XmlTextReader(_path);

        while (xmlReader.Read())
        {
            Debug.Log( xmlReader.Name );
            if (xmlReader.Name == "setting")
            {
                string OpenSystem = xmlReader.GetAttribute("OpenSystem");
                VecderID = xmlReader.GetAttribute("VendorID");
                ProductID = xmlReader.GetAttribute("ProductID");
                SerialNumber = xmlReader.GetAttribute("SerialNumber");
                if (OpenSystem == SerialPortUtilityPro.OpenSystem.USB.ToString())
                {
                    this.openSystem = SerialPortUtilityPro.OpenSystem.USB;
                }
                if (OpenSystem == SerialPortUtilityPro.OpenSystem.BluetoothSSP.ToString())
                {
                    this.openSystem = SerialPortUtilityPro.OpenSystem.BluetoothSSP;
                }
            }
        }


    } /* LoadDeviceInfo */


}