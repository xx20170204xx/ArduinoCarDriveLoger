/*
https://github.com/xx20170204xx/ArduinoCarDriveLoger/blob/main/MultiMeter/MultiMeter.ino

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

#define DEBUG_TACHOSPEED 0
#define DEBUG_TMP_PRS 0

const char* VERSION_STRING = "20221127XX";

#include <Wire.h>

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
const float SPEED_MAX = 400.0f;
const float RPM_MAX = 11000.0f;

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

// レジスタアドレス
#define MPU6050_ACCEL_XOUT_H 0x3B  // R  
#define MPU6050_WHO_AM_I     0x75  // R
#define MPU6050_PWR_MGMT_1   0x6B  // R/W
#define MPU6050_I2C_ADDRESS  0x68
// 構造体定義
typedef union accel_t_gyro_union {
  struct {
    uint8_t x_accel_h;
    uint8_t x_accel_l;
    uint8_t y_accel_h;
    uint8_t y_accel_l;
    uint8_t z_accel_h;
    uint8_t z_accel_l;
    uint8_t t_h;
    uint8_t t_l;
    uint8_t x_gyro_h;
    uint8_t x_gyro_l;
    uint8_t y_gyro_h;
    uint8_t y_gyro_l;
    uint8_t z_gyro_h;
    uint8_t z_gyro_l;
  } reg;
  struct {
    int16_t x_accel;
    int16_t y_accel;
    int16_t z_accel;
    int16_t temperature;
    int16_t x_gyro;
    int16_t y_gyro;
    int16_t z_gyro;
  } value;
};

typedef struct {
  float x;
  float y;
  float z;
} Vector3;

bool    g_mpu6050_init = false;
Vector3 g_acc;
Vector3 g_angle;
Vector3 g_gyro;
float   g_mpu6050_temp;

void setup() {
  static int error = 0;

  memset(&g_acc, 0, sizeof(g_acc) );
  memset(&g_angle, 0, sizeof(g_angle) );
  memset(&g_gyro, 0, sizeof(g_gyro) );
  g_mpu6050_temp = 0;

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

  Wire.begin();
  Serial.begin(115200);
  error = InitMPU6050();
  if( error != 0 )
  {
    Serial.print("E\tReadMPU6050 Init error=");
    Serial.print(error);
    Serial.println("");
  }

  if( error == 0 )
  {
    g_mpu6050_init = true;
  }
}

void loop() {
  static int error = 0;
  if( g_mpu6050_init == true )
  {
    error = ReadMPU6050(&g_acc, &g_angle, &g_gyro, &g_mpu6050_temp);
    if( error != 0 )
    {
      Serial.print("E\tReadMPU6050 error=");
      Serial.print(error);
      Serial.println("");
    }
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
  delay(UPDATE_DELAY);
} /* loop */

static int InitMPU6050()
{
  int error;
  uint8_t cc;
  // 初回の読み出し
  error = MPU6050_read(MPU6050_WHO_AM_I, &cc, 1);
  if( error != 0 )
  {
    return error;
  }

  // 動作モードの読み出し
  error = MPU6050_read(MPU6050_PWR_MGMT_1, &cc, 1);
  if( error != 0 )
  {
    return error;
  }

  // MPU6050動作開始
  error = MPU6050_write_reg(MPU6050_PWR_MGMT_1, 0);

  return error;
} /* InitMPU6050 */

/*
pAcc    [ o]
pAngle  [ o]  Degree
pGyro   [ o]
pTemp   [ o]  Degree
*/
static int ReadMPU6050(Vector3 *pAcc, Vector3 *pAngle, Vector3 *pGyro, float *pTemp)
{
  int error = 0;
  uint8_t swap;
  accel_t_gyro_union accel_t_gyro;
  accel_t_gyro_union *pData = &accel_t_gyro;

  error = MPU6050_read(MPU6050_ACCEL_XOUT_H, (uint8_t *)pData, sizeof(accel_t_gyro));
  if( error != 0 )
  {
    memset( pAcc, 0x00, sizeof(Vector3) );
    memset( pAngle, 0x00, sizeof(Vector3) );
    memset( pGyro, 0x00, sizeof(Vector3) );
    *pTemp = 0;
    return error;
  }
#define SWAP(x,y) swap = x; x = y; y = swap
  SWAP (pData->reg.x_accel_h, pData->reg.x_accel_l);
  SWAP (pData->reg.y_accel_h, pData->reg.y_accel_l);
  SWAP (pData->reg.z_accel_h, pData->reg.z_accel_l);
  SWAP (pData->reg.t_h,       pData->reg.t_l);
  SWAP (pData->reg.x_gyro_h,  pData->reg.x_gyro_l);
  SWAP (pData->reg.y_gyro_h,  pData->reg.y_gyro_l);
  SWAP (pData->reg.z_gyro_h,  pData->reg.z_gyro_l);

  // 取得した加速度値を分解能で割って加速度(G)に変換する
  pAcc->x = pData->value.x_accel / 16384.0;
  pAcc->y = pData->value.y_accel / 16384.0;
  pAcc->z = pData->value.z_accel / 16384.0;

  // 加速度からセンサ対地角を求める
  // Degree
  pAngle->x = atan2(pAcc->x, pAcc->z) * 360 / 2.0 / PI;
  pAngle->y = atan2(pAcc->y, pAcc->z) * 360 / 2.0 / PI;
  pAngle->z = atan2(pAcc->x, pAcc->y) * 360 / 2.0 / PI;

  // 取得した角速度値を分解能で割って角速度(degrees per sec)に変換する
  pGyro->x = pData->value.x_gyro / 131.0;
  pGyro->y = pData->value.y_gyro / 131.0;
  pGyro->z = pData->value.z_gyro / 131.0;

  *pTemp = ( (float) accel_t_gyro.value.temperature + 12412.0f) / 340.0f;

  return error;
} /* ReadMPU6050 */


static void ReadSerialCommand()
{
  char cmd = Serial.read();
  switch(cmd)
  {
    /* Version */
    case 'V':
      {
        Serial.print('V');
        Serial.print(SEP_CHAR);
        Serial.println(VERSION_STRING);
      }
      break;
  }
} /* ReadSerialCommand */

static void OutputSerial( void ){
  char bufVars[0x10][10+1];
  memset( bufVars, 0x00, sizeof(bufVars) );

  // 浮動小数点を文字列に変換
  dtostrf(g_WaterTmp,    3,4, bufVars[0] );
  dtostrf(g_OilTmp,      3,4, bufVars[1] );
  dtostrf(g_OilPrs,      3,4, bufVars[2] );
  dtostrf(g_BoostPrs,    3,4, bufVars[3] );
  dtostrf(g_tachoRpm,    5,0, bufVars[4] );
  dtostrf(g_speedKm,     3,0, bufVars[5] );

  dtostrf(g_acc.x,       1,5, bufVars[6] );
  dtostrf(g_acc.y,       1,5, bufVars[7] );
  dtostrf(g_acc.z,       1,5, bufVars[8] );
  dtostrf(g_angle.x,     4,5, bufVars[9] );
  dtostrf(g_angle.y,     4,5, bufVars[10] );
  dtostrf(g_angle.z,     4,5, bufVars[11] );
  dtostrf(g_gyro.x,      4,5, bufVars[12] );
  dtostrf(g_gyro.y,      4,5, bufVars[13] );
  dtostrf(g_gyro.z,      4,5, bufVars[14] );
  dtostrf(g_mpu6050_temp,3,5, bufVars[15] );

  /* G = 16 */
  Serial.print("DG");
  Serial.print(SEP_CHAR);
  for( int ii = 0; ii < 0x10; ii++ )
  {
    Serial.print(bufVars[ii]);
    Serial.print(SEP_CHAR);
  }
  Serial.println("");

} /* OutputSerial */

static void UpdateSensorInfo()
{
  float wtr_avg = 0;
  float oilT_avg = 0;
  float oilP_avg = 0;

  // センサーから各温度・圧力を取得
  g_WaterTmp  = get_temp(WATER_SENSOR_PIN);
  g_OilTmp    = get_temp(OIL_SENSOR_PIN);
  g_OilPrs  = get_oil_pressure(PRESSURE_SENSOR_PIN);
  g_BoostPrs  = get_boost_press(BOOST_SENSOR_PIN);

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

/* ブースト圧(kPa) */
static float get_boost_press( const int pinNum )
{
  float ret_psi;
  float ret_kpa;
  double input_for_value = analogReadAvg(pinNum, SENSOR_AVG_COUNT);
  ret_psi=(((input_for_value - 102) / 0.1798f ) - 1535) * 0.01f;
  ret_kpa = ret_psi * 6.895f;
  return ret_kpa;
} /* get_boost_press */

static void InterruptTachoFunc( void )
{
//const float SPEED_MAX = 400.0f;
//const float RPM_MAX = 11000.0f;
  const float ONE_MIN_USEC = 60.0f * 1000.0f * 1000.0f;
  float _tachoRpm = 0;

  g_tachoAfter = micros();//現在の時刻を記録
  g_tachoWidth = g_tachoAfter - g_tachoBefore;//前回と今回の時間の差を計算
  g_tachoBefore = g_tachoAfter;//今回の値を前回の値に代入する
  if( g_tachoWidth <= 0 )
  {
    return;
  }
  _tachoRpm = ONE_MIN_USEC / (g_tachoWidth * 2.0f);//タイヤの回転数[rpm]を計算

  /* 回転数の最大値を超えていた場合、誤検知とする */
  if( _tachoRpm >= RPM_MAX )
  {
    return;
  }

  g_tachoRpm = _tachoRpm;
} /* InterruptTachoFunc */

static void InterruptSpeedFunc( void )
{
  const float CSPD = 60.0 * 60 / (637 * SPEED_PULSE_COUNT) * 1000 * 1000;
  float _speedKm = 0;

  g_speedAfter = micros();//現在の時刻を記録
  g_speedWidth = g_speedAfter - g_speedBefore;//前回と今回の時間の差を計算
  g_speedBefore = g_speedAfter;//今回の値を前回の値に代入する
  if( g_speedWidth <= 0 )
  {
    return;
  }
  _speedKm = CSPD / g_speedWidth;
  /* 速度の最大値を超えていた場合、誤検知とする */
  if( _speedKm >= SPEED_MAX )
  {
    return;
  }
  
  /* 加速度が10を超えていた場合、誤検知とする */
  if( fabs(g_speedKm - _speedKm) > 10 )
  {
    return;
  }

  g_speedKm = _speedKm;
} /* InterruptSpeedFunc */

/*
  パルスが入らない状態を確認して 0rpmを設定する
*/
static void UpdateTachoReset( void ){
  const float ONE_MIN_USEC = 60.0f * 1000.0f * 1000.0f / 2.0f;
  const float CSPD = 60.0 * 60 / (637 * SPEED_PULSE_COUNT) * 1000 * 1000;
  unsigned long width = micros() - g_tachoBefore;
  if( width <= CSPD )
    return;

  if( width <= 0 )
  {
    g_tachoWidth = 0.0f;
    g_tachoBefore = micros();
    g_tachoAfter = g_tachoBefore;
    return;
  }

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

  if( width <= 0 )
  {
    g_speedWidth = 0.0f;
    g_speedBefore = micros();
    g_speedAfter = g_speedBefore;
    return;
  }
  g_speedWidth = 0.0f;
  g_speedKm = 0.0f;

} /* UpdateSpeedReset */

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

// MPU6050_read
int MPU6050_read(int start, uint8_t *buffer, int size) {
  int ii, nn, error;
  Wire.beginTransmission(MPU6050_I2C_ADDRESS);
  nn = Wire.write(start);
  if (nn != 1) {
    return (-10);
  }
  nn = Wire.endTransmission(false);// hold the I2C-bus
  if (nn != 0) {
    return (nn);
  }
  // Third parameter is true: relase I2C-bus after data is read.
  Wire.requestFrom(MPU6050_I2C_ADDRESS, size, true);
  ii = 0;
  while (Wire.available() && ii < size) {
    buffer[ii++] = Wire.read();
  }
  if ( ii != size) {
    return (-11);
  }
  return (0); // return : no error
} /* MPU6050_read */

// MPU6050_write
int MPU6050_write(int start, const uint8_t *pData, int size) {
  int nn, error;
  Wire.beginTransmission(MPU6050_I2C_ADDRESS);
  nn = Wire.write(start);// write the start address
  if (nn != 1) {
    return (-20);
  }
  nn = Wire.write(pData, size);// write data bytes
  if (nn != size) {
    return (-21);
  }
  error = Wire.endTransmission(true); // release the I2C-bus
  if (error != 0) {
    return (error);
  }

  return (0);// return : no error
} /* MPU6050_write */

// MPU6050_write_reg
int MPU6050_write_reg(int reg, uint8_t data) {
  int error;
  error = MPU6050_write(reg, &data, 1);
  return (error);
} /* MPU6050_write_reg */

/* EOF */
