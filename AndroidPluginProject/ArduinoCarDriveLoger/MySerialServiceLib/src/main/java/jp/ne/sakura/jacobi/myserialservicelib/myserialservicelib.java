package jp.ne.sakura.jacobi.myserialservicelib;

import android.app.Activity;
//import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
//import android.content.IntentFilter;
import android.os.Build;
import android.widget.Toast;

public class myserialservicelib {
    public static String AppPath;

    public static void Toast( Context context, String message )
    {
        //Toast.makeText(context, message,Toast.LENGTH_LONG).show();
        Toast.makeText(context, MySerialService.dataline,Toast.LENGTH_LONG).show();
    } /* Toast */

    public static Context getApplicationContext( Activity context )
    {
        if( context == null )
        {
            return null;
        }

        return context.getApplicationContext();
    } /* getApplicationContext */

    public static void setApplicationDirectory(String _AppPath)
    {
        if( _AppPath == null )
        {
            return;
        }
        AppPath = _AppPath;
    } /* setApplicationDirectory */

    public static void StartService(Context context, String devID , int interval)
    {
        if( context == null )
        {
            return;
        }
        Toast.makeText(context, "StartService - start",Toast.LENGTH_LONG).show();

        Intent intent = new Intent(context, MySerialService.class);
        intent.putExtra(MySerialService.ACTION_DEVID, devID);
        intent.putExtra(MySerialService.ACTION_INTERVAL, interval);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            context.startForegroundService(intent);
        }else{
            context.startService(intent);
        }
        Toast.makeText(context, "startForegroundService - end",Toast.LENGTH_LONG).show();
    } /* StartService */

    public static boolean GetIsDeviceOpen() { return MySerialService.isDeviceOpen(); } /* GetIsDeviceOpen */

    public static string GetDataLine()
    {
        return MySerialService.dataline;
    } /* GetDataLine */

    public static float GetWaterTmp()
    {
        return MySerialService.waterTemp;
    } /* GetWaterTmp */

    public static float GetOilTmp()
    {
        return MySerialService.oilTemp;
    } /* GetOilTmp */

    public static float GetOilPress()
    {
        return MySerialService.oilPress;
    } /* GetOilPress */

    public static float GetBoostPress()
    {
        return MySerialService.boostPress;
    } /* GetBoostPress */

    public static float GetRPM()
    {
        return MySerialService.rpm;
    } /* GetRPM */

    public static float GetSpeed()
    {
        return MySerialService.speed;
    } /* GetSpeed */

    public static float GetRoomTemp()
    {
        return MySerialService.roomTemp;
    } /* GetRoomTemp */

    public static float GetAccX() { return MySerialService.acc_x; } /* GetAccX */
    public static float GetAccY() { return MySerialService.acc_y; } /* GetAccY */
    public static float GetAccZ() { return MySerialService.acc_z; } /* GetAccZ */

    public static float GetAngleX() { return MySerialService.angle_x; } /* GetAngleX */
    public static float GetAngleY() { return MySerialService.angle_y; } /* GetAngleY */
    public static float GetAngleZ() { return MySerialService.angle_z; } /* GetAngleZ */

}/* class myserialservicelib */
