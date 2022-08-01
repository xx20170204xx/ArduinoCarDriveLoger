using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SerialPortUtility;

/*
 
    GPS:
        Androidの場合、アプリ情報から位置情報を取得することを許可する必要がある
*/
public class SerialReceive : MonoBehaviour
{
    public static SerialReceive Instance { get; set; }

    public SerialPortUtilityPro serialHandler;

    public MeterUnitController controller = null;

    public SerialPortUtilityPro.OpenSystem openSystem = SerialPortUtilityPro.OpenSystem.NumberOrder;
    public string VecderID;
    public string ProductID;
    public string SerialNumber;
    public string Port;

    private bool isRecordData = false;
    private string RecordDataFilename = "";

    /* GPS情報 */
    public float latitude;  /* 経度 */
    public float longitude; /* 経度 */
    public float altitude;  /* 高度 */

    private void Awake()
    {
        Instance = this;
        StartCoroutine(StartLocationService());
    } /* Awake */

    private void OnDestroy()
    {
        Instance = null;
    } /* OnDestroy */

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
            if (isRecordData == true)
            {
                SaveRecordData(data[0]);
            }

        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message.ToString());//エラーを表示
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

    public void SwitchRecordData()
    {
        System.DateTime dateTime = System.DateTime.Now;
        if (isRecordData == false)
        {
            isRecordData = true;
            RecordDataFilename = Application.persistentDataPath + "/data_" + dateTime.ToString("yyyyMMddHHmmss") + ".csv";
        } else {
            isRecordData = false;
        }
    } /* SwitchRecordData */

    private void SaveRecordData( string _data )
    {
        StreamWriter csvWriter = null;
        System.DateTime dateTime = System.DateTime.Now;
        if (string.IsNullOrEmpty(RecordDataFilename) == true)
        {
            return;
        }
        csvWriter = new StreamWriter(RecordDataFilename,true);

        string data = _data.Replace("\t", ",");
        data = data.Replace("\r", "");
        data = data.Replace("\n", "");

        data += "," + latitude;   /* 緯度 */
        data += "," + longitude;  /* 経度 */
        data += "," + altitude;   /* 高度*/

        csvWriter.WriteLine( dateTime.ToString("yyyy/MM/dd HH:mm:ss.fff") + "," + data);
        csvWriter.Close();

    } /* SaveRecordData */

    private IEnumerator StartLocationService()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("GPS not enabled");
            yield break;
        }

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait <= 0)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }

        // Set locational infomations
        while (Instance != null)
        {
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
            altitude = Input.location.lastData.altitude;
            yield return new WaitForSeconds(10);
        }
    } /* StartLocationService */

}