package jp.ne.sakura.jacobi.myserialservicelib;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Build;
import android.widget.Toast;

public class myserialservicelib {

    public static void Toast( Activity context, String message )
    {
        Toast.makeText(context, message,Toast.LENGTH_LONG).show();
    } /* Toast */

    public static void StartService(Context context, String devID )
    {
        if( context == null )
        {
            return;
        }
        Toast.makeText(context, "StartService - start",Toast.LENGTH_LONG).show();

        Intent intent = new Intent(context.getApplicationContext(), MySerialService.class);
        // intent.putExtra(MySerialService.ACTION_DEVID, devID);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            context.startForegroundService(intent);
        }else{
            context.startService(intent);
        }
        Toast.makeText(context, "startForegroundService - end",Toast.LENGTH_LONG).show();
    } /* OnStartService */

    public static float GetWaterTmp()
    {
        return MyReceiver.waterTemp;
    } /* GetWaterTmp */

}/* class myserialservicelib */
