using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingBoolean : MonoBehaviour
{
    [SerializeField]
    private Toggle m_Value;

    [SerializeField]
    private Text m_Text;

    public float m_stepAmount = 1;
    public float stepAmount { get { return m_stepAmount; } set { m_stepAmount = value; } }

    public bool Value { get { return m_Value.isOn; } set { m_Value.isOn = value; } }


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

    } /* Awake */

    void Start()
    {
    } /* Start */

    void Update()
    {
        m_Text.text = Value.ToString();

    } /* Update */

} /* class */
