using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SerialReceive))]
public class PanelSelect : MonoBehaviour
{
    [SerializeField]
    private List<MeterUnitController> units = new List<MeterUnitController>();

    [SerializeField]
    private MeterUnitController deviceList = null;

    [SerializeField]
    [Min(0)]
    private int startNum = 0;

    private int nowNum;
    SerialReceive serial;
    private void Start()
    {
        serial = this.gameObject.GetComponent<SerialReceive>();
        if (serial == null)
        {
            Debug.LogError("Not found  SerialReceive.");
            this.gameObject.SetActive(false);
            return;
        }
        if (units.Count == 0)
        {
            Debug.LogError("Not found  MeterUnitController.");
            this.gameObject.SetActive(false);
            return;
        }

        units.Add(deviceList);

        for (int ii = 0; ii < units.Count; ii++)
        {
            GameObject _go = units[ii].gameObject;
            _go.SetActive(false);
            _go.transform.localPosition = new Vector3(0, 0, 0);
        }

        nowNum = startNum;

        if (nowNum < units.Count)
        {
            units[nowNum].gameObject.SetActive(true);
        }
        else {
            nowNum = 0;
            units[nowNum].gameObject.SetActive(true);
        }

        serial.controller = units[nowNum];


    } /* Start */

    public void OnNextPanel()
    {
        if (units.Count < 1)
        {
            return;
        }

        units[nowNum].gameObject.SetActive(false);

        nowNum += 1;
        if (units.Count == nowNum)
        {
            nowNum = 0;
        }

        serial.controller = units[nowNum];
        serial.controller.resetValue();
        serial.controller.gameObject.SetActive(true);
    } /* OnNextPanel */
    public void OnPrevPanel()
    {
        if (units.Count < 1)
        {
            return;
        }

        units[nowNum].gameObject.SetActive(false);
        if (nowNum == 0)
        {
            nowNum = units.Count - 1;
        } else {
            nowNum -= 1;
        }
        serial.controller = units[nowNum];
        serial.controller.resetValue();
        serial.controller.gameObject.SetActive(true);
    } /* OnPrevPanel */


}
