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

    //��M�����M��(message)�ɑ΂��鏈��
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
            Debug.LogWarning(e.Message);//�G���[��\��
        }
    } /* OnDataReceived */

    public void OnSerialEvent(SerialPortUtilityPro _serialPort,string _data)
    {
        Debug.Log("OnSerialEvent : " + _data);


    } /* OnSerialEvent */
}