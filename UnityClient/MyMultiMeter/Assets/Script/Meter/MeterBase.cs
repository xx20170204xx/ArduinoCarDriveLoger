using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeterBase : MonoBehaviour
{
    public enum MeterType 
    {
        TYPE_NONE,

        TYPE_TACHO= 1,
        TYPE_SPEED,
        TYPE_GEAR_RATIO,

        TYPE_WATER_TEMP = 10,
        TYPE_OIL_TEMP,
        TYPE_OIL_PRESS,
        TYPE_BOOST_PRESS,

        TYPE_SPEED_FIX = 100,
    }
    public enum MeterValueType 
    {
        TYPE_NONE,
        TYPE_LOW_VALUE,
        TYPE_HIGH_VALUE,
        TYPE_BLINK_VALUE,

    }

    public enum MeterColorType
    {
        TYPE_NONE,
        TYPE_LOW_COLOR,
        TYPE_NORMAL_COLOR,
        TYPE_HIGH_COLOR,
    }


    public MeterType meterType = MeterType.TYPE_NONE;
    public MeterBase subMeter;
    public Text text = null;

    /*
     * https://docs.microsoft.com/ja-jp/dotnet/standard/base-types/standard-numeric-format-strings
     */
    [SerializeField]
    protected string format = "{0:F0}";

#if false
    [SerializeField]
#if true
    [Range(-273,360)]
#else
    [Range(-273,11000)]  /* tacho用 */
#endif
#endif
    private float meterValue;

    public float Value { get { return this.meterValue; } set { this.meterValue = value; if (this.peak < value) { this.peak = value; } } }
    
    protected float peak;

    public float lowValue = -1.0f;
    public float highValue = 95.0f;

    public Color lowColor = Color.cyan;
    public Color normalColor = Color.white;
    public Color highColor = Color.red;

    public void resetValue(float _value = 0) {
        Value = _value;
        peak = _value;
    } /* resetValue */

    void Start()
    {
    } /* Start */

    // Update is called once per frame
    void Update()
    {
        UpdateSubMeter();
        UpdateText();
    } /* Update */

    protected void UpdateSubMeter()
    {
        if (subMeter != null)
        {
            subMeter.Value = Value;
        }
    } /* UpdateSubMeter */

    protected void UpdateText()
    {
        if (text == null)
        {
            return;
        }
        text.text = string.Format(format, Value);

        text.color = normalColor;
        if (Value < lowValue) { text.color = lowColor; }
        if (Value > highValue) { text.color = highColor; }

    } /* UpdateText */
} /* class */

