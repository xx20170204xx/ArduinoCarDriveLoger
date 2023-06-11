using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SerialServiceAndroid : MonoBehaviour
{
    public const string CCONTEXT_UNITYPLAYER = "com.unity3d.player.UnityPlayer";
    public const string CCLASS_ID = "jp.ne.sakura.jacobi.myserialservicelib.myserialservicelib";

    [SerializeField]
    private Text m_IsOpenText = null;
    [SerializeField]
    private Text m_WaterTmpText = null;
    [SerializeField]
    private Text m_OilTmpText = null;
    [SerializeField]
    private Text m_OilPressText = null;
    [SerializeField]
    private Text m_BoostPressText = null;
    [SerializeField]
    private Text m_RoomTempText = null;

    [SerializeField]
    private Text m_DebugText = null;

    [SerializeField]
    private Text m_DebugText2 = null;

    private void OnDestroy()
    {
        OnStopService();
    } /* OnDestroy */


    private void Update()
    {
        if (m_IsOpenText != null)
        {
            m_IsOpenText.text = (GetIsDeviceOpen() ? "O" : "C");
        }
        if (m_WaterTmpText != null)
        {
            m_WaterTmpText.text = "W:" + GetWaterTmp();
        }
        if (m_OilTmpText != null)
        {
            m_OilTmpText.text = "O:" + GetOilTmp();
        }
        if (m_OilPressText != null)
        {
            m_OilPressText.text = "o:" + GetOilPress();
        }
        if (m_BoostPressText != null)
        {
            m_BoostPressText.text = "B:" + GetBoostPress();
        }
        if (m_RoomTempText != null)
        {
            m_RoomTempText.text = "R:" + GetRoomTemp() + " " +
               "X:" + GetAngleX() + " " +
               "Y:" + GetAngleY() + " " +
               "Z:" + GetAngleZ()
                ;
        }/*
        if (m_DebugText != null)
        {
            m_DebugText.text = GetDataLine();
        }*/
    } /* Update */

    private AndroidJavaObject GetAndroidContext()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass cls = new AndroidJavaClass(CCONTEXT_UNITYPLAYER);
        if (cls != null) 
        {
            var _curAct = cls.GetStatic<AndroidJavaObject>("currentActivity");
            using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
            {
                return androidJavaClass.CallStatic<AndroidJavaObject>("getApplicationContext", _curAct);
            }
        }
#else
        /* nop */
#endif
        return null;
    }

    public void OnStartService()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            var _context = GetAndroidContext();
            AndroidJavaObject _javaString = new AndroidJavaObject("java.lang.String", Application.dataPath);
            androidJavaClass.CallStatic("setApplicationDirectory", _javaString);
        }
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            var _context = GetAndroidContext();
            AndroidJavaObject _javaString = new AndroidJavaObject("java.lang.String", "DevID");
            androidJavaClass.CallStatic("StartService", _context, _javaString, 8);
        }
#else
        /* nop */
        Debug.Log("OnStartService - nop.");
#endif
    } /* OnStartService */

    public void OnStopService()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            androidJavaClass.CallStatic("StopService");
        }
#else
        /* nop */
        Debug.Log("OnStopService - nop.");
#endif
    } /* OnStopService */

    public float GetWaterTmp()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            return androidJavaClass.CallStatic<float>("GetWaterTmp");
        }
#else
        /* nop */
        Debug.Log("GetWaterTmp - nop.");
        return -273.0f;
#endif
    } /* GetWaterTmp */

    public float GetOilTmp()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            return androidJavaClass.CallStatic<float>("GetOilTmp");
        }
#else
        /* nop */
        Debug.Log("GetOilTmp - nop.");
        return -273.0f;
#endif
    } /* GetOilTmp */

    public float GetOilPress()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            return androidJavaClass.CallStatic<float>("GetOilPress");
        }
#else
        /* nop */
        Debug.Log("GetOilPress - nop.");
        return 0.0f;
#endif
    } /* GetOilPress */

    public float GetBoostPress()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            return androidJavaClass.CallStatic<float>("GetBoostPress");
        }
#else
        /* nop */
        Debug.Log("GetBoostPress - nop.");
        return 0.0f;
#endif
    } /* GetBoostPress */

    public float GetRoomTemp()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            return androidJavaClass.CallStatic<float>("GetRoomTemp");
        }
#else
        /* nop */
        Debug.Log("GetRoomTemp - nop.");
        return -273.0f;
#endif
    } /* GetRoomTemp */

    public string GetDataLine()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            return androidJavaClass.CallStatic<string>("GetDataLine");
        }
#else
        /* nop */
        Debug.Log("GetDataLine - nop.");
        return string.Empty;
#endif
    } /* GetDataLine */

    public float GetAngleX()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            return androidJavaClass.CallStatic<float>("GetAngleX");
        }
#else
        /* nop */
        Debug.Log("GetAngleX - nop.");
        return 0.0f;
#endif
    } /* GetAngleX */

    public float GetAngleY()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            return androidJavaClass.CallStatic<float>("GetAngleY");
        }
#else
        /* nop */
        Debug.Log("GetAngleY - nop.");
        return 0.0f;
#endif
    } /* GetAngleY */

    public float GetAngleZ()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            return androidJavaClass.CallStatic<float>("GetAngleZ");
        }
#else
        /* nop */
        Debug.Log("GetAngleZ - nop.");
        return 0.0f;
#endif
    } /* GetAngleZ */


    public bool GetIsDeviceOpen()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            return androidJavaClass.CallStatic<bool>("GetIsDeviceOpen");
        }
#else
        /* nop */
        Debug.Log("GetIsDeviceOpen - nop.");
        return false;
#endif
    } /* GetIsDeviceOpen */

    public void OnToast()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            var _context = GetAndroidContext();
            AndroidJavaObject _javaString = new AndroidJavaObject("java.lang.String", "ToastText");
            androidJavaClass.CallStatic("Toast",_context , _javaString);
        }
#else
        /* nop */
        Debug.Log("OnToast - nop.");
#endif
    } /* OnToast */

    public void OnGetStringTest()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            string _str = androidJavaClass.CallStatic<string>("getStringTest");
            if( m_DebugText != null )
            {
                m_DebugText.text = _str;
            }
        }
#else
        /* nop */
        Debug.Log("OnGetStringTest - nop.");
#endif
    } /* OnGetStringTest */

    public void OnGetFilePath()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            string _str = androidJavaClass.CallStatic<string>("getFilePath");
            if( m_DebugText2 != null )
            {
                m_DebugText2.text = _str;
            }
        }
#else
        /* nop */
        Debug.Log("OnGetFilePath - nop.");
#endif
    } /* OnGetFilePath */

    public void OnGetPluginVersion()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            var _context = GetAndroidContext();
            string _str = androidJavaClass.CallStatic<string>("getPluginVersion",_context);
            if( m_DebugText != null )
            {
                m_DebugText.text = _str;
            }
        }
#else
        /* nop */
        Debug.Log("OnGetPluginVersion - nop.");
#endif
    } /* OnGetPluginVersion */

    public void OnGetDataLine()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(CCLASS_ID))
        {
            int _recvCount = androidJavaClass.CallStatic<int>("GetRecvCount");
            string _str = androidJavaClass.CallStatic<string>("GetDataLine");
            if( m_DebugText2 != null )
            {
                m_DebugText2.text = "**" + _recvCount + ":" + _str + "**";
            }
        }
#else
        /* nop */
        Debug.Log("GetDataLine - nop.");
#endif
    } /* OnGetStringTest */


} /* class */

