using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeterUnitController : MonoBehaviour
{
    [SerializeField]
    private MeterBase waterTempMeter = null;
    [SerializeField]
    private MeterBase oilTempMeter = null;
    [SerializeField]
    private MeterBase oilPressMeter = null;

    [SerializeField]
    private MeterBase tachoMeter = null;
    [SerializeField]
    private MeterBase speedMeter = null;

    [SerializeField]
    private MeterBase Meter = null;

    public void OnDataReceived(string message)
    {
        string[] values = message.Split('\t');
        waterTempMeter.Value = float.Parse(values[4]);
        oilTempMeter.Value = float.Parse(values[5]);
        oilPressMeter.Value = float.Parse(values[6]);

        tachoMeter.Value = float.Parse(values[7]);
        speedMeter.Value = float.Parse(values[8]);
        Meter.Value = tachoMeter.Value / speedMeter.Value;
    } /* OnDataReceived */

    public void resetValue()
    {
        waterTempMeter.resetValue();
        oilTempMeter.resetValue();
        oilPressMeter.resetValue();
        tachoMeter.resetValue();
        speedMeter.resetValue();
        Meter.resetValue();
    } /* resetValue */
}
