using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AnalogMeter : MeterBase
{
    private Quaternion init_rot;
    public Image needle = null;
    public Image needlePeak = null;

    public float valueMin = 0.0f;
    public float valueMax = 360.0f;

    [Range(0,720)]
    public float angle_max = 360.0f;
    void Start()
    {
        if (needle == null)
        {
            this.gameObject.SetActive(false);
            return;
        }
        init_rot = needle.transform.rotation;

    }

    // Update is called once per frame
    void Update()
    {
        UpdateSubMeter();
        Value = Mathf.Clamp(Value, valueMin, valueMax);
        UpdateText();
        UpdateNeedle(needle, Value);
        if (needlePeak != null)
        {
            UpdateNeedle(needlePeak, peak);
        }

    } /* Update */

    protected void UpdateNeedle( Image _needle, float _value )
    {
        if (_needle == null)
        {
            return;
        }
        float angle;
        float _per = valueMax - valueMin;

        angle = angle_max * ((_value - valueMin) / _per);
        angle = -angle;


        _needle.transform.rotation = init_rot * Quaternion.AngleAxis(angle, Vector3.forward);

    } /* UpdateNeedle */

}
