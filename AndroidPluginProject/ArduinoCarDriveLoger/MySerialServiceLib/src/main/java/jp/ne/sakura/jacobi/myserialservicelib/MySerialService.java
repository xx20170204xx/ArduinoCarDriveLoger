package jp.ne.sakura.jacobi.myserialservicelib;

import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.hardware.usb.UsbDeviceConnection;
import android.hardware.usb.UsbManager;
import android.os.IBinder;
import android.widget.Toast;

import com.hoho.android.usbserial.driver.UsbSerialDriver;
import com.hoho.android.usbserial.driver.UsbSerialPort;
import com.hoho.android.usbserial.driver.UsbSerialProber;
import com.hoho.android.usbserial.util.SerialInputOutputManager;

import java.util.List;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

public class MySerialService extends Service implements SerialInputOutputManager.Listener  {
    public static final String ACTION_DEVID = "DEVID";
    public static final String ACTION_WATER = "WaterTmp";
    public static final String ACTION_OIL_TEMP = "OilTemp";
    public static final String ACTION_OIL_PRESS = "OilPress";
    public static final String ACTION_BOOST = "Boost";
    public static final String ACTION_TACHO = "Tacho";
    public static final String ACTION_SPEED = "Speed";

    /* USB Serial */
    private UsbSerialPort port = null;
    private String buf = "";

    private float m_waterTmp;
    private float m_oilTmp;
    private float m_oilPress;
    private float m_boostPress;
    private float m_tacho;
    private float m_speedKm;

    public MySerialService() {
    }

    @Override
    public void onCreate() {
        super.onCreate();
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        String DevID = intent.getStringExtra(ACTION_DEVID);

        ScheduledExecutorService
                schedule = Executors.newSingleThreadScheduledExecutor();
        schedule.scheduleAtFixedRate(() -> {
            intent.putExtra(ACTION_WATER,m_waterTmp);
            intent.putExtra(ACTION_OIL_TEMP,m_oilTmp);
            intent.putExtra(ACTION_OIL_PRESS,m_oilPress);
            intent.putExtra(ACTION_BOOST,m_boostPress);
            intent.putExtra(ACTION_TACHO,m_tacho);
            intent.putExtra(ACTION_SPEED,m_speedKm);
            sendBroadcast(intent);

        },0,250,TimeUnit.MILLISECONDS);

        this.openDevice();

        return START_NOT_STICKY;
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        this.closeDevice();
    }

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    @Override
    public void onNewData(byte[] data) {
        buf = buf.concat(new String(data));
        String[] lines = buf.split("\n");
        if( lines.length > 2 )
        {
            String[] strValues = lines[1].split("\t");
            m_waterTmp = Float.parseFloat(strValues[3]);
            m_oilTmp = Float.parseFloat(strValues[4]);
            m_oilPress = Float.parseFloat(strValues[5]);
            m_boostPress  = Float.parseFloat(strValues[6]);
            m_tacho = Float.parseFloat(strValues[7]);
            m_speedKm = Float.parseFloat(strValues[8]);
/*
            String strOutput = "";
            buf="";
            strOutput += "回転：" + m_tacho + "rpm ";
            strOutput += "速度：" + m_speedKm + "Km ";
            strOutput += "水温：" + m_waterTmp + "℃ ";
            strOutput += "油温：" + m_oilTmp + "℃ ";
            strOutput += "油圧：" + m_oilPress + "bar ";
            strOutput += "ブースト圧：" + m_boostPress + "Kpa ";*/


        }else{
        }
    } /* onNewData */

    @Override
    public void onRunError(Exception e) {
        // Toast.makeText(this, e.getMessage(),Toast.LENGTH_LONG).show();

    } /* onRunError */


    public void openDevice(){

        if( port != null ) {
            /* 接続済み */
            return;
        }

        UsbManager manager = (UsbManager) getSystemService(Context.USB_SERVICE);
        List<UsbSerialDriver> availableDrivers = UsbSerialProber.getDefaultProber().findAllDrivers(manager);
        if (availableDrivers.isEmpty()) {
            /* 接続できるデバイスなし */
            Toast.makeText(this, "No Device",Toast.LENGTH_LONG).show();
            return;
        }

        // Open a connection to the first available driver.
        UsbSerialDriver driver = availableDrivers.get(0);
        UsbDeviceConnection connection = manager.openDevice(driver.getDevice());
        if (connection == null) {
            /* 接続失敗 */
            Toast.makeText(this, "Connection Error.",Toast.LENGTH_LONG).show();
            return;
        }

        port = driver.getPorts().get(0); // Most devices have just one port (port 0)
        buf = "";
        try {
            port.open(connection);
            port.setParameters(19200, 8, UsbSerialPort.STOPBITS_1, UsbSerialPort.PARITY_NONE);
        }catch (Exception _e)
        {
            Toast.makeText(this, _e.getMessage(), Toast.LENGTH_LONG).show();
        }

        SerialInputOutputManager usbIoManager = new SerialInputOutputManager(port, this);
        usbIoManager.start();

    } /* openDevice */

    public void closeDevice() {

        if( port == null ) {
            return;
        }
        if( port.isOpen() == true) {
            try{
                port.close();
                port = null;
            }catch (Exception e)
            {
                Toast.makeText(this,e.getMessage(), Toast.LENGTH_LONG).show();
            }
        }
    } /* closeDevice */


}
