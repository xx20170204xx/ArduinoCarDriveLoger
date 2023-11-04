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
[RequireComponent(typeof(AudioSource))]
public class SerialReceive : MonoBehaviour
{
    public struct MeterSetting
    {
        public float m_lowValue;
        public float m_highValue;
        public float m_blinkValue;
        public Color m_lowColor;
        public Color m_highColor;
        public bool m_enable;
    }
    public Dictionary<MeterBase.MeterType, MeterSetting> m_MeterSetting = null;

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

    public Text m_debugText = null;

    private SoundController m_infoSound;

    private System.DateTime m_lastDate;

    struct DataValue
    {
        public float m_waterTemp;
        public float m_oilTemp;
        public float m_oilPress;
        public float m_boostPress;
        public float m_tacho;
        public float m_speed;
    }
    private DataValue m_data;

    public float WaterTemp { get { return m_data.m_waterTemp; } }
    public float OilTemp { get { return m_data.m_oilTemp; } }
    public float OilPress { get { return m_data.m_oilPress; } }
    public float BoostPress { get { return m_data.m_boostPress; } }
    public float Tacho { get { return m_data.m_tacho; } }
    public float Speed { get { return m_data.m_speed; } }

    private void Awake()
    {
        m_infoSound = GetComponent<SoundController>();
        m_lastDate = System.DateTime.Now;
        Instance = this;

        if (m_MeterSetting == null ) 
        {
            m_MeterSetting = new Dictionary<MeterBase.MeterType, MeterSetting>();
            m_MeterSetting.Add(MeterBase.MeterType.TYPE_TACHO, new MeterSetting());
            m_MeterSetting.Add(MeterBase.MeterType.TYPE_SPEED, new MeterSetting());
            m_MeterSetting.Add(MeterBase.MeterType.TYPE_GEAR_RATIO, new MeterSetting());

            m_MeterSetting.Add(MeterBase.MeterType.TYPE_WATER_TEMP, new MeterSetting());
            m_MeterSetting.Add(MeterBase.MeterType.TYPE_OIL_TEMP, new MeterSetting());
            m_MeterSetting.Add(MeterBase.MeterType.TYPE_OIL_PRESS, new MeterSetting());
            m_MeterSetting.Add(MeterBase.MeterType.TYPE_BOOST_PRESS, new MeterSetting());

            m_MeterSetting.Add(MeterBase.MeterType.TYPE_SPEED_FIX, new MeterSetting());

        }
        StartCoroutine(StartLocationService());
    } /* Awake */

    private void OnDestroy()
    {
        Instance = null;
    } /* OnDestroy */

    private void Start()
    {
        LoadLastDate();
        SaveLastDate();
        LoadDeviceInfo();
        OpenDevice();
        StartCoroutine(StartOpeningSE());
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

            string[] values = data[0].Split('\t');

            m_data.m_waterTemp = float.Parse(values[1]);
            m_data.m_oilTemp = float.Parse(values[2]);
            m_data.m_oilPress = float.Parse(values[3]);
            m_data.m_boostPress = float.Parse(values[4]);
            m_data.m_tacho = float.Parse(values[5]);
            m_data.m_speed = float.Parse(values[6]);

            /* TODO:速度を1割増しで設定 */
            if (m_MeterSetting[MeterBase.MeterType.TYPE_SPEED_FIX].m_enable == true)
            {
                m_data.m_speed *= 1.1f;
            }

            if (controller != null)
            {
                controller.UpdateData();
            }

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
            // message = "  " + serialHandler.OpenMethod;   AddDebugText(message);
            // message = "  " + serialHandler.VendorID; AddDebugText(message);
            // message = "  " + serialHandler.ProductID; AddDebugText(message);
            // message = "  " + serialHandler.SerialNumber; AddDebugText(message);
            serialHandler.Close();
        }
        if (_data == "DISCONNECT_ERROR")
        {
            /* SEを出力する */
            m_infoSound.AddPlaySound(m_infoSound.m_conLostClip);
            /* 再接続を実施(コルーチン) */
            StartCoroutine(ReConnectDevice());
        }

    } /* OnSerialEvent */

    /*
        再接続時のコルーチン
    */
    IEnumerator ReConnectDevice() 
    {
        string msg;
        bool l_flag = true;
        int l_Count = 5;

        AddDebugText("ReConnectDevice Start");

        if (serialHandler.IsConnected() == true)
        {
            AddDebugText("ReConnectDevice End(IsConnected==true)");
            yield return null;
        }

        while (l_flag == true) 
        {
            /* 1秒待つ */
            yield return new WaitForSeconds(1.0f);

            AddDebugText("this.OpenDevice();");
            /* 再接続を実施 */
            this.OpenDevice();

            /* 1秒待つ */
            yield return new WaitForSeconds(1.0f);

            msg = "IsOpn:" + serialHandler.IsOpened().ToString();
            msg += "IsCon:"+serialHandler.IsConnected().ToString();
            AddDebugText(msg);

            if (serialHandler.IsOpened() == true)
            {
                /* 再接続成功 */
                m_infoSound.AddPlaySound(m_infoSound.m_InfoDevConSuccess);
                l_flag = false;
            }
            else
            {
                /* 再接続失敗 */
                l_Count -= 1;
                if (l_Count == 0)
                {
                    break;
                }
            }
        }

        if (l_flag == true)
        {
            m_infoSound.AddPlaySound(m_infoSound.m_InfoDevConError);
        }

        AddDebugText("ReConnectDevice End");
    } /* ReConnectDevice */

    public void SaveLastDate()
    {
        string _path = Application.persistentDataPath + "/" + "lastdate.xml";

        System.Xml.XmlTextWriter xmlWriter = new System.Xml.XmlTextWriter(_path, System.Text.Encoding.UTF8);
        xmlWriter.WriteStartDocument();
        xmlWriter.WriteWhitespace("\r\n");
        xmlWriter.WriteStartElement("LastUpdate");
        xmlWriter.WriteStartElement("lastDate");
        {
            System.DateTime _now = System.DateTime.Now;
            xmlWriter.WriteAttributeString("lastDate", _now.ToString("yyyy/MM/dd"));
        }
        xmlWriter.WriteEndElement();
        xmlWriter.WriteWhitespace("\r\n");
        xmlWriter.WriteEndDocument();
        xmlWriter.Close();

    } /* SaveLastDate */

    public void LoadLastDate()
    {
        string _path = Application.persistentDataPath + "/" + "lastdate.xml";
        System.Xml.XmlTextReader xmlReader = new System.Xml.XmlTextReader(_path);
        try
        {
            while (xmlReader.Read())
            {
                Debug.Log(xmlReader.Name);
                if (xmlReader.Name == "lastDate")
                {
                    string lastDate = xmlReader.GetAttribute("lastDate");
                    System.DateTime.TryParseExact(lastDate, "yyyy/MM/dd", null, System.Globalization.DateTimeStyles.None, out m_lastDate);
                }
            }

        }
        catch (System.Exception _e)
        {
            Debug.LogWarning("Exception=[" + _e.Message + "]");
            xmlReader.Close();
        }
        finally
        {
            xmlReader.Close();
        }

    } /* LoadLastDate */

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
                xmlWriter.WriteAttributeString("highColor", "#" + ColorUtility.ToHtmlStringRGBA(values.m_highColor));
                xmlWriter.WriteAttributeString("enable", values.m_enable.ToString());
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteWhitespace("\r\n");
        }
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
                    string highColorStr = xmlReader.GetAttribute("highColor");
                    string enableStr = xmlReader.GetAttribute("enable");

                    Debug.Log("Type=" + meter_type.ToString() +
                        " lowValue=" + lowValue +
                        " highValue=" + highValue +
                        " blinkValue=" + blinkValue +
                        " lowColor=" + lowColorStr +
                        " highColor=" + highColorStr +
                        " enable=" + enableStr
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
                    Color highColor;
                    ColorUtility.TryParseHtmlString(lowColorStr,out lowColor);
                    ColorUtility.TryParseHtmlString(highColorStr, out highColor);
                    mSetting.m_lowValue = int.Parse(lowValue);
                    mSetting.m_highValue = int.Parse(highValue);
                    mSetting.m_blinkValue = int.Parse(blinkValue);
                    mSetting.m_lowColor = lowColor;
                    mSetting.m_highColor = highColor;
                    mSetting.m_enable = bool.Parse(enableStr);
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

    /*
     * データの記録状態を切り替える
    */
    public void SwitchRecordData()
    {
        System.DateTime dateTime = System.DateTime.Now;
        /* データ記録状態の確認 */
        if (isRecordData == false)
        {
            /* データ記録をしていない場合、 */
            isRecordData = true;
            /* ファイル名を決める */
            RecordDataFilename = Application.persistentDataPath + "/data_" + dateTime.ToString("yyyyMMddHHmmss") + ".csv";
            
            /* 表示しているスプライトを切り替える */
            if (recoedImage != null && stopSprite != null)
            {
                recoedImage.sprite = stopSprite;
            }
            /* SEを出力する */
            m_infoSound.AddPlaySound(m_infoSound.m_recStartClip);
        } else {
            /* データ記録をしている場合、 */
            isRecordData = false;

            /* 表示しているスプライトを切り替える */
            if (recoedImage != null && recoedSprite != null)
            {
                recoedImage.sprite = recoedSprite;
            }
            /* SEを出力する */
            m_infoSound.AddPlaySound(m_infoSound.m_recStopClip);
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

    private IEnumerator StartOpeningSE()
    {
        AudioClip _clip = null;
        System.DateTime _now = System.DateTime.Now;

        _clip = m_infoSound.GetOpeningSE();

        m_infoSound.AddPlaySound(_clip);

        yield return new WaitForSeconds(0);

        if (m_lastDate.Date == _now.Date)
        {
            int num = Random.Range(0, m_infoSound.m_opOnePoint.Count);
            m_infoSound.AddPlaySound(m_infoSound.m_opOnePoint[num]);
        }
        else
        {
            m_infoSound.AddPlaySound(m_infoSound.m_opDayOnce);
        }

        // return null;

    } /* StartOpeningSE */



    public void AddDebugText(string message)
    {
        if (m_debugText != null)
        {
            m_debugText.text += "\r\n" + message;
        }

    } /* AddDebugText */

    private void OnApplicationPause(bool _pause)
    {
        
    } /* OnApplicationPause */

} /* class */