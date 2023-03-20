using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SerialServiceAndroid : MonoBehaviour
{
    public const string CCONTEXT_UNITYPLAYER = "com.unity3d.player.UnityPlayer";
    public const string CCLASS_ID = "jp.ne.sakura.jacobi.myserialservicelib.myserialservicelib";

    [SerializeField]
    private Text m_WaterTmpText = null;

    [SerializeField]
    private Text m_DebugText = null;

    private void OnDestroy()
    {
        OnStopService();
    } /* OnDestroy */


    private void Update()
    {
        float _tmp = GetWaterTmp();
        if (m_WaterTmpText != null)
        {
            m_WaterTmpText.text = _tmp.ToString();
        }
        if (m_DebugText != null)
        {
            // m_DebugText.text = GetDataLine();
        }
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
            AndroidJavaObject _javaString = new AndroidJavaObject("java.lang.String", "ToastText");
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


} /* class */

