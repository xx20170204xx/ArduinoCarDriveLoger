using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        if(waterTempMeter != null) waterTempMeter.Value = float.Parse(values[1]);
        if(oilTempMeter != null) oilTempMeter.Value = float.Parse(values[2]);
        if (oilPressMeter != null) oilPressMeter.Value = float.Parse(values[3]);

        if (tachoMeter != null) tachoMeter.Value = float.Parse(values[4]);
        if (speedMeter != null) speedMeter.Value = float.Parse(values[5]);
        if (Meter != null) Meter.Value = tachoMeter.Value / speedMeter.Value;
    } /* OnDataReceived */

    public void resetValue()
    {
        if (waterTempMeter != null) waterTempMeter.resetValue();
        if (oilTempMeter != null) oilTempMeter.resetValue();
        if (oilPressMeter != null) oilPressMeter.resetValue();
        if (tachoMeter != null) tachoMeter.resetValue();
        if (speedMeter != null) speedMeter.resetValue();
        if (Meter != null) Meter.resetValue();
    } /* resetValue */

    private void Update()
    {
    } /* Update */

}
