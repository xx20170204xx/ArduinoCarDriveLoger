using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class InfoSoundController : MonoBehaviour
{
    [Header("Info")]
    [SerializeField]
    private AudioClip m_InfoTachoClip;
    [SerializeField]
    private AudioClip m_InfoEngStlClip;

    [Header("Warning")]
    [SerializeField]
    private AudioClip m_WarningTachoClip;
    [SerializeField]
    private AudioClip m_WarningSpeedClip;
    [SerializeField]
    private AudioClip m_WarningWaterLowClip;
    [SerializeField]
    private AudioClip m_WarningWaterHighClip;
    [SerializeField]
    private AudioClip m_WarningOilTLowClip;
    [SerializeField]
    private AudioClip m_WarningOilTHighClip;

    private AudioSource m_audioSource;

    private bool m_isTachoMove = false;
    private bool m_isTachoMovePrev = false;
    private float m_isTachoNormal = 0.0f;
    private float m_EngStlCount = 0.0f;

    private bool m_isWaterTempMove = false;
    private bool m_isWaterTempMovePrev = false;
    private float m_isWaterTempNormal = 0.0f;
    private float m_isWaterTempWarning = 0.0f;

    private bool m_isOilTempMove = false;
    private bool m_isOilTempMovePrev = false;
    private float m_isOilTempNormal = 0.0f;
    private float m_isOilTempWarning = 0.0f;

    private Dictionary<AudioClip,float> m_AudioList= new Dictionary<AudioClip,float>();
    List<AudioClip> m_AudioKeys = new List<AudioClip>();

    void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
        /* Warning */
        m_AudioList.Add(m_WarningTachoClip, 0.0f);
        m_AudioList.Add(m_WarningSpeedClip, 0.0f);
        m_AudioList.Add(m_WarningWaterLowClip, 0.0f);
        m_AudioList.Add(m_WarningWaterHighClip, 0.0f);
        m_AudioList.Add(m_WarningOilTLowClip, 0.0f);
        m_AudioList.Add(m_WarningOilTHighClip, 0.0f);
        /* Info */
        m_AudioList.Add(m_InfoTachoClip, 0.0f);
        m_AudioList.Add(m_InfoEngStlClip, 0.0f);
        foreach (var _key in m_AudioList.Keys)
        {
            m_AudioKeys.Add(_key);
        }


    } /* Awake */

    void Update()
    {
        var _meter = SerialReceive.Instance.controller;
        CheckSpeed(_meter);
        CheckTacho(_meter);
        CheckWaterTemp(_meter);
        CheckOilTemp(_meter);
        PlaySound();
    } /* Update */

    private void PlaySound()
    {
        foreach (var _key in m_AudioKeys)
        {
            float _v = m_AudioList[_key];
            if (_v > 0.0f)
            {
                _v -= Time.deltaTime;
                m_AudioList[_key] = _v;
            }
        }

        /* 再生中の場合、関数を抜ける */
        if (m_audioSource.isPlaying == true)
        {
            return;
        }

        foreach (var _key  in m_AudioKeys)
        {
            float _v = m_AudioList[_key];
            if (_v <= -1.5f)
            {
                /* Play */
                m_audioSource.PlayOneShot(_key);
                /* X分間はチェックをしても鳴らさない */
                m_AudioList[_key] = 60 * 1.0f;
                break;
            }
        }
    } /* PlaySound */

    private void CheckSpeed(MeterUnitController _controller)
    {
        var _set = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_SPEED];
        float _value = _controller.Speed;
        if (_value > _set.m_highValue )
        {
            if (m_AudioList[m_WarningSpeedClip] <= 0.0f)
            {
                m_AudioList[m_WarningSpeedClip] = -2.0f;
            }
        }
    } /* CheckSpeed */

    private void CheckTacho(MeterUnitController _controller)
    {
        var _set = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_TACHO];
        float _value = _controller.Tacho;

        if (_value >= _set.m_lowValue)
        {
            m_isTachoMovePrev = m_isTachoMove;
            m_isTachoMove = true;
            if (_value >= _set.m_lowValue && _value <= _set.m_highValue)
            {
                m_isTachoNormal += Time.deltaTime;
            }
            else
            {
                if (m_isTachoNormal > 0.0f)
                {
                    m_isTachoNormal -= Time.deltaTime;
                }
            }
        }
        else {
            m_isTachoMovePrev = m_isTachoMove;
            m_isTachoMove = false;
            if (m_isTachoNormal > 0.0f)
            {
                m_isTachoNormal -= Time.deltaTime;
            }
        }

        /* エンスト検知 */
        /* 適正状態が 1秒 以上かつ 現在値が 0 以下の場合 */
        if (m_isTachoNormal > 1.0f && _value <= 0f)
        {
            m_EngStlCount += Time.deltaTime;
            /* 検知回数が 1秒 を超えた場合 */
            if (m_EngStlCount >= 1.0f) 
            {
                if (m_AudioList[m_InfoEngStlClip] <= 0.0f)
                {
                    m_AudioList[m_InfoEngStlClip] = -2.0f;
                }
            }
        }
        else if (m_isTachoNormal > 1.0f && _value >= _set.m_blinkValue)
        {
            if (m_AudioList[m_InfoTachoClip] <= 0.0f)
            {
                m_AudioList[m_InfoTachoClip] = -2.0f;
            }
        }
        else if (m_isTachoNormal > 1.0f && _value >= _set.m_highValue)
        {
            if (m_AudioList[m_WarningTachoClip] <= 0.0f)
            {
                m_AudioList[m_WarningTachoClip] = -2.0f;
            }
        }
        else{
            m_EngStlCount = 0.0f;
        }

    } /* CheckTacho */

    private void CheckWaterTemp(MeterUnitController _controller)
    {
        var _set = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_WATER_TEMP];
        float _value = _controller.WaterTemp;

        if (_value >= _set.m_lowValue && _value <= _set.m_highValue)
        {
            /* 温度が適正内の場合 */
            m_isWaterTempMovePrev = m_isWaterTempMove;
            m_isWaterTempMove = true;
            m_isWaterTempNormal += Time.deltaTime;
            m_isWaterTempWarning = 0.0f;
        }
        else
        {
            m_isWaterTempMovePrev = m_isWaterTempMove;
            m_isWaterTempMove = false;
            m_isWaterTempNormal = 0.0f;
            m_isWaterTempWarning += Time.deltaTime;
        }

        if (m_isWaterTempMovePrev == true && _value >= _set.m_highValue)
        {
            if (m_AudioList[m_WarningWaterHighClip] <= 0.0f)
            {
                m_AudioList[m_WarningWaterHighClip] = -2.0f;
            }
        }
        else if (m_isWaterTempMovePrev == true && _value <= _set.m_lowValue)
        {
            if (m_AudioList[m_WarningWaterLowClip] <= 0.0f)
            {
                m_AudioList[m_WarningWaterLowClip] = -2.0f;
            }
        }else{
        }
    } /* CheckWaterTemp */

    private void CheckOilTemp(MeterUnitController _controller)
    {
        var _set = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_OIL_TEMP];
        float _value = _controller.OilTemp;

        if (_value >= _set.m_lowValue && _value <= _set.m_highValue)
        {
            /* 温度が適正内の場合 */
            m_isOilTempMovePrev = m_isOilTempMove;
            m_isOilTempMove = true;
            m_isOilTempNormal += Time.deltaTime;
            m_isOilTempWarning = 0.0f;
        }
        else
        {
            m_isOilTempMovePrev = m_isOilTempMove;
            m_isOilTempMove = false;
            m_isOilTempNormal = 0.0f;
            m_isOilTempWarning += Time.deltaTime;
        }

        if (m_isOilTempMovePrev == true && _value >= _set.m_highValue)
        {
            if (m_AudioList[m_WarningOilTHighClip] <= 0.0f)
            {
                m_AudioList[m_WarningOilTHighClip] = -2.0f;
            }
        }
        else if (m_isOilTempMovePrev == true && _value <= _set.m_lowValue)
        {
            if (m_AudioList[m_WarningOilTLowClip] <= 0.0f)
            {
                m_AudioList[m_WarningOilTLowClip] = -2.0f;
            }
        }
        else
        {
        }
    } /* CheckOilTemp */



} /* class */
