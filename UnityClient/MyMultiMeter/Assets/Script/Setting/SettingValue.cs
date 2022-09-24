using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingValue : MonoBehaviour
{
    [SerializeField]
    private Slider m_Value;

    [SerializeField]
    private Text m_Text;

    public float m_stepAmount = 1;
    public float stepAmount { get { return m_stepAmount; } set { m_stepAmount = value; UpdateNumOfStep(); } }
    private int numberOfSteps = 0;

    public float Value { get { return m_Value.value; } set { m_Value.value = value; } }

    public float minValue { get { return m_Value.minValue; } set { m_Value.minValue = value; UpdateNumOfStep(); } }
    public float maxValue { get { return m_Value.maxValue; } set { m_Value.maxValue = value; UpdateNumOfStep(); } }

    private void UpdateNumOfStep()
    {
        numberOfSteps = (int)((m_Value.maxValue - m_Value.minValue) / stepAmount);
    } /* UpdateNumOfStep */

    void Awake()
    {
        if (m_Value == null)
        {
            this.gameObject.SetActive(false);
            return;
        }
        if (m_Text == null)
        {
            this.gameObject.SetActive(false);
            return;
        }

        UpdateNumOfStep();
    } /* Awake */

    void Start()
    {
    } /* Start */

    void Update()
    {
        m_Text.text = Value.ToString();

    } /* Update */

    public void OnChangeSlider()
    {
        float range = (m_Value.value / (m_Value.maxValue - m_Value.minValue)) * numberOfSteps;
        int ceil = Mathf.CeilToInt(range);
        m_Value.value = ceil * stepAmount;

    } /* OnChangeSlider */
} /* class */
