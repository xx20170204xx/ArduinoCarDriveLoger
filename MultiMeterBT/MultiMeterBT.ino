/*
https://github.com/xx20170204xx/ArduinoCarDriveLoger/blob/main/MultiMeterBT/MultiMeterBT.ino

参考URL
https://github.com/puriso/arduino-thermometer_by_defi_sensor/blob/master/thermomerter_by_defi.ino/thermomerter_by_defi.ino.ino
https://github.com/matt-downs/arduino-oled-auto-gauges/blob/master/boost/boost.ino

[ Temp ]
[Sensor]
 |    |
 |    +--+
 |    |  |
 |    R  |
 |    |  |
+5V  GND A1
         A2

R= 1Kohm

[  Press ]
[ Sensor ]
 |   |  |
 |   |  |
 |   |  |
+5V GND A3

[  Boost ]
[ Sensor ]
 |   |  |
 |   |  |
 |   |  |
+5V GND A4

                       +-- ||---GND
                       |
+5V----------|<--------+---|<---GND
                       |
SpeedPulse---1Kohm-----+--------D2 or D3
    or
TachoPulse

diode : --|<--
        1N4148

*/

#define DEBUG_TACHOSPEED 1
#define DEBUG_TMP_PRS 1
#define USE_LCD 0

const char* VERSION_STRING = "20220716XX";

#include "BluetoothSerial.h"
#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

#if !defined(CONFIG_BT_SPP_ENABLED)
#error Serial Bluetooth not available or not enabled. It is only available for the ESP32 chip.
#endif

BluetoothSerial SerialBT;
boolean confirmRequestPending = true;

#if USE_LCD == 1
// include the library code:
#include <LiquidCrystal.h>

/* LCD操作ボタン */
enum LCD_BUTTONS {
  LCD_BUTTON_NONE,
  LCD_BUTTON_UP,
  LCD_BUTTON_DOWN,
  LCD_BUTTON_LEFT,
  LCD_BUTTON_RIGHT,
  LCD_BUTTON_SELECT  
};
#endif

/* LCD Button 各抵抗値 */
const int LCD_BUTTON_VAR_NONE=1024;
const int LCD_BUTTON_VAR_SELECT=721;
const int LCD_BUTTON_VAR_LEFT=479;
const int LCD_BUTTON_VAR_DOWN=308;
const int LCD_BUTTON_VAR_UP=132;
const int LCD_BUTTON_VAR_RIGHT=0;

const char SEP_CHAR= '\t';

/* 更新間隔(ms) */
const int UPDATE_DELAY = 100;

/* アナログピンの値を取得する際の回数 */
const int SENSOR_AVG_COUNT = 100;

/* 各センサーのアナログピン番号 */
const int WATER_SENSOR_PIN = 1;
const int OIL_SENSOR_PIN = 2;
const int PRESSURE_SENSOR_PIN = 3;
const int BOOST_SENSOR_PIN = 4;
/* 回転数取得用ピン(デジタル/割り込み可能) */
const int TACHO_PULSE_PIN = 2;
/* 車速取得用ピン(デジタル/割り込み可能) */
const int SPEED_PULSE_PIN = 3;
/* 設定速度超過用ピン */
const int SPEED_WARNING_PIN=10;
/* 設定回転数超過用ピン */
const int RPM_WARNING_PIN=11;

/*--------------------------------------*/
const int R25C   = 10000; // R25℃ = Ω
const int B      = 3380;  // B定数
const float K    = 273.16; // ケルビン
const float C25  = K + 25; // 摂氏25度

/* 1回転当たりの発生パルス数 */
/* 4, 8, 16, 20, 25 */
const int SPEED_PULSE_COUNT = 4;

const float SPEED_WARNING_VALUE = 61.0f;
const float RPM_WARNING_VALUE = 3000.0f;

/*--------------------------------------*/

/* 油温 */
float g_OilTmp = 0;
/* 油圧 */
float g_OilPrs = 0;
/* 水温 */
float g_WaterTmp = 0;
/* ブースト圧 */
float g_BoostPrs = 0;

volatile unsigned long g_tachoBefore = 0;//クランクセンサーの前回の反応時の時間
volatile unsigned long g_tachoAfter = 0;//クランクセンサーの今回の反応時の時間
volatile unsigned long g_tachoWidth = 0;//クランク一回転の時間　tachoAfter - tachoBefore
volatile float g_tachoRpm = 0;//エンジンの回転数[rpm]

volatile unsigned long g_speedBefore = 0;
volatile unsigned long g_speedAfter = 0;
volatile unsigned long g_speedWidth = 0;
volatile float g_speedKm = 0;//車速[Km/h]


#if USE_LCD == 1
// initialize the library by associating any needed LCD interface pin
// with the arduino pin number it is connected to
const int rs = 8, en = 9, d4 = 4, d5 = 5, d6 = 6, d7 = 7;
LiquidCrystal lcd(rs, en, d4, d5, d6, d7);
/* 表示モード */
int g_LCDmode=0;
#endif

void BTConfirmRequestCallback(uint32_t numVal)
{
  Serial.println(numVal);
  confirmRequestPending = true;
} /* BTConfirmRequestCallback */

void BTAuthCompleteCallback(boolean success)
{
  confirmRequestPending = false;
  if (success)
  {
    Serial.println("Pairing success!!");
  }
  else
  {
    Serial.println("Pairing failed, rejected by user!!");
  }
} /* BTAuthCompleteCallback */

void BTconfirmRequest(void)
{
  Serial.println("BTconfirmRequest.");
  SerialBT.confirmReply(true);
} /* BTconfirmRequest */

void setup() {

  // 20220802 : PIN番号が使用できないためコメントアウト
  // pinMode(SPEED_WARNING_PIN, OUTPUT);
  // pinMode(RPM_WARNING_PIN, OUTPUT);

#if DEBUG_TACHOSPEED == 0
  /* Tacho */
  pinMode(TACHO_PULSE_PIN, INPUT_PULLUP);//ピンモードの設定
  attachInterrupt(digitalPinToInterrupt(TACHO_PULSE_PIN), InterruptTachoFunc, FALLING);//外部割り込み

  /* SPEED */
  pinMode(SPEED_PULSE_PIN, INPUT_PULLUP);//ピンモードの設定
  attachInterrupt(digitalPinToInterrupt(SPEED_PULSE_PIN), InterruptSpeedFunc, FALLING);//外部割り込み
#else
  /* nop */
#endif

  Serial.begin(115200);
  Serial.println("setup.");
  // SerialBT.enableSSP();
  // SerialBT.onConfirmRequest(BTConfirmRequestCallback);
  // SerialBT.onAuthComplete(BTAuthCompleteCallback);
  SerialBT.begin("MMM_BT"); //Bluetooth device name
  // Serial.println("The device started, now you can pair it with bluetooth!");
  confirmRequestPending = false;

#if USE_LCD == 1
  // set up the LCD's number of columns and rows:
  lcd.begin(16, 2);
  lcd.clear();
#endif
}

void loop() {
  if (confirmRequestPending == true)
  {
    BTconfirmRequest();
    delay(UPDATE_DELAY);
    return;
  }
  ReadSerialCommand();
#if DEBUG_TMP_PRS == 0
  UpdateSensorInfo();
#else
  UpdateDebugSensorInfo();
#endif
#if DEBUG_TACHOSPEED == 0
  UpdateTachoReset();
  UpdateSpeedReset();
#else
  UpdateDebugTachoSpeed();
#endif
  OutputSerial();
  OutputWarningPin();
#if USE_LCD == 1
  UpdateLCD();
#endif
  delay(UPDATE_DELAY);
} /* loop */

static void ReadSerialCommand()
{
  char cmd = SerialBT.read();
  switch(cmd)
  {
    /* Version */
    case 'V':
      {
        SerialBT.print('V');
        SerialBT.print(SEP_CHAR);
        SerialBT.println(VERSION_STRING);
      }
      break;
  }
} /* ReadSerialCommand */

static void OutputSerial( void ){
  char bufVars[6][10+1];
  char bufOut[256];
  memset( bufVars, 0x00, sizeof(bufVars) );
  memset( bufOut, 0x00, sizeof(bufOut) );

  // 浮動小数点を文字列に変換
  dtostrf(g_WaterTmp,    3,4, bufVars[0] );
  dtostrf(g_OilTmp,      3,4, bufVars[1] );
  dtostrf(g_OilPrs,      3,4, bufVars[2] );
  dtostrf(g_tachoRpm,    5,0, bufVars[3] );
  dtostrf(g_speedKm,     3,0, bufVars[4] );
  // dtostrf(g_BoostPrs,    3,4, bufVars[5] );

/*
  SerialBT.print('D');
  SerialBT.print(SEP_CHAR);
  for( int ii = 0; ii < 5; ii++ )
  {
    SerialBT.print(bufVars[ii]);
    SerialBT.print(SEP_CHAR);
  }*/
  sprintf( bufOut, "D%c%s%c%s%c%s%c%s%c%s", SEP_CHAR,
    bufVars[0],SEP_CHAR,
    bufVars[1],SEP_CHAR,
    bufVars[2],SEP_CHAR,
    bufVars[3],SEP_CHAR,
    bufVars[4]
    );
  SerialBT.println(bufOut);

} /* OutputSerial */

static void UpdateSensorInfo()
{
  float wtr_avg = 0;
  float oilT_avg = 0;
  float oilP_avg = 0;

  // センサーから各温度・圧力を取得
  g_WaterTmp = get_temp(WATER_SENSOR_PIN);
  g_OilTmp = get_temp(OIL_SENSOR_PIN);
  g_OilPrs = get_oil_pressure(PRESSURE_SENSOR_PIN);
  // g_BoostPrs = get_boost_press(BOOST_SENSOR_PIN);

} /* UpdateSensorInfo */

// 油圧取得(bar)
static float get_oil_pressure( int pinNum ){
  double input_for_value = analogReadAvg(pinNum, SENSOR_AVG_COUNT);

  float vo = (float)input_for_value * 5.0f / 1023.0f;
  float ret = 250 * (vo - 0.480) * 0.0101972;

  return ret;
} /* get_oil_pressure */

// 温度取得(摂氏)
static float get_temp( const int pinNum ){
  float out_tmp;
  float res;

  double input_for_value = analogReadAvg(pinNum, SENSOR_AVG_COUNT);

  res = resistance_by_input((int)input_for_value);
  out_tmp = convert_temp_by_ntc(res);
 
  return out_tmp;
} /* get_temp */

// 入力から抵抗値を求める(5V)
static float resistance_by_input(int input) {
  float vout = input / 1023.0f * 5.0f; //分圧した出力電圧の計算
  float r = ((5.0 / vout) - 1.0f) * R25C; //サーミスタ抵抗計算
  return r;
} /* resistance_by_input */

// NTCサーミスタでの温度
static float convert_temp_by_ntc(float r) {
  return B / (log(r/R25C) + (B/C25)) - K;
} /* convert_temp_by_ntc */

/* ブースト圧(bar) */
static float get_boost_press( const int pinNum )
{
  return 0.0f; /* TODO : DELETE */

 double input_for_value = analogReadAvg(pinNum, SENSOR_AVG_COUNT);

  float vo = (float)input_for_value * 5.0f / 1023.0f;
  float ret = (vo - 1.0f ) * 0.88;
  
} /* get_boost_press */

static void InterruptTachoFunc( void )
{
  const float ONE_MIN_USEC = 60.0f * 1000.0f * 1000.0f;
  g_tachoAfter = micros();//現在の時刻を記録
  g_tachoWidth = g_tachoAfter - g_tachoBefore;//前回と今回の時間の差を計算
  g_tachoBefore = g_tachoAfter;//今回の値を前回の値に代入する
  g_tachoRpm = ONE_MIN_USEC / (g_tachoWidth * 2.0f);//タイヤの回転数[rpm]を計算
} /* InterruptTachoFunc */

static void InterruptSpeedFunc( void )
{
  const float CSPD = 60.0 * 60 / (637 * SPEED_PULSE_COUNT) * 1000 * 1000;
  g_speedAfter = micros();//現在の時刻を記録
  g_speedWidth = g_speedAfter - g_speedBefore;//前回と今回の時間の差を計算
  g_speedBefore = g_speedAfter;//今回の値を前回の値に代入する
  g_speedKm = CSPD / g_speedWidth;
} /* InterruptSpeedFunc */

#if USE_LCD == 1
static void UpdateLCD()
{
  const unsigned int SEL_MAX = 6;
  int btn = getLCDButton(0);
  switch( btn )
  {
    case LCD_BUTTON_UP:
    case LCD_BUTTON_LEFT:
    g_LCDmode -= 1;
    break;
    case LCD_BUTTON_DOWN:
    case LCD_BUTTON_RIGHT:
    g_LCDmode += 1;
    break;
  }
  if( g_LCDmode < 0 ) g_LCDmode = SEL_MAX;
  if( g_LCDmode > SEL_MAX ) g_LCDmode = 0;

  lcd.clear();
  switch( g_LCDmode ){
    case 0:
      UpdateLCD_TachoSpeed();
      break;
    case 1:
      UpdateLCD_Tmps();
      break;
    case 2:
      UpdateLCD_WaterTemp();
      break;
    case 3:
      UpdateLCD_OilTemp();
      break;
    case 4:
      UpdateLCD_OilPress();
      break;
    case 5:
      UpdateLCD_Tacho();
      break;
    case 6:
      UpdateLCD_Speed();
      break;
  }
} /* UpdateLCD */

static void UpdateLCD_Tmps()
{
  char buf1[16+1];
  char buf2[16+1];

  char bufVars[3][10+1];

  memset( buf1, 0x00, sizeof(buf1) );
  memset( buf2, 0x00, sizeof(buf2) );
  memset( bufVars, 0x00, sizeof(bufVars) );

  // 浮動小数点を文字列に変換
  //  小数点以下なし
  dtostrf(g_OilTmp, 3,0, bufVars[0] );
  dtostrf(g_WaterTmp, 3,0, bufVars[1] );
  dtostrf(g_OilPrs, 3,4, bufVars[2] );

  snprintf( buf1, sizeof(buf1), "TMP O:%3.3s W:%3.3s", bufVars[0], bufVars[1] );
  snprintf( buf2, sizeof(buf2), "PRS O:%7.7sbar", bufVars[2] );

  lcd.setCursor(0,0);
  lcd.print(buf1);
  lcd.setCursor(0,1);
  lcd.print(buf2);
} /* UpdateLCD_Tmps */


static void UpdateLCD_WaterTemp()
{
  char buf1[16+1];
  char buf2[16+1];

  char bufVars[3][10+1];

  memset( buf1, 0x00, sizeof(buf1) );
  memset( buf2, 0x00, sizeof(buf2) );
  memset( bufVars, 0x00, sizeof(bufVars) );

  // 浮動小数点を文字列に変換
  //  小数点以下1
  dtostrf(g_WaterTmp, 3,1, bufVars[1] );

  snprintf( buf1, sizeof(buf1), "Water Temp" );
  snprintf( buf2, sizeof(buf2), "      %8.8s C", bufVars[1] );

  lcd.setCursor(0,0);
  lcd.print(buf1);
  lcd.setCursor(0,1);
  lcd.print(buf2);

} /* UpdateLCD_WaterTemp */

static void UpdateLCD_OilTemp()
{
  char buf1[16+1];
  char buf2[16+1];

  char bufVars[3][10+1];

  memset( buf1, 0x00, sizeof(buf1) );
  memset( buf2, 0x00, sizeof(buf2) );
  memset( bufVars, 0x00, sizeof(bufVars) );

  // 浮動小数点を文字列に変換
  //  小数点以下1
  dtostrf(g_OilTmp, 3,1, bufVars[0] );

  snprintf( buf1, sizeof(buf1), "Oil Temp" );
  snprintf( buf2, sizeof(buf2), "      %8.8s C", bufVars[0] );

  lcd.setCursor(0,0);
  lcd.print(buf1);
  lcd.setCursor(0,1);
  lcd.print(buf2);
} /* UpdateLCD_OilTemp */

static void UpdateLCD_OilPress(){
  char buf1[16+1];
  char buf2[16+1];

  char bufVars[3][10+1];

  memset( buf1, 0x00, sizeof(buf1) );
  memset( buf2, 0x00, sizeof(buf2) );
  memset( bufVars, 0x00, sizeof(bufVars) );

  // 浮動小数点を文字列に変換
  //  小数点以下4
  dtostrf(g_OilPrs, 3,4, bufVars[2] );

  snprintf( buf1, sizeof(buf1), "Oil Press" );
  snprintf( buf2, sizeof(buf2), "    %8.8s bar", bufVars[2] );

  lcd.setCursor(0,0);
  lcd.print(buf1);
  lcd.setCursor(0,1);
  lcd.print(buf2);
} /* UpdateLCD_OilPress */

static void UpdateLCD_TachoSpeed(){
  char buf1[16+1];
  char buf2[16+1];

  char bufVars[3][10+1];

  memset( buf1, 0x00, sizeof(buf1) );
  memset( buf2, 0x00, sizeof(buf2) );
  memset( bufVars, 0x00, sizeof(bufVars) );

  // 浮動小数点を文字列に変換
  dtostrf(g_tachoRpm, 4,0, bufVars[0] );
  dtostrf(g_speedKm,  3,0, bufVars[1] );

  snprintf( buf1, sizeof(buf1), "Tacho  :%4.4s rpm", bufVars[0] );
  snprintf( buf2, sizeof(buf2), "Speed  : %3.3s Km", bufVars[1] );

  lcd.setCursor(0,0);
  lcd.print(buf1);
  lcd.setCursor(0,1);
  lcd.print(buf2);
} /* UpdateLCD_TachoSpeed */

static void UpdateLCD_Tacho(){
  char buf1[16+1];
  char buf2[16+1];

  char bufVars[3][10+1];

  memset( buf1, 0x00, sizeof(buf1) );
  memset( buf2, 0x00, sizeof(buf2) );
  memset( bufVars, 0x00, sizeof(bufVars) );

  // 浮動小数点を文字列に変換
  dtostrf(g_tachoRpm, 4,0, bufVars[0] );
  dtostrf(g_tachoWidth, 16,0, bufVars[1] );

  snprintf( buf1, sizeof(buf1), "Tacho:%4.4s rpm", bufVars[0] );
  snprintf( buf2, sizeof(buf2), "%16.16s", bufVars[1] );

  lcd.setCursor(0,0);
  lcd.print(buf1);
  lcd.setCursor(0,1);
  lcd.print(buf2);
} /* UpdateLCD_Tacho */

static void UpdateLCD_Speed(){
  char buf1[16+1];
  char buf2[16+1];

  char bufVars[3][10+1];

  memset( buf1, 0x00, sizeof(buf1) );
  memset( buf2, 0x00, sizeof(buf2) );
  memset( bufVars, 0x00, sizeof(bufVars) );

  // 浮動小数点を文字列に変換
  dtostrf(g_speedKm,  3,0, bufVars[0] );
  dtostrf(g_speedWidth, 16,0, bufVars[1] );

  snprintf( buf1, sizeof(buf1), "Speed: %4.4s Km", bufVars[0] );
  snprintf( buf2, sizeof(buf2), "%16.16s", bufVars[1] );

  lcd.setCursor(0,0);
  lcd.print(buf1);
  lcd.setCursor(0,1);
  lcd.print(buf2);
} /* UpdateLCD_Speed */

#endif

/*
  パルスが入らない状態を確認して 0rpmを設定する
*/
static void UpdateTachoReset( void ){
  const float CSPD = 60.0 * 60 / (637 * SPEED_PULSE_COUNT) * 1000 * 1000;
  unsigned long width = micros() - g_tachoBefore;
  if( width <= CSPD )
    return;

  g_tachoWidth = 0.0f;
  g_tachoRpm = 0.0f;

} /* UpdateTachoReset */

/*
  パルスが入らない状態を確認して 0Kmを設定する
*/
static void UpdateSpeedReset( void ){
  const float CSPD = 60.0 * 60 / (637 * SPEED_PULSE_COUNT) * 1000 * 1000;
  unsigned long width = micros() - g_speedBefore;
  if( width <= CSPD )
    return;

  g_speedWidth = 0.0f;
  g_speedKm = 0.0f;

} /* UpdateSpeedReset */

static void OutputWarningPin(void)
{
  // 20220802 : PIN番号が使用できないためコメントアウト
  // digitalWrite(SPEED_WARNING_PIN, (g_speedKm >= SPEED_WARNING_VALUE));
  // digitalWrite(RPM_WARNING_PIN, (g_tachoRpm >= RPM_WARNING_VALUE));
  
} /* OutputWarningPin */

#if USE_LCD == 1
static int getLCDButton( int pinNum )
{
  int var = analogRead(pinNum);
  if( var <= LCD_BUTTON_VAR_RIGHT ) return LCD_BUTTON_RIGHT;
  if( var <= LCD_BUTTON_VAR_UP ) return LCD_BUTTON_UP;
  if( var <= LCD_BUTTON_VAR_DOWN ) return LCD_BUTTON_DOWN;
  if( var <= LCD_BUTTON_VAR_LEFT ) return LCD_BUTTON_LEFT;
  if( var <= LCD_BUTTON_VAR_SELECT ) return LCD_BUTTON_SELECT;

  return LCD_BUTTON_NONE;
} /* getLCDButton */
#endif

// 指定したアナログピンを指定回数取得した平均を返す
static double analogReadAvg( int pinNum , int count )
{
  double input_for_value = 0;
  for( int ii = 0; ii < count; ii++ ) { input_for_value += analogRead(pinNum); }
  input_for_value /= count;
  return input_for_value;
} /* analogReadAvg */


static void UpdateDebugTachoSpeed( void ){
#define RPM_MAX   8000
#define SPEED_MAX  300
    static int GEAR = 0;
    static float GEARS[] = { 
        0.0f,     // N
        183.068f, // 1st
        106.008f, // 2nd
        71.938f,  // 3rd
        52.716f,  // 4th
        43.163f   // 5th
    };
    g_tachoRpm += RPM_MAX * 0.1f * ( 100.0f / 1000.0f );
    g_tachoWidth = 1;

    g_speedKm = (GEARS[GEAR] != 0.0f ? g_tachoRpm / GEARS[GEAR] : 0.0f);
    
    if( g_tachoRpm  >= RPM_MAX )
    {
      g_tachoRpm = 750.0f;
      GEAR += 1;
    }
    if( GEAR > 5 )
    {
      GEAR = 0;
    }


} /* UpdateDebugTachoSpeed */

static void UpdateDebugSensorInfo( void )
{
/*
float g_OilTmp = 0;    // 油温
float g_OilPrs = 0;    // 油圧
float g_WaterTmp = 0;  // 水温
float g_BoostPrs = 0;  // ブースト圧
*/
    static float TMP_MIN = -20.0f;
    static float TMP_MAX = 150.0f;

    /* bar */
    static float PRS_MIN = -1.5f;
    static float PRS_MAX = 12.0f;

    g_OilTmp += TMP_MAX * 0.1f * ( 100.0f / 1000.0f );
    g_WaterTmp += TMP_MAX * 0.1f * ( 100.0f / 1000.0f );

    g_OilPrs += PRS_MAX * 0.1f * ( 100.0f / 1000.0f );



    if( g_OilTmp >= TMP_MAX ){ g_OilTmp = TMP_MIN; }
    if( g_WaterTmp >= TMP_MAX ){ g_WaterTmp = TMP_MIN; }

    if( g_OilPrs >= PRS_MAX ){ g_OilPrs = PRS_MIN; }


} /* UpdateDebugSensorInfo */


/* EOF */
