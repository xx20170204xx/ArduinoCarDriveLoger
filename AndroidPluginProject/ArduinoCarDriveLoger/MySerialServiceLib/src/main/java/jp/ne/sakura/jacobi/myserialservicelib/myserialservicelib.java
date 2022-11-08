package jp.ne.sakura.jacobi.myserialservicelib;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;

public class myserialservicelib {

    static private MyRcv m_receiver = null;

    public static void StartService( Context context, String devID )
    {
        if( context != null )
        {
            return;
        }

        if( m_receiver != null )
        {
            return;
        }

        m_receiver = new MyRcv();
        IntentFilter intentFilter = new IntentFilter();
        intentFilter.addAction(MySerialService.ACTION_WATER);
        context.registerReceiver(m_receiver, intentFilter);

        Intent intent = new Intent(context.getApplicationContext(), MySerialService.class);
        intent.putExtra(MySerialService.ACTION_DEVID, devID);
        context.startService(intent);
    } /* OnStartService */

    public float GetWaterTmp()
    {
        if( m_receiver != null )
        {
            return m_receiver.m_waterTmp;
        }
        return 0.0f;
    } /* GetWaterTmp */

    public static class MyRcv extends BroadcastReceiver
    {
        public float m_waterTmp;
        public float m_oilTmp;
        public float m_oilPress;
        public float m_boostPress;
        public float m_tacho;
        public float m_speedKm;

        @Override
        public void onReceive(Context context, Intent intent)
        {
            m_waterTmp = intent.getFloatExtra(MySerialService.ACTION_WATER,0.0f);
            m_oilTmp = intent.getFloatExtra(MySerialService.ACTION_OIL_TEMP,0.0f);
            m_oilPress = intent.getFloatExtra(MySerialService.ACTION_OIL_PRESS,0.0f);
            m_boostPress = intent.getFloatExtra(MySerialService.ACTION_BOOST,0.0f);
            m_tacho = intent.getFloatExtra(MySerialService.ACTION_TACHO,0.0f);
            m_speedKm = intent.getFloatExtra(MySerialService.ACTION_SPEED,0.0f);
        } /* onReceive */
    } /* class MyRcv */
}/* class myserialservicelib */
