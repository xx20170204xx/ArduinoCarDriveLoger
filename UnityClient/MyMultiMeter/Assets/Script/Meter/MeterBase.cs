using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeterBase : MonoBehaviour
{
    public MeterBase subMeter;
    public Text text = null;

    /*
     * https://docs.microsoft.com/ja-jp/dotnet/standard/base-types/standard-numeric-format-strings
     */
    [SerializeField]
    private string format = "{0:F0}";

    private float meterValue;

    public float Value { get { return this.meterValue; } set { this.meterValue = value; if (this.peak < value) { this.peak = value; } } }
    public float peak;

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
}

