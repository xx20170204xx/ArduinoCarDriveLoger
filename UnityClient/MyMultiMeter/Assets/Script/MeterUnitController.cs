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
        if(waterTempMeter) waterTempMeter.Value = float.Parse(values[1]);
        if(oilTempMeter)oilTempMeter.Value = float.Parse(values[2]);
        if (oilPressMeter) oilPressMeter.Value = float.Parse(values[3]);

        if (tachoMeter) tachoMeter.Value = float.Parse(values[4]);
        if (speedMeter) speedMeter.Value = float.Parse(values[5]);
        if (Meter) Meter.Value = tachoMeter.Value / speedMeter.Value;
    } /* OnDataReceived */

    public void resetValue()
    {
        if (waterTempMeter) waterTempMeter.resetValue();
        if (oilTempMeter) oilTempMeter.resetValue();
        if (oilPressMeter) oilPressMeter.resetValue();
        if (tachoMeter) tachoMeter.resetValue();
        if (speedMeter) speedMeter.resetValue();
        if (Meter) Meter.resetValue();
    } /* resetValue */
}
