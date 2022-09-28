using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeterUnitController : MonoBehaviour
{
    [Tooltip("水温")]
    [SerializeField]
    private MeterBase waterTempMeter = null;
    [Tooltip("油温")]
    [SerializeField]
    private MeterBase oilTempMeter = null;
    [Tooltip("油圧")]
    [SerializeField]
    private MeterBase oilPressMeter = null;

    [Tooltip("回転数")]
    [SerializeField]
    private MeterBase tachoMeter = null;
    [Tooltip("速度")]
    [SerializeField]
    private MeterBase speedMeter = null;

    [Tooltip("減速比")]
    [SerializeField]
    private MeterBase gearRatioMeter = null;

    struct DataValue{
        public float m_waterTemp;
        public float m_oilTemp;
        public float m_oilPress;
        public float m_tacho;
        public float m_speed;
    }
    private DataValue m_data;

    public float WaterTemp { get { return m_data.m_waterTemp; }  }
    public float OilTemp { get { return m_data.m_oilTemp; } }
    public float OilPress { get { return m_data.m_oilPress; } }
    public float Tacho { get { return m_data.m_tacho; } }
    public float Speed { get { return m_data.m_speed; } }

    public void OnDataReceived(string message)
    {
        string[] values = message.Split('\t');

        m_data.m_waterTemp = float.Parse(values[1]);
        m_data.m_oilTemp = float.Parse(values[2]);
        m_data.m_oilPress = float.Parse(values[3]);
        m_data.m_tacho = float.Parse(values[4]);
        m_data.m_speed = float.Parse(values[5]);

        /* 水温・油温・油圧 */
        if (waterTempMeter != null) waterTempMeter.Value = m_data.m_waterTemp;
        if(oilTempMeter != null) oilTempMeter.Value = m_data.m_oilTemp;
        if (oilPressMeter != null) oilPressMeter.Value = m_data.m_oilPress;

        /* 回転数・速度・減速比 */
        if (tachoMeter != null) tachoMeter.Value = m_data.m_tacho;
        if (speedMeter != null) speedMeter.Value = m_data.m_speed;
        if (gearRatioMeter != null) gearRatioMeter.Value = tachoMeter.Value / speedMeter.Value;

        // SerialReceive.Instance

    } /* OnDataReceived */

    public void resetValue()
    {
        if (waterTempMeter != null) waterTempMeter.resetValue();
        if (oilTempMeter != null) oilTempMeter.resetValue();
        if (oilPressMeter != null) oilPressMeter.resetValue();
        if (tachoMeter != null) tachoMeter.resetValue();
        if (speedMeter != null) speedMeter.resetValue();
        if (gearRatioMeter != null) gearRatioMeter.resetValue();

        m_data.m_waterTemp = 0;
        m_data.m_oilTemp = 0;
        m_data.m_oilPress = 0;
        m_data.m_tacho = 0;
        m_data.m_speed = 0;

        ReloadSetting();
    } /* resetValue */

    public void ReloadSetting()
    {
        if (waterTempMeter != null)
        {
            var _type = MeterBase.MeterType.TYPE_WATER_TEMP;
            var _setting = SerialReceive.Instance.m_MeterSetting[_type];
            var _meter = waterTempMeter;
            do {
                _meter.lowValue = _setting.m_lowValue;
                _meter.highValue = _setting.m_highValue;
                _meter.lowColor = _setting.m_lowColor;
                _meter.highColor = _setting.m_highColor;
            } while ( _meter = _meter.subMeter);
        }
        if (oilTempMeter != null)
        {
            var _type = MeterBase.MeterType.TYPE_OIL_TEMP;
            var _setting = SerialReceive.Instance.m_MeterSetting[_type];
            var _meter = oilTempMeter;
            do
            {
                _meter.lowValue = _setting.m_lowValue;
                _meter.highValue = _setting.m_highValue;
                _meter.lowColor = _setting.m_lowColor;
                _meter.highColor = _setting.m_highColor;
            } while (_meter = _meter.subMeter);
        }
        if (oilPressMeter != null)
        {
            var _type = MeterBase.MeterType.TYPE_OIL_PRESS;
            var _setting = SerialReceive.Instance.m_MeterSetting[_type];
            var _meter = oilPressMeter;
            do
            {
                _meter.lowValue = _setting.m_lowValue;
                _meter.highValue = _setting.m_highValue;
                _meter.lowColor = _setting.m_lowColor;
                _meter.highColor = _setting.m_highColor;
            } while (_meter = _meter.subMeter);
        }
        if (tachoMeter != null)
        {
            var _type = MeterBase.MeterType.TYPE_TACHO;
            var _setting = SerialReceive.Instance.m_MeterSetting[_type];
            var _meter = tachoMeter;
            do
            {
                _meter.lowValue = _setting.m_lowValue;
                _meter.highValue = _setting.m_highValue;
                _meter.lowColor = _setting.m_lowColor;
                _meter.highColor = _setting.m_highColor;

                Debug.Log("Meter Type" + _meter.GetType().ToString());
                if (_meter.GetType() == typeof(TachoShiftLampMeter))
                {
                    ((TachoShiftLampMeter)_meter).shiftValue = _setting.m_blinkValue;
                }

            } while (_meter = _meter.subMeter);
        }
        if (speedMeter != null)
        {
            var _type = MeterBase.MeterType.TYPE_SPEED;
            var _setting = SerialReceive.Instance.m_MeterSetting[_type];
            var _meter = speedMeter;
            do
            {
                _meter.lowValue = _setting.m_lowValue;
                _meter.highValue = _setting.m_highValue;
                _meter.lowColor = _setting.m_lowColor;
                _meter.highColor = _setting.m_highColor;

            } while (_meter = _meter.subMeter);
        }
    } /* ReloadSetting */

#if true// UNITY_EDITOR
    private IEnumerator coroutine;

    public void StartDemoPlay()
    {
        coroutine = DemoPlay();
        StartCoroutine(coroutine);
    } /* StartDemoPlay */

    private IEnumerator DemoPlay()
    {
        bool _end_flag = true;
        float _wait_time = 1.0f / 60.0f;

        float _wtemp = -10.0f;
        float _otemp = -10.0f;
        float _oil_press = 0.0f;
        float _tacho = 750f;
        float _speed = 0f;
        int _gear = 1;
        const int GEAR_MAX = 5;
        float _shift_timing = 7500.0f;

        Debug.Log("Start Demo.");
        while (_end_flag)
        {
            yield return new WaitForSeconds(_wait_time);

            _wtemp += 50f / 60.0f;
            if (_wtemp >= 200.0f) _wtemp = -10.0f;
            _otemp += 30f / 60.0f;
            if (_otemp >= 200.0f) _otemp = -10.0f;
            _oil_press += 1f / 60.0f;
            if (_oil_press >= 10.0f) _oil_press = 0;
            _tacho += 1000f / 60.0f;
            _speed = _tacho / (((GEAR_MAX+1) - _gear) * 40);
            if (_tacho >= _shift_timing) {
                _gear += 1;
                _tacho = _speed * (((GEAR_MAX + 1) - _gear) * 40);
            }

            string _mes; ;
            _mes = string.Format("V\t{0}\t{1}\t{2}\t{3}\t{4}\n",
                _wtemp, _otemp,_oil_press,_tacho,_speed);
            SerialReceive.Instance.
            OnDataReceived(_mes);
            if (_gear >= (GEAR_MAX+1)) 
            {
                _end_flag = false;
            }
        }

        {/* 終了時 */
            _wtemp = 0;
            _otemp = 0;
            _oil_press = 0;
            _tacho = 0;
            _speed = 0;
            string _mes; ;
            _mes = string.Format("V\t{0}\t{1}\t{2}\t{3}\t{4}\n",
                _wtemp, _otemp, _oil_press, _tacho, _speed);
            SerialReceive.Instance.
            OnDataReceived(_mes);
        }
        Debug.Log("End Demo.");
    } /* DemoPlay */
#endif


}/* class */
