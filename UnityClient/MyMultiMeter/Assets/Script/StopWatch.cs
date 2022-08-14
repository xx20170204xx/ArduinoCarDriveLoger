/* あああ */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StopWatch : MonoBehaviour
{

    [SerializeField]
    private Text m_text;

    [SerializeField]
    private Image m_image;

    [SerializeField]
    private Color m_normalColor = Color.white;

    [SerializeField]
    private Color m_runColor = Color.red;


    bool isRun = false;
    public bool isStop { get { return (isRun == false); } }

    private float movetime;

    void Awake()
    {
        if (m_text == null)
        {
            Debug.LogError("m_text is null.");
            this.gameObject.SetActive(false);
            return;
        }
        if (m_image == null)
        {
            Debug.LogError("m_image is null.");
            this.gameObject.SetActive(false);
            return;
        }
        m_image.color = m_normalColor;
        isRun = false;
    } /* Awake */

    void Update()
    {
        if (isStop == true)
        {
            return;
        }

        movetime += Time.deltaTime;
        m_text.text = TimeSet();

    } /* Update */

    public void OnRunSwitch()
    {
        if (isRun == false)
        {
            /* set start */
            movetime = 0;
            m_image.color = m_runColor;
            isRun = true;
        }
        else {
            /* stop */
            m_image.color = m_normalColor;
            isRun = false;
        }
    } /* OnRunSwitch */

    //時・分・秒の表示方法を指定
    string TimeSet()
    {
        //各種変数
        string timetext = null;
        int provtime = 0;
        int twodecimal = 0;

        //稼働時間から小数点以下を取り出して二桁の整数にする
        provtime = Mathf.FloorToInt(movetime);
        twodecimal = Mathf.FloorToInt((movetime - provtime) * 1000);

        //TimeSpanを使って　00：00：00.　の文字列を作る
        TimeSpan ts = new TimeSpan(0, 0, provtime);
        timetext = ts.ToString(@"hh\:mm\:ss\.");

        //切り離した小数点以下を文字化する
        timetext += twodecimal.ToString("D3");

        return timetext;
    }
}
