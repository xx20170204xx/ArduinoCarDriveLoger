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
    } /* resetValue */

    private void Update()
    {
    } /* Update */

}
