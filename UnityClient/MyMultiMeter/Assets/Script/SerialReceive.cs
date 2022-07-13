using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SerialPortUtility;

public class SerialReceive : MonoBehaviour
{
    public SerialPortUtilityPro serialHandler;

    public MeterUnitController controller = null;

    public void OpenDevice()
    {
        if (serialHandler.IsConnected() == false) 
        {
            serialHandler.Open();
        }
    } /* OpenDevice */

    //受信した信号(message)に対する処理
    public void OnDataReceived( object _data)
    {
        try
        {
            var message = _data as string;
            var data = message.Split(
                    new string[] { "\n" }, System.StringSplitOptions.None);
            controller.OnDataReceived(data[0]);

        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);//エラーを表示
        }
    } /* OnDataReceived */

    public void OnSerialEvent(SerialPortUtilityPro _serialPort,string _data)
    {
        Debug.Log("OnSerialEvent : " + _data);


    } /* OnSerialEvent */
}