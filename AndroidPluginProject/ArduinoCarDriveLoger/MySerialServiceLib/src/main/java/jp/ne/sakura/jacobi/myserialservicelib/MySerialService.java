package jp.ne.sakura.jacobi.myserialservicelib;

import android.app.IntentService;
import android.app.Service;
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

import java.util.List;

public class MySerialService extends IntentService {
    public static final String ACTION_DEVID = "DEVID";
    public static final String C_ACTION_NEWDATA="ActionNewData";
    public static final String C_INTENT_DATALINE     = "IntentDataLine";
    public static final String C_INTENT_WATER_TEMP   = "IntentWaterTemp";
    public static final String C_INTENT_OIL_TEMP     = "IntentOilTemp";
    public static final String C_INTENT_OIL_PRESS    = "IntentOilPress";
    public static final String C_INTENT_BOOST_PRESS  = "IntentBoostPress";

    /* USB Serial */
    private static UsbSerialPort port = null;
    private String buf = "";

    private MyReceiver mReceiver;
    private IntentFilter mIntentFilter;

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
        this.openDevice();
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
            port.setParameters(115200, 8, UsbSerialPort.STOPBITS_1, UsbSerialPort.PARITY_NONE);
        }catch (Exception _e)
        {
            Toast.makeText(this, _e.getMessage(), Toast.LENGTH_LONG).show();
        }

        SerialInputOutputManager usbIoManager = new SerialInputOutputManager(port, new SerialInputOutputManager.Listener() {
            @Override
            public void onNewData(byte[] data) {
                updateReceivedData(data);

            }

            @Override
            public void onRunError(Exception e) {
                Toast.makeText(getBaseContext(),e.getMessage(), Toast.LENGTH_LONG).show();
            }
        });
        usbIoManager.start();
        Toast.makeText(this, "openDevice - Success.",Toast.LENGTH_LONG).show();

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

            // strOutput += "回転：" + _rpm + "rpm ";
            // strOutput += "速度：" + _speedKm + "Km ";
            // strOutput += "水温：" + _waterTmp + "℃ ";
            // strOutput += "油温：" + _oilTmp + "℃ ";
            // strOutput += "油圧：" + _oilPress + "Kpa ";

            Intent broadcastIntent = new Intent(MySerialService.C_ACTION_NEWDATA);
            broadcastIntent.putExtra(C_INTENT_DATALINE, strOutput);
            broadcastIntent.putExtra(C_INTENT_WATER_TEMP,  _waterTmp);
            broadcastIntent.putExtra(C_INTENT_OIL_TEMP,    _oilTmp);
            broadcastIntent.putExtra(C_INTENT_OIL_PRESS,   _oilPress);
            broadcastIntent.putExtra(C_INTENT_BOOST_PRESS, _boostPress);
            getBaseContext().sendBroadcast(broadcastIntent);
        }

    } /* updateReceivedData */


}
