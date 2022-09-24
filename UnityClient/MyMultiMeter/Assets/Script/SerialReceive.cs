using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SerialPortUtility;

/*
 
    GPS:
        Androidの場合、アプリ情報から位置情報を取得することを許可する必要がある
*/
public class SerialReceive : MonoBehaviour
{
    public struct MeterSetting
    {
        public float m_lowValue;
        public float m_highValue;
        public float m_blinkValue;
        public Color m_lowColor;
        public Color m_normalColor;
        public Color m_highColor;
    }
    public Dictionary<MeterBase.MeterType, MeterSetting> m_MeterSetting = new Dictionary<MeterBase.MeterType, MeterSetting>();

    public static SerialReceive Instance { get; set; }

    public SerialPortUtilityPro serialHandler;

    public MeterUnitController controller = null;

    [HideInInspector]
    public SerialPortUtilityPro.OpenSystem openSystem = SerialPortUtilityPro.OpenSystem.NumberOrder;
    [HideInInspector]
    public string VecderID;
    [HideInInspector]
    public string ProductID;
    [HideInInspector]
    public string SerialNumber;
    [HideInInspector]
    public string Port;

    private bool isRecordData = false;
    private string RecordDataFilename = "";

    /* GPS情報 */
    [HideInInspector]
    public float latitude;  /* 経度 */
    [HideInInspector]
    public float longitude; /* 経度 */
    [HideInInspector]
    public float altitude;  /* 高度 */

    [SerializeField]
    private Image recoedImage = null;
    [SerializeField]
    private Sprite recoedSprite = null;
    [SerializeField]
    private Sprite stopSprite = null;

    [SerializeField]
    private string m_GoogleAPIKEY;
    public string GoogleAPIKey { get{ return m_GoogleAPIKEY; } }

    public Text m_debugText = null;

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
            serialHandler.OpenMethod = openSystem;
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
            AddDebugText("Exception :" + e.Message.ToString());
        }
    } /* OnDataReceived */

    public void OnSerialEvent(SerialPortUtilityPro _serialPort,string _data)
    {
        Debug.Log("OnSerialEvent : " + _data);
        AddDebugText("OnSerialEvent :" + _data);
        if (_data == "OPEN_ERROR")
        {
            string message = "OnSerialEvent :" + _data;
            AddDebugText(message);
            message = "  " + serialHandler.OpenMethod;   AddDebugText(message);
            message = "  " + serialHandler.VendorID; AddDebugText(message);
            message = "  " + serialHandler.ProductID; AddDebugText(message);
            message = "  " + serialHandler.SerialNumber; AddDebugText(message);
        }

    } /* OnSerialEvent */

    public void SaveDeviceInfo()
    {
        string _path = Application.persistentDataPath + "/" + "setting.xml";

        System.Xml.XmlTextWriter xmlWriter = new System.Xml.XmlTextWriter(_path, System.Text.Encoding.UTF8);
        xmlWriter.WriteStartDocument();
        xmlWriter.WriteWhitespace("\r\n");
        xmlWriter.WriteStartElement("AllSetting");
        xmlWriter.WriteWhitespace("\r\n");
        xmlWriter.WriteStartElement("setting");
        {
            xmlWriter.WriteAttributeString("OpenSystem", openSystem.ToString());
            xmlWriter.WriteAttributeString("VendorID", VecderID);
            xmlWriter.WriteAttributeString("ProductID", ProductID);
            xmlWriter.WriteAttributeString("SerialNumber", SerialNumber);
        }
        xmlWriter.WriteEndElement();
        xmlWriter.WriteWhitespace("\r\n");
        xmlWriter.WriteWhitespace("\r\n");
        foreach (var key in m_MeterSetting.Keys)
        {
            var values = m_MeterSetting[key];
            xmlWriter.WriteComment("Meter=[" + key.ToString() + "]");
            xmlWriter.WriteWhitespace("\r\n");
            xmlWriter.WriteStartElement("meter");
            {
                xmlWriter.WriteAttributeString("type", key.ToString("d"));
                xmlWriter.WriteAttributeString("lowValue", values.m_lowValue.ToString());
                xmlWriter.WriteAttributeString("highValue", values.m_highValue.ToString());
                xmlWriter.WriteAttributeString("blinkValue", values.m_blinkValue.ToString());
                xmlWriter.WriteAttributeString("lowColor", "#"+ColorUtility.ToHtmlStringRGBA(values.m_lowColor));
                xmlWriter.WriteAttributeString("normalColor", "#" + ColorUtility.ToHtmlStringRGBA(values.m_normalColor));
                xmlWriter.WriteAttributeString("highColor", "#" + ColorUtility.ToHtmlStringRGBA(values.m_highColor));
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\r\n");
        }
        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
        xmlWriter.Close();

    } /* SaveDeviceInfo */

    public void LoadDeviceInfo()
    {
        string _path = Application.persistentDataPath + "/" + "setting.xml";
        System.Xml.XmlTextReader xmlReader = new System.Xml.XmlTextReader(_path);

        try
        {
            while (xmlReader.Read())
            {
                Debug.Log(xmlReader.Name);
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
                if (xmlReader.Name == "meter")
                {
                    string meter_type = xmlReader.GetAttribute("type");
                    string lowValue = xmlReader.GetAttribute("lowValue");
                    string highValue = xmlReader.GetAttribute("highValue");
                    string blinkValue = xmlReader.GetAttribute("blinkValue");
                    string lowColorStr = xmlReader.GetAttribute("lowColor");
                    string normalColorStr = xmlReader.GetAttribute("normalColor");
                    string highColorStr = xmlReader.GetAttribute("highColor");

                    Debug.Log("Type=" + meter_type.ToString() +
                        " lowValue=" + lowValue +
                        " highValue=" + highValue +
                        " blinkValue=" + blinkValue +
                        " lowColor=" + lowColorStr +
                        " normalColor=" + normalColorStr +
                        " highColor=" + highColorStr
                        );
                    MeterBase.MeterType _key = (MeterBase.MeterType)int.Parse(meter_type);
                    MeterSetting mSetting;
                    if (this.m_MeterSetting.ContainsKey(_key))
                    {
                        mSetting = m_MeterSetting[_key];
                    }else {
                        mSetting = new MeterSetting();
                        m_MeterSetting.Add(_key, mSetting);
                    }
                    Color lowColor;
                    Color normalColor;
                    Color highColor;
                    ColorUtility.TryParseHtmlString(lowColorStr,out lowColor);
                    ColorUtility.TryParseHtmlString(normalColorStr, out normalColor);
                    ColorUtility.TryParseHtmlString(highColorStr, out highColor);
                    mSetting.m_lowValue = int.Parse(lowValue);
                    mSetting.m_highValue = int.Parse(highValue);
                    mSetting.m_blinkValue = int.Parse(blinkValue);
                    mSetting.m_lowColor = lowColor;
                    mSetting.m_normalColor = normalColor;
                    mSetting.m_highColor = highColor;
                    m_MeterSetting[_key] = mSetting;
                }
            }
        }
        catch (System.Exception _e)
        {
            Debug.LogWarning("Exception=[" + _e.Message + "]");
            xmlReader.Close();
        }
        finally {
            xmlReader.Close();
        }
        xmlReader = null;

    } /* LoadDeviceInfo */

    public void SwitchRecordData()
    {
        System.DateTime dateTime = System.DateTime.Now;
        if (isRecordData == false)
        {
            isRecordData = true;
            RecordDataFilename = Application.persistentDataPath + "/data_" + dateTime.ToString("yyyyMMddHHmmss") + ".csv";
            if (recoedImage != null && stopSprite != null)
            {
                recoedImage.sprite = stopSprite;
            }
        } else {
            isRecordData = false;
            if (recoedImage != null && recoedSprite != null)
            {
                recoedImage.sprite = recoedSprite;
            }
        }
        Debug.Log("isRecordData : " + isRecordData.ToString());
        AddDebugText("isRecordData : " + isRecordData.ToString());
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

    public void AddDebugText(string message)
    {
        if (m_debugText != null)
        {
            m_debugText.text += "\r\n" + message;
        }

    } /* AddDebugText */

} /* class */