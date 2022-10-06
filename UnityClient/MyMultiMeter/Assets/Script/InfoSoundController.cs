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

    private bool isTachoMove = false;
    private bool isTachoMovePrev = false;
    private float isTachoNormal = 0.0f;

    private float m_SpeedWaitTime = 0.0f;

    void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
    } /* Awake */

    void Update()
    {
        var _meter = SerialReceive.Instance.controller;
        CheckSpeed( _meter );
        CheckTacho(_meter);
    } /* Update */

    private void CheckSpeed(MeterUnitController _controller)
    {
        var _set = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_SPEED];
        if (m_SpeedWaitTime > 0.0f)
        {
            m_SpeedWaitTime -= Time.deltaTime;
        }
        if (_controller.Speed > _set.m_highValue )
        {
            if (m_audioSource.isPlaying == false && m_SpeedWaitTime <= 0.0f)
            {
                m_audioSource.PlayOneShot(m_WarningSpeedClip);
                /* X分間はチェックをしても鳴らさない */
                m_SpeedWaitTime = 60.0f * 1f;
            }
        }
    } /* CheckSpeed */

    private void CheckTacho(MeterUnitController _controller)
    {
        var _set = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_TACHO];
        if (_controller.Tacho >= _set.m_lowValue)
        {
            isTachoMovePrev = isTachoMove;
            isTachoMove = true;
            if (_controller.Tacho >= _set.m_lowValue && _controller.Tacho <= _set.m_lowValue)
            {
                isTachoNormal += Time.deltaTime;
            }
            else
            {
                isTachoNormal = 0.0f;
            }
        }
        else {
            isTachoMovePrev = isTachoMove;
            isTachoMove = false;
        }


        /* エンスト検知 */
        if (isTachoMovePrev == true && _controller.Tacho <= 1f)
        {
            if (m_audioSource.isPlaying == false)
            {
                m_audioSource.PlayOneShot(m_InfoEngStlClip);
            }
        }

    } /* CheckTacho */

} /* class */
