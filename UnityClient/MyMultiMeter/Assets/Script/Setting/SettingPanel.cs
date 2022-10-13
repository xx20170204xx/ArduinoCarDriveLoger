using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingPanel : MonoBehaviour
{
    [Header("Tacho")]
    [SerializeField]
    private SettingValue m_tachoLowValue;
    [SerializeField]
    private SettingValue m_tachoHighValue;
    [SerializeField]
    private SettingValue m_tachoBlinkValue;
    [SerializeField]
    private SettingColor m_tachoLowColor;
    [SerializeField]
    private SettingColor m_tachoHighColor;

    [Space]
    [Header("Speed")]
    [SerializeField]
    private SettingValue m_speedLowValue;
    [SerializeField]
    private SettingValue m_speedHighValue;
    [SerializeField]
    private SettingColor m_speedLowColor;
    [SerializeField]
    private SettingColor m_speedHighColor;

    [Space]
    [Header("WaterTemp")]
    [SerializeField]
    private SettingValue m_waterLowValue;
    [SerializeField]
    private SettingValue m_waterHighValue;
    [SerializeField]
    private SettingColor m_waterLowColor;
    [SerializeField]
    private SettingColor m_waterHighColor;

    [Space]
    [Header("OilTemp")]
    [SerializeField]
    private SettingValue m_oiltLowValue;
    [SerializeField]
    private SettingValue m_oiltHighValue;
    [SerializeField]
    private SettingColor m_oiltLowColor;
    [SerializeField]
    private SettingColor m_oiltHighColor;

    [Space]
    [Header("OilPress")]
    [SerializeField]
    private SettingValue m_oilpLowValue;
    [SerializeField]
    private SettingValue m_oilpHighValue;
    [SerializeField]
    private SettingColor m_oilpLowColor;
    [SerializeField]
    private SettingColor m_oilpHighColor;

    [Space]
    [Header("BoostPress")]
    [SerializeField]
    private SettingValue m_boostLowValue;
    [SerializeField]
    private SettingValue m_boostHighValue;
    [SerializeField]
    private SettingColor m_boostLowColor;
    [SerializeField]
    private SettingColor m_boostHighColor;

    void Awake()
    {
        /* Tacho */
        m_tachoLowValue.minValue = 0;
        m_tachoLowValue.maxValue = 15000;
        m_tachoLowValue.stepAmount = 100;

        m_tachoHighValue.stepAmount = 100;
        m_tachoHighValue.minValue = 0;
        m_tachoHighValue.maxValue = 15000;

        m_tachoBlinkValue.stepAmount = 100;
        m_tachoBlinkValue.minValue = 0;
        m_tachoBlinkValue.maxValue = 15000;

        /* Speed */
        m_speedLowValue.stepAmount = 1;
        m_speedLowValue.minValue = 0;
        m_speedLowValue.maxValue = 300;

        m_speedHighValue.stepAmount = 1;
        m_speedHighValue.minValue = 0;
        m_speedHighValue.maxValue = 300;

        /* Water Temp */
        m_waterLowValue.stepAmount = 1;
        m_waterLowValue.minValue = 0;
        m_waterLowValue.maxValue = 200;

        m_waterHighValue.stepAmount = 1;
        m_waterHighValue.minValue = 0;
        m_waterHighValue.maxValue = 200;

        /* Oil Temp */
        m_oiltLowValue.stepAmount = 1;
        m_oiltLowValue.minValue = 0;
        m_oiltLowValue.maxValue = 200;

        m_oiltHighValue.stepAmount = 1;
        m_oiltHighValue.minValue = 0;
        m_oiltHighValue.maxValue = 200;

        /* Oil Press */
        m_oilpLowValue.stepAmount = 1;
        m_oilpLowValue.minValue = 0;
        m_oilpLowValue.maxValue = 200;

        m_oilpHighValue.stepAmount = 1;
        m_oilpHighValue.minValue = 0;
        m_oilpHighValue.maxValue = 200;

        /* Oil Press */
        m_oilpLowValue.stepAmount = 1;
        m_oilpLowValue.minValue = 0;
        m_oilpLowValue.maxValue = 10;

        m_oilpHighValue.stepAmount = 1;
        m_oilpHighValue.minValue = 0;
        m_oilpHighValue.maxValue = 10;

        /* Boost Press */
        m_boostLowValue.stepAmount = 1f;
        m_boostLowValue.minValue = -100;
        m_boostLowValue.maxValue = 100;

        m_boostHighValue.stepAmount = 1f;
        m_boostHighValue.minValue = 0;
        m_boostHighValue.maxValue = 300;
    } /* Awake */

    void Start()
    {
        SerialReceive.Instance.LoadDeviceInfo();
        {
            var _tacho = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_TACHO];
            m_tachoLowValue.Value = _tacho.m_lowValue;
            m_tachoHighValue.Value = _tacho.m_highValue;
            m_tachoBlinkValue.Value = _tacho.m_blinkValue;
            m_tachoLowColor.SetColor(_tacho.m_lowColor);
            m_tachoHighColor.SetColor(_tacho.m_highColor);
        }

        {
            var _speed = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_SPEED];
            m_speedLowValue.Value = _speed.m_lowValue;
            m_speedHighValue.Value = _speed.m_highValue;
            m_speedLowColor.SetColor(_speed.m_lowColor);
            m_speedHighColor.SetColor(_speed.m_highColor);
        }

        {
            var _water = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_WATER_TEMP];
            m_waterLowValue.Value = _water.m_lowValue;
            m_waterHighValue.Value = _water.m_highValue;
            m_waterLowColor.SetColor(_water.m_lowColor);
            m_waterHighColor.SetColor(_water.m_highColor);
        }

        {
            var _oilTemp = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_OIL_TEMP];
            m_oiltLowValue.Value = _oilTemp.m_lowValue;
            m_oiltHighValue.Value = _oilTemp.m_highValue;
            m_oiltLowColor.SetColor(_oilTemp.m_lowColor);
            m_oiltHighColor.SetColor(_oilTemp.m_highColor);
        }

        {
            var _oilPress = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_OIL_PRESS];
            m_oilpLowValue.Value = _oilPress.m_lowValue;
            m_oilpHighValue.Value = _oilPress.m_highValue;
            m_oilpLowColor.SetColor(_oilPress.m_lowColor);
            m_oilpHighColor.SetColor(_oilPress.m_highColor);
        }

        {
            var _boostPress = SerialReceive.Instance.m_MeterSetting[MeterBase.MeterType.TYPE_BOOST_PRESS];
            m_boostLowValue.Value = _boostPress.m_lowValue;
            m_boostHighValue.Value = _boostPress.m_highValue;
            m_boostLowColor.SetColor(_boostPress.m_lowColor);
            m_boostHighColor.SetColor(_boostPress.m_highColor);
        }


    } /* Start */


    public void OnSettingUpdate() 
    {
        {
            MeterBase.MeterType _type = MeterBase.MeterType.TYPE_TACHO;
            var _tacho = SerialReceive.Instance.m_MeterSetting[_type];
            _tacho.m_lowValue = m_tachoLowValue.Value;
            _tacho.m_highValue = m_tachoHighValue.Value;
            _tacho.m_blinkValue = m_tachoBlinkValue.Value;
            _tacho.m_lowColor = m_tachoLowColor.Color;
            _tacho.m_highColor = m_tachoHighColor.Color;
            SerialReceive.Instance.m_MeterSetting[_type] = _tacho;
        }

        {
            MeterBase.MeterType _type = MeterBase.MeterType.TYPE_SPEED;
            var _speed = SerialReceive.Instance.m_MeterSetting[_type];
            _speed.m_lowValue = m_speedLowValue.Value;
            _speed.m_highValue = m_speedHighValue.Value;
            _speed.m_lowColor = m_speedLowColor.Color;
            _speed.m_highColor = m_speedHighColor.Color;
            SerialReceive.Instance.m_MeterSetting[_type] = _speed;
        }

        {
            MeterBase.MeterType _type = MeterBase.MeterType.TYPE_WATER_TEMP;
            var _water = SerialReceive.Instance.m_MeterSetting[_type];
            _water.m_lowValue = m_waterLowValue.Value;
            _water.m_highValue = m_waterHighValue.Value;
            _water.m_lowColor = m_waterLowColor.Color;
            _water.m_highColor = m_waterHighColor.Color;
            SerialReceive.Instance.m_MeterSetting[_type] = _water;
        }

        {
            MeterBase.MeterType _type = MeterBase.MeterType.TYPE_OIL_TEMP;
            var _oilTemp = SerialReceive.Instance.m_MeterSetting[_type];
            _oilTemp.m_lowValue = m_oiltLowValue.Value;
            _oilTemp.m_highValue = m_oiltHighValue.Value;
            _oilTemp.m_lowColor = m_oiltLowColor.Color;
            _oilTemp.m_highColor = m_oiltHighColor.Color;
            SerialReceive.Instance.m_MeterSetting[_type] = _oilTemp;
        }

        {
            MeterBase.MeterType _type = MeterBase.MeterType.TYPE_OIL_PRESS;
            var _oilPress = SerialReceive.Instance.m_MeterSetting[_type];
            _oilPress.m_lowValue = m_oilpLowValue.Value;
            _oilPress.m_highValue = m_oilpHighValue.Value;
            _oilPress.m_lowColor = m_oilpLowColor.Color;
            _oilPress.m_highColor = m_oilpHighColor.Color;
            SerialReceive.Instance.m_MeterSetting[_type] = _oilPress;
        }

        {
            MeterBase.MeterType _type = MeterBase.MeterType.TYPE_BOOST_PRESS;
            var _boostPress = SerialReceive.Instance.m_MeterSetting[_type];
            _boostPress.m_lowValue = m_boostLowValue.Value;
            _boostPress.m_highValue = m_boostHighValue.Value;
            _boostPress.m_lowColor = m_boostLowColor.Color;
            _boostPress.m_highColor = m_boostHighColor.Color;
            SerialReceive.Instance.m_MeterSetting[_type] = _boostPress;
        }

        SerialReceive.Instance.SaveDeviceInfo();
    } /* OnUpdate */
} /* class */
