package jp.ne.sakura.jacobi.myserialservicelib;

import android.app.IntentService;
import android.content.Context;
import android.content.Intent;
import android.hardware.usb.UsbDeviceConnection;
import android.hardware.usb.UsbManager;
import android.os.IBinder;
import android.widget.Toast;

import com.hoho.android.usbserial.driver.UsbSerialDriver;
import com.hoho.android.usbserial.driver.UsbSerialPort;
import com.hoho.android.usbserial.driver.UsbSerialProber;

import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

public class MySerialService extends IntentService {
    public static final String ACTION_DEVID = "DEVID";
    public static final String ACTION_INTERVAL = "INTERVAL";

    /* USB Serial */
    private static UsbSerialPort port = null;
    private String buf = "";
    Timer timer = new Timer();

    public static int recvCount = 0;
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
    public static String filepath;
    public static PrintWriter pw_newdata = null;
    public static PrintWriter pw_data = null;

    public static  boolean isDeviceOpen()
    {
        if( port == null )
        {
            return false;
        }

        return port.isOpen();
    } /* isDeviceOpen */

    public MySerialService() {
        super("MySerialService");
    }

    @Override
    protected void onHandleIntent(Intent intent) {
    } /* onHandleIntent */

    @Override
    public void onCreate() {
        super.onCreate();
    } /* onCreate */

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        this.openDevice(intent);
        return START_STICKY;
    } /* onStartCommand */

    @Override
    public void onDestroy() {
        super.onDestroy();
    } /* onDestroy */

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    } /* onBind */

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
                if( port == null )
                {
                    return;
                }

                read();
            }
        },0, interval);

        { /* Debug用にファイル出力 */
            try {
                if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
                    LocalDateTime date = LocalDateTime.now();
                    filepath = getApplicationContext().getExternalFilesDir(null) + "/" + date.format(DateTimeFormatter.ofPattern("yyyyMMddHHmmss")) + "_data.txt";
                    pw_data = new PrintWriter(new FileWriter(filepath));
                    filepath = getApplicationContext().getExternalFilesDir(null) + "/" + date.format(DateTimeFormatter.ofPattern("yyyyMMddHHmmss")) + "_nd.txt";
                    pw_newdata = new PrintWriter(new FileWriter(filepath));
                }
            } catch (IOException ee) {
                ee.printStackTrace();
            }

        }

        recvCount = 0;
    } /* openDevice */

    public void closeDevice() {

        /* Debug用のファイルを閉じる */
        if( pw_newdata != null ){
            pw_newdata.close();
            pw_newdata = null;
        }
        if( pw_data != null ){
            pw_data.close();
            pw_data = null;
        }
        if( port == null ) {
            return;
        }
        if(port.isOpen()) {
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
            byte[] buffer = new byte[2000];
            int timeOut = 2000;

            if( port == null )
            {
                return;
            }

            if( port.isOpen() == false )
            {

                return;
            }

            int readSize = port.read(buffer, timeOut);
            updateReceivedData(buffer,readSize);
        }catch (Exception _E){

        }
    } /* read */

    private void updateReceivedData(byte[] data,int readSize) {
        String newbnf = new String(data);
        newbnf = newbnf.substring(0,readSize);
        buf = buf.concat(newbnf);
        String[] lines = buf.split("\n");

        /* デバック用出力 */
        if( pw_newdata != null )
        {
            pw_newdata.print(recvCount);
            pw_newdata.println("[" + newbnf + "]");
        }

        if( lines.length > 1 ) {
            /* デバック用出力 */
            if( pw_data != null )
            {
                pw_data.print(recvCount);
                pw_data.println("[" + lines[0] + "]");
            }

            recvCount = recvCount + 1;
            String[] strValues = lines[0].split("\t");
            buf = buf.substring(lines[0].length()+1);
            /*-------------------------------------------------------------------------------*/
            dataline = lines[0];
            waterTemp = Float.parseFloat(strValues[1]);
            oilTemp = Float.parseFloat(strValues[2]);
            oilPress = Float.parseFloat(strValues[3]);
            boostPress = Float.parseFloat(strValues[4]);
            rpm = Float.parseFloat(strValues[5]);
            speed = Float.parseFloat(strValues[6]);

            //if( strValues[0].startsWith("DD"))
            {
                roomTemp = Float.parseFloat(strValues[7]);
                acc_x = Float.parseFloat(strValues[8]);
                acc_y = Float.parseFloat(strValues[9]);
                acc_z = Float.parseFloat(strValues[10]);
                angle_x = Float.parseFloat(strValues[11]);
                angle_y = Float.parseFloat(strValues[12]);
                angle_z = Float.parseFloat(strValues[13]);
            }
        }

    } /* updateReceivedData */


}
