using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeterBase : MonoBehaviour
{
    public Text text = null;

    /*
     * https://docs.microsoft.com/ja-jp/dotnet/standard/base-types/standard-numeric-format-strings
     */
    [SerializeField]
    private string format = "{0:F0}";

    private float meterValue;

    public float Value { get { return this.meterValue; } set { this.meterValue = value; if (this.peak < value) { this.peak = value; } } }
    public float peak;

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

        UpdateText();
    } /* Update */

    protected void UpdateText()
    {
        if (text == null)
        {
            return;
        }
        text.text = string.Format(format, Value);
    } /* UpdateText */
}

