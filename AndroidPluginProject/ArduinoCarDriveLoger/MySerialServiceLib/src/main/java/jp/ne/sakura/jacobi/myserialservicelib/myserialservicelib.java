package jp.ne.sakura.jacobi.myserialservicelib;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Build;
import android.widget.Toast;

public class myserialservicelib {
    public static String AppPath;

    public static void Toast( Context context, String message )
    {
        Toast.makeText(context, message,Toast.LENGTH_LONG).show();
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

    public static void StartService(Context context, String devID )
    {
        if( context == null )
        {
            return;
        }
        Toast.makeText(context, "StartService - start",Toast.LENGTH_LONG).show();

        Intent intent = new Intent(context, MySerialService.class);
        // intent.putExtra(MySerialService.ACTION_DEVID, devID);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            context.startForegroundService(intent);
        }else{
            context.startService(intent);
        }
        Toast.makeText(context, "startForegroundService - end",Toast.LENGTH_LONG).show();
    } /* StartService */

    public static String GetDataLine()
    {
        return MyReceiver.dataline;
    } /* GetDataLine */

    public static float GetWaterTmp()
    {
        return MyReceiver.waterTemp;
    } /* GetWaterTmp */

    public static float GetOilTmp()
    {
        return MyReceiver.oilTemp;
    } /* GetOilTmp */

    public static float GetOilPress()
    {
        return MyReceiver.oilPress;
    } /* GetOilPress */

    public static float GetBoostPress()
    {
        return MyReceiver.boostPress;
    } /* GetOilPress */

}/* class myserialservicelib */
