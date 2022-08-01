using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TachoShiftLampMeter : MeterBase
{
    [Serializable]
    public class LampInfo
    {
        [SerializeField]
        public Sprite litSprite;

        [SerializeField]
        public Sprite unlitSprite;

        [SerializeField]
        public float limit;
    }

    [Serializable]
    public class LampImageInfo 
    {
        [SerializeField]
        public Image image1;

        [SerializeField]
        public Image image2;
    }


    public float valueMin = 0.0f;
    [Range(0,11000)]
    public float valueMax = 360.0f;

    [Range(0, 11000)]
    public float shiftValue = 6900.0f;
    public float blinkTime = 0.05f;
    private float lastTime = 0;
    private bool act = true;

    [SerializeField]
    private LampInfo[] lamps;

    [SerializeField]
    private LampImageInfo[] lampImages;

    // Start is called before the first frame update
    void Start()
    {

    } /* Start */

    // Update is called once per frame
    void Update()
    {
        UpdateSubMeter();
        UpdateText();
        UpdateLams();
    } /* Update */

    void UpdateLams()
    {
        if (lamps.Length <= 0)
        {
            return;
        }
        if (lampImages.Length <= 0)
        {
            return;
        }

        Value = Mathf.Clamp(Value, valueMin, valueMax);
        if (Value >= shiftValue)
        {
            // 点滅
            if (lastTime + blinkTime < Time.time)
            {
                act = (act ? false : true);
                lastTime = Time.time;
            }
        }
        else {
            act = true;
        }

        float range = valueMax - valueMin;
        float step = range / lampImages.Length;

        float _value = 0;
        for (int ii = 0; ii < lampImages.Length; ii++)
        {
            var lampImage = lampImages[ii];
            var lamp = GetLampInfo(_value);
            if (act == true)
            {
                if (Value > _value) {
                    if (lampImage.image1 != null) { lampImage.image1.sprite = lamp.litSprite; }
                    if (lampImage.image2 != null) { lampImage.image2.sprite = lamp.litSprite; }
                }else {
                    if (lampImage.image1 != null) { lampImage.image1.sprite = lamp.unlitSprite; }
                    if (lampImage.image2 != null) { lampImage.image2.sprite = lamp.unlitSprite; }
                }
            }
            else {
                if (lampImage.image1 != null) { lampImage.image1.sprite = lamp.unlitSprite; }
                if (lampImage.image2 != null) { lampImage.image2.sprite = lamp.unlitSprite; }
            }
            _value += step;
        }

    } /* UpdateLams */

    public LampInfo GetLampInfo(float _value)
    {
        if (lamps.Length <= 0)
        {
            return null;
        }

        foreach (var lamp in lamps)
        {
            if (_value < lamp.limit)
            {
                return lamp;
            }
        }

        return lamps[lamps.Length-1];
    } /* GetLampInfo */


}
