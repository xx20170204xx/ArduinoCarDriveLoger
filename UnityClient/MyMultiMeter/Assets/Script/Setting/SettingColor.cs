using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingColor : MonoBehaviour
{
    [SerializeField]
    private Slider m_sliderH = null;
    [SerializeField]
    private Slider m_sliderS = null;
    [SerializeField]
    private Slider m_sliderV = null;

    [SerializeField]
    private Image m_image = null;

    public Color Color { get {  return m_image.color; } }

    void Awake()
    {
        if (m_sliderH == null)
        {
            this.gameObject.SetActive(false);
            return;
        }
        if (m_sliderS == null)
        {
            this.gameObject.SetActive(false);
            return;
        }
        if (m_sliderV == null)
        {
            this.gameObject.SetActive(false);
            return;
        }
        if (m_image == null)
        {
            this.gameObject.SetActive(false);
            return;
        }

        m_sliderH.minValue = 0;
        m_sliderH.maxValue = 360;

        m_sliderS.minValue = 0;
        m_sliderS.maxValue = 100;

        m_sliderV.minValue = 0;
        m_sliderV.maxValue = 100;

    } /* Awake */

    private void Start()
    {
        SetColor(m_image.color);
    } /* Start */

    // Update is called once per frame
    void Update()
    {
        
        float _H = m_sliderH.value;
        float _S = m_sliderS.value;
        float _V = m_sliderV.value;

        m_image.color = Color.HSVToRGB(_H / 360.0f, _S / 100.0f, _V / 100.0f);
    } /* Update */

    public void SetColor(Color _color)
    {
        float _H, _S, _V;
        Color.RGBToHSV(_color, out _H, out _S, out _V);
        m_sliderH.value = _H * 360;
        m_sliderS.value = _S * 100;
        m_sliderV.value = _V * 100;
    } /* SetColor */

} /* class */
