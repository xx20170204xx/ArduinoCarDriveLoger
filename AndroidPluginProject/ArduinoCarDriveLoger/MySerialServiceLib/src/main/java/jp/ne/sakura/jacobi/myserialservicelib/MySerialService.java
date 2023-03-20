package jp.ne.sakura.jacobi.myserialservicelib;

import android.app.IntentService;
import android.app.Service;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.hardware.usb.UsbDeviceConnection;
import android.hardware.usb.UsbManager;
import android.os.IBinder;
import android.widget.Toast;

import com.hoho.android.usbserial.driver.UsbSerialDriver;
import com.hoho.android.usbserial.driver.UsbSerialPort;
import com.hoho.android.usbserial.driver.UsbSerialProber;
import com.hoho.android.usbserial.util.SerialInputOutputManager;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

public class MySerialService extends IntentService {
    public static final String ACTION_DEVID = "DEVID";
    public static final String ACTION_INTERVAL = "INTERVAL";
    public static final String C_ACTION_NEWDATA="ActionNewData";
    public static final String C_INTENT_DATALINE     = "IntentDataLine";
    public static final String C_INTENT_WATER_TEMP   = "IntentWaterTemp";
    public static final String C_INTENT_OIL_TEMP     = "IntentOilTemp";
    public static final String C_INTENT_OIL_PRESS    = "IntentOilPress";
    public static final String C_INTENT_BOOST_PRESS  = "IntentBoostPress";
    public static final String C_INTENT_RPM  = "IntentRpm";
    public static final String C_INTENT_SPEED  = "IntentSpeed";
    public static final String C_INTENT_ROOM_TEMP  = "IntentRoomTemp";
    public static final String C_INTENT_ACC_X  = "IntentAccX";
    public static final String C_INTENT_ACC_Y  = "IntentAccY";
    public static final String C_INTENT_ACC_Z  = "IntentAccZ";
    public static final String C_INTENT_ANGLE_X  = "IntentAngleX";
    public static final String C_INTENT_ANGLE_Y  = "IntentAngleY";
    public static final String C_INTENT_ANGLE_Z  = "IntentAngleZ";

    /* USB Serial */
    private static UsbSerialPort port = null;
    private String buf = "";
    Timer timer = new Timer();

    private MyReceiver mReceiver;
    private IntentFilter mIntentFilter;

    public static String dataline;
    public static float waterTemp = 0.0f;
    public static float oilTemp = 0.0f;
    public static float oilPress = 0.0f;
    public static float boostPress = 0.0f;
    public static float rpm = 0.0f;
    public static float speed = 0.0f;
    public static float roomTemp = 0.0f;
    public static float acc_x = 0.0f;
    public static float acc_y = 0.0f;
    public static float acc_z = 0.0f;
    public static float angle_x = 0.0f;
    public static float angle_y = 0.0f;
    public static float angle_z = 0.0f;

    public class MyReceiver extends BroadcastReceiver {


        @Override
        public void onReceive(Context context, Intent intent) {
            dataline = intent.getStringExtra(MySerialService.C_INTENT_DATALINE);
            waterTemp = intent.getFloatExtra(MySerialService.C_INTENT_WATER_TEMP, 0.0f);
            oilTemp = intent.getFloatExtra(MySerialService.C_INTENT_OIL_TEMP, 0.0f);
            oilPress = intent.getFloatExtra(MySerialService.C_INTENT_OIL_PRESS, 0.0f);
            boostPress = intent.getFloatExtra(MySerialService.C_INTENT_BOOST_PRESS, 0.0f);
            rpm = intent.getFloatExtra(MySerialService.C_INTENT_RPM, 0.0f);
            speed = intent.getFloatExtra(MySerialService.C_INTENT_SPEED, 0.0f);
            roomTemp = intent.getFloatExtra(MySerialService.C_INTENT_ROOM_TEMP, 0.0f);
            acc_x = intent.getFloatExtra(MySerialService.C_INTENT_ACC_X, 0.0f);
            acc_y = intent.getFloatExtra(MySerialService.C_INTENT_ACC_Y, 0.0f);
            acc_z = intent.getFloatExtra(MySerialService.C_INTENT_ACC_Z, 0.0f);
            angle_x = intent.getFloatExtra(MySerialService.C_INTENT_ANGLE_X, 0.0f);
            angle_y = intent.getFloatExtra(MySerialService.C_INTENT_ANGLE_Y, 0.0f);
            angle_z = intent.getFloatExtra(MySerialService.C_INTENT_ANGLE_Z, 0.0f);
        } /* onReceive */
    }

    public MySerialService() {
        super("MySerialService");
    }

    @Override
    protected void onHandleIntent(Intent intent) {
    } /* onHandleIntent */

    @Override
    public void onCreate() {
        super.onCreate();
        registerScreenReceiver();
    } /* onCreate */

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        this.openDevice(intent);
        return START_STICKY;
    } /* onStartCommand */

    @Override
    public void onDestroy() {
        super.onDestroy();
        unregisterReceiver(mReceiver);
    } /* onDestroy */

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    } /* onBind */

    // receiverを登録
    private void registerScreenReceiver() {
        mReceiver = new MyReceiver();
        mIntentFilter = new IntentFilter();
        mIntentFilter.addAction(MySerialService.C_ACTION_NEWDATA);
        registerReceiver(mReceiver, mIntentFilter);
    } /* registerScreenReceiver */

    public void openDevice(Intent intent) {
        int interval = 250;

        if( port != null ) {
            /* 接続済み */
            return;
        }

        interval = intent.getIntExtra(ACTION_INTERVAL,interval);

        UsbManager manager = (UsbManager) getSystemService(Context.USB_SERVICE);
        List<UsbSerialDriver> availableDrivers = UsbSerialProber.getDefaultProber().findAllDrivers(manager);
        if (availableDrivers.isEmpty()) {
            /* 接続できるデバイスなし */
            Toast.makeText(getBaseContext(), "No Device", Toast.LENGTH_LONG).show();
            return;
        }

        // Open a connection to the first available driver.
        UsbSerialDriver driver = availableDrivers.get(0);
        UsbDeviceConnection connection = manager.openDevice(driver.getDevice());
        if (connection == null) {
            /* 接続失敗 */
            Toast.makeText(getBaseContext(), "Connection Error.", Toast.LENGTH_LONG).show();
            return;
        }

        port = driver.getPorts().get(0); // Most devices have just one port (port 0)
        buf = "";
        try {
            port.open(connection);
            port.setParameters(115200, 8, UsbSerialPort.STOPBITS_1, UsbSerialPort.PARITY_NONE);
        } catch (Exception _e) {
            Toast.makeText(getBaseContext(), _e.getMessage(), Toast.LENGTH_LONG).show();
            return;
        }

        timer.scheduleAtFixedRate(new TimerTask() {
            @Override
            public void run() {
                read();
            }
        },0, interval);
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

    public void read()
    {
        try{
            byte[] buffer = new byte[8192];
            int len = port.read(buffer, 2000);
            updateReceivedData(buffer);
        }catch (Exception _E){

        }
    } /* read */

    private void updateReceivedData(byte[] data) {
        buf = buf.concat(new String(data));
        String[] lines = buf.split("\n");
        if( lines.length > 2 ) {
            String[] strValues = lines[1].split("\t");
            String strOutput = lines[1];
            buf = "";
            float _waterTmp = Float.parseFloat(strValues[1]);
            float _oilTmp = Float.parseFloat(strValues[2]);
            float _oilPress = Float.parseFloat(strValues[3]);
            float _boostPress = Float.parseFloat(strValues[4]);
            float _rpm = Float.parseFloat(strValues[5]);
            float _speedKm = Float.parseFloat(strValues[6]);

            Intent broadcastIntent = new Intent(MySerialService.C_ACTION_NEWDATA);
            broadcastIntent.putExtra(C_INTENT_DATALINE, strOutput);
            broadcastIntent.putExtra(C_INTENT_WATER_TEMP,  _waterTmp);
            broadcastIntent.putExtra(C_INTENT_OIL_TEMP,    _oilTmp);
            broadcastIntent.putExtra(C_INTENT_OIL_PRESS,   _oilPress);
            broadcastIntent.putExtra(C_INTENT_BOOST_PRESS, _boostPress);
            broadcastIntent.putExtra(C_INTENT_RPM, _rpm);
            broadcastIntent.putExtra(C_INTENT_SPEED, _speedKm);
            if( strValues[0] == "DD" )
            {
                float _mpu6050_temp = Float.parseFloat(strValues[7]);
                float _mpu6050_acc_x = Float.parseFloat(strValues[8]);
                float _mpu6050_acc_y = Float.parseFloat(strValues[9]);
                float _mpu6050_acc_z = Float.parseFloat(strValues[10]);
                float _mpu6050_angle_x = Float.parseFloat(strValues[11]);
                float _mpu6050_angle_y = Float.parseFloat(strValues[12]);
                float _mpu6050_angle_z = Float.parseFloat(strValues[13]);
                broadcastIntent.putExtra(C_INTENT_SPEED, _mpu6050_temp);
                broadcastIntent.putExtra(C_INTENT_ACC_X, _mpu6050_acc_x);
                broadcastIntent.putExtra(C_INTENT_ACC_Y, _mpu6050_acc_y);
                broadcastIntent.putExtra(C_INTENT_ACC_Z, _mpu6050_acc_z);
                broadcastIntent.putExtra(C_INTENT_ANGLE_X, _mpu6050_angle_x);
                broadcastIntent.putExtra(C_INTENT_ANGLE_Y, _mpu6050_angle_y);
                broadcastIntent.putExtra(C_INTENT_ANGLE_Z, _mpu6050_angle_z);
            }
            getBaseContext().sendBroadcast(broadcastIntent);
        }

    } /* updateReceivedData */


}
