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

    public void OnDataReceived(string message)
    {
        string[] values = message.Split('\t');
        /* 水温・油温・油圧 */
        if(waterTempMeter != null) waterTempMeter.Value = float.Parse(values[1]);
        if(oilTempMeter != null) oilTempMeter.Value = float.Parse(values[2]);
        if (oilPressMeter != null) oilPressMeter.Value = float.Parse(values[3]);

        /* 回転数・速度・減速比 */
        if (tachoMeter != null) tachoMeter.Value = float.Parse(values[4]);
        if (speedMeter != null) speedMeter.Value = float.Parse(values[5]);
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
                _meter.normalColor = _setting.m_normalColor;
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
                _meter.normalColor = _setting.m_normalColor;
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
                _meter.normalColor = _setting.m_normalColor;
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
                _meter.normalColor = _setting.m_normalColor;
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
                _meter.normalColor = _setting.m_normalColor;
                _meter.highColor = _setting.m_highColor;

            } while (_meter = _meter.subMeter);
        }
    } /* ReloadSetting */

#if UNITY_EDITOR
    private IEnumerator coroutine;

    public void StartDemoMode()
    {
        coroutine = WaitAndPrint(2.0f);
        StartCoroutine(coroutine);
    } /* StartDemoMode */

    private IEnumerator WaitAndPrint(float waitTime)
    {
        bool _end_flag = true;
        float _wait_time = 1.0f / 60.0f;

        float _temp = -10.0f;
        float _oil_press = 0.0f;
        float _tacho = 750f;
        float _speed = 0f;
        int _gear = 1;

        Debug.Log("Start Demo.");
        while (_end_flag)
        {
            yield return new WaitForSeconds(_wait_time);

            _temp += 10f / 60.0f;
            if (_temp >= 200.0f) _temp = -10.0f;
            _oil_press += 1f / 60.0f;
            if (_oil_press >= 10.0f) _oil_press = 0;
            _tacho += 1000f / 60.0f;
            _speed = _tacho / ((6 - _gear) * 40);
            if (_tacho >= 9000.0f) { 
                _tacho = 750.0f;
                _gear += 1;
            }

            string _mes = "V\t1.0\t1.0\t1.0\t1.0\t1.0\t\n";
            _mes = string.Format("V\t{0}\t{1}\t{2}\t{3}\t{4}\n",
                _temp, _temp,_oil_press,_tacho,_speed);
            OnDataReceived(_mes);
            if (_gear >= 6) 
            {
                _end_flag = false;
            }
        }
        Debug.Log("End Demo.");
    }
#endif


}/* class */
