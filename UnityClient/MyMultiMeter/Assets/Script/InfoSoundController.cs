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

    private float m_updateTime = 5.0f * 60.0f;

    private float m_speedWarning = 0.0f;

    private float m_tacho1Warning = 0.0f;
    private float m_tacho2Warning = 0.0f;
    private float m_isTachoNormal = 0.0f;
    private float m_EngStlCount = 0.0f;

    private float m_isWaterTempNormal = 0.0f;

    private float m_isOilTempNormal = 0.0f;

    private Dictionary<AudioClip,float> m_AudioList= new Dictionary<AudioClip,float>();
    List<AudioClip> m_AudioKeys = new List<AudioClip>();

    void Awake()
    {
        m_updateTime = 5.0f * 60.0f;
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
        PlaySoundFunc();
        UpdateLastDate();
    } /* Update */

    private void UpdateLastDate()
    {
        m_updateTime -= Time.deltaTime;
        if (m_updateTime < 0.0f)
        {
            m_updateTime = 5.0f * 60.0f;

            SerialReceive.Instance.SaveLastDate();
        }

    } /* UpdateLastDate */

    private void PlaySoundFunc()
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
    } /* PlaySoundFunc */

    private void AddPlaySound(AudioClip _clip)
    {
        if (m_AudioList[_clip] <= 0.0f)
        {
            m_AudioList[_clip] = -2.0f;
        }
    } /* AddPlaySound */

    private void CheckSpeed(MeterUnitController _controller)
    {
        var _set = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_SPEED];
        float _value = SerialReceive.Instance.Speed;
        if (_value > _set.m_highValue)
        {
            m_speedWarning += Time.deltaTime;
            if (m_speedWarning > 1.0f)
            {
                /* 速度超過状態が1秒以上の場合 */
                AddPlaySound(m_WarningSpeedClip);
                m_speedWarning = 0.0f;
            }
        } else {
            m_speedWarning = 0.0f;
        }
    } /* CheckSpeed */

    private void CheckTacho(MeterUnitController _controller)
    {
        var _set = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_TACHO];
        float _value = SerialReceive.Instance.Tacho;

        if (_value >= _set.m_lowValue)
        {
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
        } else {
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
                AddPlaySound(m_InfoEngStlClip);
            }
        }
        else if (m_isTachoNormal > 1.0f && _value >= _set.m_blinkValue)
        {
            m_tacho1Warning += Time.deltaTime;
            if (m_tacho1Warning > 0.1f)
            {
                /* 回転数超過状態が 0.1秒以上の場合 */
                AddPlaySound(m_InfoTachoClip);
                m_tacho1Warning = 0.0f;
            }
        }
        else if (m_isTachoNormal > 1.0f && _value >= _set.m_highValue)
        {
            m_tacho2Warning += Time.deltaTime;
            if (m_tacho2Warning > 0.1f)
            {
                /* 回転数超過状態が 0.1秒以上の場合 */
                AddPlaySound(m_WarningTachoClip);
                m_tacho2Warning = 0.0f;
            }
        }
        else{
            m_EngStlCount = 0.0f;
            m_tacho1Warning = 0.0f;
            m_tacho2Warning = 0.0f;
        }

    } /* CheckTacho */

    private void CheckWaterTemp(MeterUnitController _controller)
    {
        var _set = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_WATER_TEMP];
        float _value = SerialReceive.Instance.WaterTemp;

        if (_value >= _set.m_lowValue && _value <= _set.m_highValue)
        {
            /* 温度が適正内の場合 */
            m_isWaterTempNormal += Time.deltaTime;
        }
        else
        {
            if (m_isWaterTempNormal > 0.0f)
            {
                m_isWaterTempNormal -= Time.deltaTime;
            }
        }

        if (m_isWaterTempNormal > 1.0f && _value >= _set.m_highValue)
        {
            AddPlaySound(m_WarningWaterHighClip);
        }
        else if (m_isWaterTempNormal > 1.0f && _value <= _set.m_lowValue)
        {
            AddPlaySound(m_WarningWaterLowClip);
        }else{
        }
    } /* CheckWaterTemp */

    private void CheckOilTemp(MeterUnitController _controller)
    {
        var _set = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_OIL_TEMP];
        float _value = SerialReceive.Instance.OilTemp;

        if (_value >= _set.m_lowValue && _value <= _set.m_highValue)
        {
            /* 温度が適正内の場合 */
            m_isOilTempNormal += Time.deltaTime;
        }
        else
        {
            if (m_isOilTempNormal > 0.0f)
            {
                m_isOilTempNormal -= Time.deltaTime;
            }
        }

        if (m_isOilTempNormal > 1.0f && _value >= _set.m_highValue)
        {
            AddPlaySound(m_WarningOilTHighClip);
        }
        else if (m_isOilTempNormal > 1.0f && _value <= _set.m_lowValue)
        {
            AddPlaySound(m_WarningOilTLowClip);
        }
        else
        {
        }
    } /* CheckOilTemp */



} /* class */
