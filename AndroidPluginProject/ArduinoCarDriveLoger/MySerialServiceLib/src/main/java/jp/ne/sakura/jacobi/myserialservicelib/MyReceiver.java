package jp.ne.sakura.jacobi.myserialservicelib;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;
import android.widget.Toast;

public class MyReceiver extends BroadcastReceiver {

    public static String dataline;
    public static float waterTemp = 0.0f;
    public static float oilTemp = 0.0f;
    public static float oilPress = 0.0f;
    public static float boostPress = 0.0f;

    @Override
    public void onReceive(Context context, Intent intent) {
        dataline = intent.getStringExtra(MySerialService.C_INTENT_DATALINE);
        waterTemp = intent.getFloatExtra(MySerialService.C_INTENT_WATER_TEMP, 0.0f);
        oilTemp = intent.getFloatExtra(MySerialService.C_INTENT_OIL_TEMP, 0.0f);
        oilPress = intent.getFloatExtra(MySerialService.C_INTENT_OIL_PRESS, 0.0f);
        boostPress = intent.getFloatExtra(MySerialService.C_INTENT_BOOST_PRESS, 0.0f);
    } /* onReceive */
}
