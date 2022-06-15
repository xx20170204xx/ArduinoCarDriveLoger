/*
https://github.com/puriso/arduino-thermometer_by_defi_sensor/blob/master/thermomerter_by_defi.ino/thermomerter_by_defi.ino.ino

[ Temp ]
[Sensor]
 |    |
 |    +--+
 |    |  |
 |    R  |
 |    |  |
+5V  GND A1
         A2

R= 1KΩ ？

[  Press ]
[ Sensor ]
 |   |  |
 |   |  |
 |   |  |
+5V GND A3



+5V----------|<--------+---|<---GND
                       |
SpeedPulse---1Kohm-----+--------D2 or D3
    or
TachoPulse

diode : --|<--
        1N4148

*/

#define DEBUG_TACHOSPEED 1
#define USE_LCD 1

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


/* 更新間隔(ms) */
const int UPDATE_DELAY = 100;

/*  平均値を求めるための履歴数 */
const int HISTORY_MAX = 25;

/* 各センサーのアナログピン番号 */
const int WATER_SENSOR_PIN = 1;
const int OIL_SENSOR_PIN = 2;
const int PRESSURE_SENSOR_PIN = 3;
/* 回転数取得用ピン(デジタル/割り込み可能) */
const int TACHO_PULSE_PIN = 2;
/* 車速取得用ピン(デジタル/割り込み可能) */
const int SPEED_PULSE_PIN = 3;

/*--------------------------------------*/
const int R25C   = 10000; // R25℃ = Ω
const int B      = 3380;  // B定数
const float K    = 273.16; // ケルビン
const float C25  = K + 25; // 摂氏25度
/*--------------------------------------*/

int g_HisSel = 0;
float g_WaterTmpHis[ HISTORY_MAX ];
float g_OilTmpHis[ HISTORY_MAX ];
float g_OilPrsHis[ HISTORY_MAX ];

bool g_first = false;

/* 油温 */
float g_OilTmp = 0;
/* 油圧 */
float g_OilPrs = 0;
/* 水温 */
float g_WaterTmp = 0;

/* 油温 */
float g_OilTmpAvg = 0;
/* 油圧 */
float g_OilPrsAvg = 0;
/* 水温 */
float g_WaterTmpAvg = 0;

volatile unsigned long tachoBefore = 0;//クランクセンサーの前回の反応時の時間
volatile unsigned long tachoAfter = 0;//クランクセンサーの今回の反応時の時間
volatile unsigned long tachoWidth = 0;//クランク一回転の時間　tachoAfter - tachoBefore
volatile float tachoRpm = 0;//エンジンの回転数[rpm]

volatile unsigned long speedBefore = 0;
volatile unsigned long speedAfter = 0;
volatile unsigned long speedWidth = 0;
volatile float speedKm = 0;//車速[Km/h]


#if USE_LCD == 1
// initialize the library by associating any needed LCD interface pin
// with the arduino pin number it is connected to
const int rs = 8, en = 9, d4 = 4, d5 = 5, d6 = 6, d7 = 7;
LiquidCrystal lcd(rs, en, d4, d5, d6, d7);
/* 表示モード */
int g_LCDmode=0;
#endif

void setup() {
  // global init
  memset( g_WaterTmpHis, 0x00, sizeof(g_WaterTmpHis) );
  memset( g_OilTmpHis, 0x00, sizeof(g_OilTmpHis) );
  memset( g_OilPrsHis, 0x00, sizeof(g_OilPrsHis) );

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

  Serial.begin(19200);

#if USE_LCD == 1
  // set up the LCD's number of columns and rows:
  lcd.begin(16, 2);
  lcd.clear();
#endif
}

void loop() {
  UpdateSensorInfo();
#if DEBUG_TACHOSPEED == 0
  UpdateTachoReset();
  UpdateSpeedReset();
#else
  UpdateDebugTachoSpeed();
#endif
  OutputSerial();
#if USE_LCD == 1
  UpdateLCD();
#endif
  delay(UPDATE_DELAY);
} /* loop */

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
  dtostrf(g_OilTmpAvg, 3,0, bufVars[0] );
  dtostrf(g_WaterTmpAvg, 3,0, bufVars[1] );
  dtostrf(g_OilPrsAvg, 3,4, bufVars[2] );

  snprintf( buf1, sizeof(buf1), "TMP O:%3.3s W:%3.3s", bufVars[0], bufVars[1] );
  snprintf( buf2, sizeof(buf2), "PRS O:%7.7sKpa", bufVars[2] );

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
  dtostrf(g_WaterTmpAvg, 3,1, bufVars[1] );

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
  dtostrf(g_OilTmpAvg, 3,1, bufVars[0] );

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
  dtostrf(g_OilPrsAvg, 3,4, bufVars[2] );

  snprintf( buf1, sizeof(buf1), "Oil Press" );
  snprintf( buf2, sizeof(buf2), "    %8.8s Kpa", bufVars[2] );

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
  dtostrf(tachoRpm, 4,0, bufVars[0] );
  dtostrf(speedKm,  3,0, bufVars[1] );

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
  dtostrf(tachoRpm, 4,0, bufVars[0] );
  dtostrf(tachoWidth, 16,0, bufVars[1] );

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
  dtostrf(speedKm,  3,0, bufVars[0] );
  dtostrf(speedWidth, 16,0, bufVars[1] );

  snprintf( buf1, sizeof(buf1), "Speed: %4.4s Km", bufVars[0] );
  snprintf( buf2, sizeof(buf2), "%16.16s", bufVars[1] );

  lcd.setCursor(0,0);
  lcd.print(buf1);
  lcd.setCursor(0,1);
  lcd.print(buf2);
} /* UpdateLCD_Speed */

#endif

static void UpdateSensorInfo()
{
  float wtr_avg = 0;
  float oilT_avg = 0;
  float oilP_avg = 0;

  // センサーから各温度・圧力を取得
  g_WaterTmp = get_temp(WATER_SENSOR_PIN);
  g_OilTmp = get_temp(OIL_SENSOR_PIN);
  g_OilPrs = get_oil_pressure(PRESSURE_SENSOR_PIN);

  // 平均値を取得
  g_WaterTmpHis[g_HisSel] = g_WaterTmp;
  g_OilTmpHis[g_HisSel] = g_OilTmp;
  g_OilPrsHis[g_HisSel] = g_OilPrs;

  /* 最初に取得した場合、履歴を最初に取得した値で埋めつくす */
  if( g_first == false )
  {
    g_first = true;
    for( int ii = 0; ii < HISTORY_MAX; ii++ )
    {
      g_WaterTmpHis[ii] = g_WaterTmp;
      g_OilTmpHis[ii] = g_OilTmp;
      g_OilPrsHis[ii] = g_OilPrs;
    }
  }

  /* 平均値を算出 */
  for( int ii = 0; ii < HISTORY_MAX; ii++ )
  {
    wtr_avg += g_WaterTmpHis[ii];
    oilT_avg += g_OilTmpHis[ii];
    oilP_avg += g_OilPrsHis[ii];
  }

  // 平均値を設定
  g_WaterTmpAvg = wtr_avg / (float)HISTORY_MAX;
  g_OilTmpAvg = oilT_avg / (float)HISTORY_MAX;
  g_OilPrsAvg = oilP_avg / (float)HISTORY_MAX;

  g_HisSel += 1;
  if( g_HisSel >= HISTORY_MAX )g_HisSel = 0;

} /* UpdateSensorInfo */

/*
  パルスが入らない状態を確認して 0Kmを設定する
*/
static void UpdateTachoReset( void ){
  const float ONE_MIN_USEC = 60.0f * 1000.0f * 1000.0f / 2.0f;
  unsigned long width = micros() - tachoBefore;
  if( width <= ONE_MIN_USEC )
    return;

  tachoWidth = 0.0f;
  tachoRpm = 0.0f;

} /* UpdateTachoReset */

/*
  パルスが入らない状態を確認して 0Kmを設定する
*/
static void UpdateSpeedReset( void ){
  const float CSPD = 60.0 * 60 / (637 * 4) * 1000 * 1000;
  unsigned long width = micros() - speedBefore;
  if( width <= CSPD )
    return;

  speedWidth = 0.0f;
  speedKm = 0.0f;

} /* UpdateSpeedReset */

static void OutputSerial( void ){
  char bufVars[8][10+1];
  memset( bufVars, 0x00, sizeof(bufVars) );

  // 浮動小数点を文字列に変換
  dtostrf(g_WaterTmp,    3,4, bufVars[0] );
  dtostrf(g_OilTmp,      3,4, bufVars[1] );
  dtostrf(g_OilPrs,      3,4, bufVars[2] );
  dtostrf(g_WaterTmpAvg, 3,4, bufVars[3] );
  dtostrf(g_OilTmpAvg,   3,4, bufVars[4] );
  dtostrf(g_OilPrsAvg,   3,4, bufVars[5] );
  dtostrf(tachoRpm,      5,0, bufVars[6] );
  dtostrf(speedKm,       3,0, bufVars[7] );

  // 水温 油温 油圧は平均のみ出力
  for( int ii = 0; ii < 8; ii++ )
  {
    Serial.print(bufVars[ii]);
    Serial.print('\t');
  }
  Serial.println("");

} /* OutputSerial */

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

// 油圧取得(Kpa)
static float get_oil_pressure( int pinNum ){
  int input_for_value = analogRead(pinNum);
  float vo = (float)input_for_value * 5.0f / 1023.0f;
  float ret = 250 * (vo - 0.480) * 0.0101972;

  return ret;
} /* get_oil_pressure */

// 温度取得(摂氏)
static float get_temp( const int pinNum ){
  float out_tmp;
  int input_for_temp;
  float res;

  input_for_temp = analogRead(pinNum);
  input_for_temp = input_for_temp;
  res = resistance_by_input(input_for_temp);
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

static void InterruptTachoFunc( void )
{
  const float ONE_MIN_USEC = 60.0f * 1000.0f * 1000.0f;
  tachoAfter = micros();//現在の時刻を記録
  tachoWidth = tachoAfter - tachoBefore;//前回と今回の時間の差を計算
  tachoBefore = tachoAfter;//今回の値を前回の値に代入する
  tachoRpm = ONE_MIN_USEC / (tachoWidth * 2.0f);//タイヤの回転数[rpm]を計算
} /* InterruptTachoFunc */

static void InterruptSpeedFunc( void )
{
  const float CSPD = 60.0 * 60 / (637 * 4) * 1000 * 1000;
  speedAfter = micros();//現在の時刻を記録
  speedWidth = speedAfter - speedBefore;//前回と今回の時間の差を計算
  speedBefore = speedAfter;//今回の値を前回の値に代入する
  speedKm = CSPD / speedWidth;
} /* InterruptSpeedFunc */

static void UpdateDebugTachoSpeed( void ){
#define RPM_MAX   8000
#define SPEED_MAX  300
    static int GEAR = 0;
    static float GEARS[] = {183.068f, 106.008f, 71.938f, 52.716f, 43.163f};
    tachoRpm += RPM_MAX * 0.1f * ( 100.0f / 1000.0f );
    tachoWidth = 1;

    speedKm = tachoRpm / GEARS[GEAR];
    
    if( tachoRpm  >= RPM_MAX )
    {
      tachoRpm = 750.0f;
      GEAR += 1;
    }
    if( GEAR > 4 )
    {
      GEAR = 0;
    }


} /* UpdateDebugTachoSpeed */


/* EOF */
