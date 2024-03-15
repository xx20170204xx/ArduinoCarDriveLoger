/**************************************************************************
 This is an example for our Monochrome OLEDs based on SSD1306 drivers

 Pick one up today in the adafruit shop!
 ------> http://www.adafruit.com/category/63_98

 This example is for a 128x32 pixel display using I2C to communicate
 3 pins are required to interface (two I2C and one reset).

 Adafruit invests time and resources providing this open
 source code, please support Adafruit and open-source
 hardware by purchasing products from Adafruit!

 Written by Limor Fried/Ladyada for Adafruit Industries,
 with contributions from the open source community.
 BSD license, check license.txt for more information
 All text above, and the splash screen below must be
 included in any redistribution.
 **************************************************************************/

#include <SPI.h>
#include <Wire.h>
#include <EEPROM.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define SCREEN_WIDTH 128 // OLED display width, in pixels
#define SCREEN_HEIGHT 32 // OLED display height, in pixels

// Declaration for an SSD1306 display connected to I2C (SDA, SCL pins)
// The pins for I2C are defined by the Wire-library. 
// On an arduino UNO:       A4(SDA), A5(SCL)
// On an arduino MEGA 2560: 20(SDA), 21(SCL)
// On an arduino LEONARDO:   2(SDA),  3(SCL), ...
#define OLED_RESET     -1 // Reset pin # (or -1 if sharing Arduino reset pin)
#define SCREEN_ADDRESS 0x3C ///< See datasheet for Address; 0x3D for 128x64, 0x3C for 128x32
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);

#define NUMFLAKES     10 // Number of snowflakes in the animation example

#define LOGO_HEIGHT   16
#define LOGO_WIDTH    16
static const unsigned char PROGMEM logo_bmp[] =
{ 0b00000000, 0b11000000,
  0b00000001, 0b11000000,
  0b00000001, 0b11000000,
  0b00000011, 0b11100000,
  0b11110011, 0b11100000,
  0b11111110, 0b11111000,
  0b01111110, 0b11111111,
  0b00110011, 0b10011111,
  0b00011111, 0b11111100,
  0b00001101, 0b01110000,
  0b00011011, 0b10100000,
  0b00111111, 0b11100000,
  0b00111111, 0b11110000,
  0b01111100, 0b11110000,
  0b01110000, 0b01110000,
  0b00000000, 0b00110000 };

const float BOOST_MIN   =-100.0f;
const float BOOST_MAX   = 150.0f;
const int8_t RECVDATA_MAX = 14;
const float THROTTLE_MAX   = 1024.0f;

#define BTN01_PIN A3
#define BTN02_PIN 3

enum AXIS_FLAG
{
  AXIS_FLAG_X_PLUS  = (1 << 0),
  AXIS_FLAG_X_MINUS = (1 << 1),
  AXIS_FLAG_Y_PLUS  = (1 << 2),
  AXIS_FLAG_Y_MINUS = (1 << 3),
  AXIS_FLAG_Z_PLUS  = (1 << 4),
  AXIS_FLAG_Z_MINUS = (1 << 5),

  AXIS_FLAG_X_MASK  = (AXIS_FLAG_X_PLUS | AXIS_FLAG_X_MINUS),
  AXIS_FLAG_Y_MASK  = (AXIS_FLAG_Y_PLUS | AXIS_FLAG_Y_MINUS),
  AXIS_FLAG_Z_MASK  = (AXIS_FLAG_Z_PLUS | AXIS_FLAG_Z_MINUS),
};

typedef struct
{
  float x;
  float y;
  float z;
} SVECTOR3, *PVECTOR3;

typedef struct 
{
  bool receved;
  float waterTemp;
  float oilTemp;
  float oilPress;
  float boostPress;

  float tacho;
  float speed;
  float throttle;

  float mpu6050Temp;
  SVECTOR3 acc;
  SVECTOR3 angle;
} SRECVDATA;

typedef struct
{
  size_t    size;
  char      m_modeNum;
  int       m_szThrLow;
  int       m_szThrHigh;
  int       m_AxisVFlag;
  int       m_AxisHFlag;
} EEPROMDATA;

static void InitRomData( EEPROMDATA* pRom );
static void writeRomData( EEPROMDATA* pRom );
static void buttonCheck(void);

#define MODE_THROTTLE 't'

char g_mode_table[] = { 
'S', /* Speed */
'T', /* Tacho */
'W',
'O',
'o',
'B',
MODE_THROTTLE,
'A',
'a',
};

volatile char g_mode = 't';

SRECVDATA g_recvData;
EEPROMDATA g_EEPEOM;

void setup() {

  memset(&g_recvData, 0x00, sizeof(g_recvData));
  
  // Serial.begin(9600);
  Serial.begin(115200);

  // SSD1306_SWITCHCAPVCC = generate display voltage from 3.3V internally
  if(!display.begin(SSD1306_SWITCHCAPVCC, SCREEN_ADDRESS)) {
    Serial.println(F("SSD1306 allocation failed"));
    for(;;); // Don't proceed, loop forever
  }

  // Show initial display buffer contents on the screen --
  // the library initializes this with an Adafruit splash screen.
  display.display();
  delay(2000); // Pause for 2 seconds

  // Clear the buffer
  display.clearDisplay();

  {
    int ii;
    int rom_len = EEPROM.length();
    char* pBuf = (char*)&g_EEPEOM;

    for( ii = 0; ii < sizeof(g_EEPEOM); ii++ )
    {
      pBuf[ii] = EEPROM.read(ii);
    }
    
    Serial.print("Size:");
    Serial.println(g_EEPEOM.size);

    Serial.print("Mode:");
    Serial.println(g_mode_table[g_EEPEOM.m_modeNum]);

    Serial.print("Low:");
    Serial.println(g_EEPEOM.m_szThrLow);

    Serial.print("High:");
    Serial.println(g_EEPEOM.m_szThrHigh);

    if( CheckRomData(&g_EEPEOM) != 0 )
    {
      /* init ROMDATA */
      InitRomData(&g_EEPEOM);
      writeRomData(&g_EEPEOM);
    }
    g_mode = g_mode_table[g_EEPEOM.m_modeNum];
  }

  pinMode(BTN01_PIN, INPUT_PULLUP);
  pinMode(BTN02_PIN, INPUT_PULLUP);
}

void loop() {

  recvSerial();
  buttonCheck();

  display3Meter();
  return;
  switch(g_mode)
  {
    case 'S':
    displaySpeed(g_recvData.speed);
    break;

    case 'T':
    displayTacho(g_recvData.tacho);
    break;
    
    case 'W':
    displayWaterTemp(g_recvData.waterTemp);
    break;

    case 'O':
    displayOilTemp(g_recvData.oilTemp);
    break;

    case 'o':
    displayOilPress(g_recvData.oilPress);
    break;

    case 'B':
    displayBoostPress(g_recvData.boostPress);
    break;

    case MODE_THROTTLE:
    displayThrottle(g_recvData.throttle);
    break;

    case 'A':
    displayAcc(&g_recvData.acc);
    break;

    case 'a':
    displayAngle(&g_recvData.angle);
    break;


    default:
    // NOP
    break;
  }

  delay(1000/60);
}

void recvSerial()
{
  char recvBuf[256];

#if 1
  /* 受信データが存在しない場合、処理を抜ける */
  if(Serial.available() <= 0) { return; }
#endif  
  
  g_recvData.receved= false;
  memset( recvBuf, 0x00, sizeof(recvBuf) );

  /* データ受信＆分割 */
  Serial.readBytesUntil('\n',recvBuf, sizeof(recvBuf) );

  switch (recvBuf[0])
  {
  case 'D':
    recvSerial_NormalData(recvBuf);
    break;
  
  /*
   * Water Temp     : Z0<TAB>0.0000
   * Oil Temp       : Z1<TAB>0.0000
   * Oil Press      : Z2<TAB>0.0000
   * Boost Press    : Z3<TAB>0.0000
   * Tacho          : Z4<TAB>0.0000
   * Speed          : Z5<TAB>0.0000
   * throttle       : Z6<TAB>0.0000
   * mpu6050 Temp   : Z7<TAB>0.0000
   * mpu6050 Acc    : Z8<TAB>0.0000<TAB>0.0000<TAB>0.0000
   * mpu6050 Angle  : Z9<TAB>0.0000<TAB>0.0000<TAB>0.0000
  */
  case 'Z':
    recvSerial_DummyData(recvBuf);
    break;

  default:
    break;
  }


} /* recvSerial */

void recvSerial_NormalData(char* pData)
{
  char *ptr;
  float data[14];
  int ii = 0;
  ptr = strtok(pData, "\t");

  while( ptr != NULL )
  {
    ptr = strtok(NULL, "\t");
    if( ptr == NULL )
    {
      break;
    }

    data[ii++] = atof(ptr);
    if(ii==14)break;
  }

  g_recvData.receved    = true;
  g_recvData.waterTemp  = data[0];
  g_recvData.oilTemp    = data[1];
  g_recvData.oilPress   = data[2];
  g_recvData.boostPress = data[3];

  g_recvData.tacho    = data[4];
  g_recvData.speed    = data[5];
  g_recvData.throttle = data[6];

  g_recvData.mpu6050Temp = data[7];

  g_recvData.acc.x = data[8];
  g_recvData.acc.y = data[9];
  g_recvData.acc.z = data[10];

  g_recvData.angle.x = data[11];
  g_recvData.angle.y = data[12];
  g_recvData.angle.z = data[13];
} /* recvSerial_NormalData */

void recvSerial_DummyData(char* pData)
{
  char *ptr;
  float data[14];
  int ii = 0;
  ptr = strtok(pData, "\t");

  memset( data, 0x00, sizeof(data) );

  while( ptr != NULL )
  {
    ptr = strtok(NULL, "\t");
    if( ptr == NULL )
    {
      break;
    }

    data[ii++] = atof(ptr);
    if(ii==14)break;
  }

  switch (pData[1])
  {
  case '0':
    g_recvData.waterTemp = data[0];
    break;
  
  case '1':
    g_recvData.oilTemp = data[0];
    break;
  
  case '2':
    g_recvData.oilPress = data[0];
    break;
  
  case '3':
    g_recvData.boostPress = data[0];
    break;

  case '4':
    g_recvData.tacho = data[0];
    break;
  case '5':
    g_recvData.speed = data[0];
    break;
  case '6':
    g_recvData.throttle = data[0];
    break;

  case '7':
    g_recvData.mpu6050Temp = data[0];
    break;

  case '8':
    g_recvData.acc.x = data[0];
    g_recvData.acc.y = data[1];
    g_recvData.acc.z = data[2];
    break;

  case '9':
    g_recvData.angle.x = data[0];
    g_recvData.angle.y = data[1];
    g_recvData.angle.z = data[2];
    break;


  default:
    break;
  }

} /* recvSerial_DummyData */

void displayTacho(float tacho)
{
  char buf[10+1];
  char tachoBuf[10+1];
  memset( buf, 0x00, sizeof(buf) );
  memset( tachoBuf, 0x00, sizeof(tachoBuf) );
  dtostrf(tacho,    5,0, tachoBuf ); // ZZZZ9
  sprintf(buf,"%6.6s",tachoBuf);

  display.clearDisplay();

  display.setTextSize(1);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(0,0);             // Start at top-left corner
  display.println("TACHO(rpm)");

  display.setTextSize(3);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(16,8);             // Start at top-left corner
  display.println(buf);
  
  display.display();
} /* displayTacho */

void displaySpeed(float speed)
{
  char buf[10+1];
  char speedBuf[10+1];
  memset( buf, 0x00, sizeof(buf) );
  memset( speedBuf, 0x00, sizeof(speedBuf) );
  dtostrf(speed,    3,0, speedBuf );  // ZZ9
  sprintf(buf,"%6.6s",speedBuf);

  display.clearDisplay();

  display.setTextSize(1);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(0,0);             // Start at top-left corner
  display.println("SPEED(Km/h)");

  display.setTextSize(3);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(16,8);             // Start at top-left corner
  display.println(buf);
  
  display.display();
} /* displaySpeed */

void displayWaterTemp(float water)
{
  char buf[10+1];
  char waterBuf[10+1];
  memset( buf, 0x00, sizeof(buf) );
  memset( waterBuf, 0x00, sizeof(waterBuf) );
  dtostrf(water,    3,0, waterBuf ); // ZZ9
  sprintf(buf,"%6.6s",waterBuf);

  display.clearDisplay();

  display.setTextSize(1);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(0,0);             // Start at top-left corner
  display.println("Water Temp(degree)");

  display.setTextSize(3);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(16,8);             // Start at top-left corner
  display.println(buf);
  
  display.display();
} /* displayWaterTemp */

void displayOilTemp(float oil)
{
  char buf[10+1];
  char oilBuf[10+1];
  memset( buf, 0x00, sizeof(buf) );
  memset( oilBuf, 0x00, sizeof(oilBuf) );
  dtostrf(oil,    3,0, oilBuf ); // ZZ9
  sprintf(buf,"%6.6s",oilBuf);

  display.clearDisplay();

  display.setTextSize(1);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(0,0);             // Start at top-left corner
  display.println("Oil Temp(degree)");

  display.setTextSize(3);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(16,8);             // Start at top-left corner
  display.println(buf);
  
  display.display();
} /* displayOilTemp */

void displayOilPress(float oil)
{
  char buf[10+1];
  char oilBuf[10+1];
  memset( buf, 0x00, sizeof(buf) );
  memset( oilBuf, 0x00, sizeof(oilBuf) );
  dtostrf(oil,    2,1, oilBuf );  // Z9.9
  sprintf(buf,"%6.6s",oilBuf);

  display.clearDisplay();

  display.setTextSize(1);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(0,0);             // Start at top-left corner
  display.println("Oil Press(bar)");

  display.setTextSize(3);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(16,8);             // Start at top-left corner
  display.println(buf);
  
  display.display();
} /* displayOilPress */

void displayBoostPress(float boost)
{
  int16_t width_p = display.width() / 3;
  int16_t xx,yy = 8;
  int16_t width = 0;
  int16_t height = display.height() - 8;

  char buf[16+1];
  char boostBuf[10+1];
  memset( buf, 0x00, sizeof(buf) );
  memset( boostBuf, 0x00, sizeof(boostBuf) );
  dtostrf(boost,    2,1, boostBuf );  // Z9.9
  sprintf(buf,"%9.9s",boostBuf);

  /* Boost + */
  if( boost > 0 ){
    xx = width_p;
    width = (display.width() - width_p) * (boost / BOOST_MAX);

  /* Boost - */
  }else{
    xx = width_p * (1 - (boost / BOOST_MIN));
    width = width_p - xx;
  }
  
  display.clearDisplay();

  /* タイトル表示 */
  display.setTextSize(1);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(0,0);             // Start at top-left corner
  display.println("Boost Press(kPh)");

  /* Grid */
  display.drawRect(0, 8, display.width(), height, SSD1306_WHITE);
  display.drawLine(width_p, 8, width_p, display.height(), SSD1306_WHITE);

  display.fillRect(xx, yy, width, height, SSD1306_WHITE);

  /* テキスト表示 */
  display.setTextSize(2);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_INVERSE);        // Draw white text
  display.setCursor(16,16);             // Start at top-left corner
  display.println(buf);
  
  display.display();
} /* displayBoostPress */

void displayThrottle(float throttle)
{
  char buf[10+1];
  char throttleBuf[10+1];
  int16_t width_p = display.width() / 3;
  int16_t xx,yy = 8;
  int16_t width = 0;
  int16_t height = display.height() - 8;
  float per = throttle;
  float per2;

  per = per - 0;
  per = per / (g_EEPEOM.m_szThrHigh - 0);
  per2 = per * 100;
  per2 = (per2 < 0.0f ? 0.0f : per2);
  per2 = (per2 > 100.0f ? 100.0f : per2);

  memset( buf, 0x00, sizeof(buf) );
  memset( throttleBuf, 0x00, sizeof(throttleBuf) );

  dtostrf(per2,    3,1, throttleBuf ); // ZZ9.9
  sprintf(buf,"%6.6s",throttleBuf);

  display.clearDisplay();

  display.setTextSize(1);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(0,0);             // Start at top-left corner
  display.println("Throttle");

  display.drawRect(0, 8, display.width(), height, SSD1306_WHITE);

  width = display.width() * per;

  display.fillRect(xx, yy, width, height, SSD1306_WHITE);

  display.setTextSize(3);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_INVERSE);        // Draw white text
  display.setCursor(16,8);             // Start at top-left corner
  display.println(buf);

  display.display();

} /* displayThrottle */


void displayAcc(PVECTOR3 acc)
{
  char bufX[32+1];
  char bufY[32+1];
  char valueBuf[10+1];
  memset( bufX, 0x00, sizeof(bufX) );
  memset( bufY, 0x00, sizeof(bufY) );
  memset( valueBuf, 0x00, sizeof(valueBuf) );
  dtostrf(acc->x,    2,1, valueBuf );  // Z9.9
  sprintf(bufX,"%16.16s",valueBuf);

  memset( valueBuf, 0x00, sizeof(valueBuf) );
  dtostrf(acc->y,    2,1, valueBuf );  // Z9.9
  sprintf(bufY,"%16.16s",valueBuf);
  display.clearDisplay();

  /* タイトル表示 */
  display.setTextSize(1);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(0,0);             // Start at top-left corner
  display.println("Gyro Acc");

  display.drawCircle(128/2, 32/2, 15, SSD1306_WHITE);
  display.drawCircle(128/2, 32/2, 8, SSD1306_WHITE);
  display.drawLine(128/2, 0, 128/2, 31, SSD1306_WHITE);
  display.drawLine(128/2-16, 32/2, 128/2+16, 32/2, SSD1306_WHITE);

  float cur_x = acc->x * 16.0f;
  float cur_y = acc->y * 16.0f;
  // Cursor
  display.fillCircle(128/2 + (int)cur_x, 32/2 + (int)cur_y, 3, SSD1306_INVERSE);

  /* テキスト表示 */
  // display.setTextSize(2);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_INVERSE);        // Draw white text
  display.setCursor(16,16);             // Start at top-left corner
  display.println(bufX);
  display.setCursor(16,24);             // Start at top-left corner
  display.println(bufY);
  
  display.display();
} /* displayAcc */

void displayAngle(PVECTOR3 angle)
{
  char bufX[32+1];
  char bufY[32+1];
  char valueBuf[10+1];
  memset( bufX, 0x00, sizeof(bufX) );
  memset( bufY, 0x00, sizeof(bufY) );
  memset( valueBuf, 0x00, sizeof(valueBuf) );
  dtostrf(angle->x,    2,1, valueBuf );  // Z9.9
  sprintf(bufX,"%16.16s",valueBuf);

  memset( valueBuf, 0x00, sizeof(valueBuf) );
  dtostrf(angle->y,    2,1, valueBuf );  // Z9.9
  sprintf(bufY,"%16.16s",valueBuf);
  display.clearDisplay();

  /* タイトル表示 */
  display.setTextSize(1);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(0,0);             // Start at top-left corner
  display.println("Gyro Angle");

  display.drawCircle(128/2, 32/2, 15, SSD1306_WHITE);
  display.drawCircle(128/2, 32/2, 8, SSD1306_WHITE);
  display.drawLine(128/2, 0, 128/2, 31, SSD1306_WHITE);
  display.drawLine(128/2-16, 32/2, 128/2+16, 32/2, SSD1306_WHITE);

  float cur_x = (angle->x/180.0f) * 16.0f;
  float cur_y = (angle->y/180.0f) * 16.0f;
  // Cursor
  display.fillCircle(128/2 + (int)cur_x, 32/2 + (int)cur_y, 3, SSD1306_INVERSE);

  /* テキスト表示 */
  // display.setTextSize(2);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_INVERSE);        // Draw white text
  display.setCursor(16,16);             // Start at top-left corner
  display.println(bufX);
  display.setCursor(16,24);             // Start at top-left corner
  display.println(bufY);
  
  display.display();
} /* displayAngle */

void display3Meter(){
  char tachoBuf[3][10+1];
  int fill_r[3];

  char printBuf[10+1];
  char throttleBuf[10+1];
  int16_t width_p = display.width() / 3;
  int16_t xx,yy = display.height()-4;
  int16_t width = 0;
  int16_t height = 4;
  float per = g_recvData.throttle;
  float per2;

  per = per - 0;
  per = per / (g_EEPEOM.m_szThrHigh - 0);
  per2 = per * 100;
  per2 = (per2 < 0.0f ? 0.0f : per2);
  per2 = (per2 > 100.0f ? 100.0f : per2);

  memset( printBuf, 0x00, sizeof(printBuf) );
  memset( throttleBuf, 0x00, sizeof(throttleBuf) );

  dtostrf(per2,    3,0, throttleBuf ); // ZZ9
  sprintf(printBuf,"%3.3s",throttleBuf);

  memset( tachoBuf, 0x00, sizeof(tachoBuf) );
  memset( fill_r, 0x00, sizeof(fill_r) );

  dtostrf(g_recvData.waterTemp, 3,0, tachoBuf[0] );
  dtostrf(g_recvData.oilTemp, 3,0, tachoBuf[1] );
  dtostrf(g_recvData.oilPress, 3,0, tachoBuf[2] );
  // sprintf(buf,"%6.6s",tachoBuf);

  fill_r[0] = (g_recvData.waterTemp / 130) * 12;
  fill_r[1] = (g_recvData.oilTemp / 130) * 12;
  fill_r[2] = (g_recvData.oilPress / 10) * 12;
  fill_r[0] = (fill_r[0] > 12 ? 12 : (fill_r[0] < 0 ? 0 : fill_r[0]));
  fill_r[1] = (fill_r[1] > 12 ? 12 : (fill_r[1] < 0 ? 0 : fill_r[1]));
  fill_r[2] = (fill_r[2] > 12 ? 12 : (fill_r[2] < 0 ? 0 : fill_r[2]));

  display.clearDisplay();

  display.drawCircle(16, 16, 12, SSD1306_WHITE);
  display.fillCircle(16, 16, fill_r[0], SSD1306_WHITE);

  display.drawCircle(48, 16, 12, SSD1306_WHITE);
  display.fillCircle(48, 16, fill_r[1], SSD1306_WHITE);

  display.drawCircle(80, 16, 12, SSD1306_WHITE);
  display.fillCircle(80, 16, fill_r[2], SSD1306_WHITE);

  display.drawRect(xx, yy, display.width(), height, SSD1306_WHITE);
  width = display.width() * per;
  display.fillRect(xx, yy, width, height, SSD1306_WHITE);

  /* テキスト表示 */
  display.setTextSize(1);
  display.setTextColor(SSD1306_WHITE);
  display.setCursor(96,0);
  display.println(tachoBuf[0]);
  display.setCursor(96,8);
  display.println(tachoBuf[1]);
  display.setCursor(96,16);
  display.println(tachoBuf[2]);
  display.setCursor(96,24);
  display.println(printBuf);





  display.display();
} /* display3Meter */

static int CheckRomData( EEPROMDATA* pRom )
{
  int l_iCnt1=0;
  int l_Flags = pRom->m_AxisVFlag | pRom->m_AxisHFlag;

  if( pRom == NULL )
  {
    return 1;
  }

  if( pRom->size != sizeof(EEPROMDATA) )
  {
    return 2;
  }

  /* フラグの指定に値が設定されていない場合エラーとする */
  if( pRom->m_AxisVFlag == 0 || pRom->m_AxisHFlag == 0 )
  {
    return 3;
  }

  l_iCnt1 += (l_Flags & AXIS_FLAG_X_MASK) ? 1 : 0;
  l_iCnt1 += (l_Flags & AXIS_FLAG_Y_MASK) ? 1 : 0;
  l_iCnt1 += (l_Flags & AXIS_FLAG_Z_MASK) ? 1 : 0;
  
  /* 2種類の軸が指定されていない場合エラーとする */
  if( l_iCnt1 != 2 )
  {
    return 4;
  }

  return 0;
} /* CheckRomData */

static void InitRomData( EEPROMDATA* pRom )
{
  if( pRom == NULL )
  {
    return;
  }
  
  pRom->size = sizeof(EEPROMDATA);
  pRom->m_modeNum = 0;
  pRom->m_szThrLow = 0;
  pRom->m_szThrHigh = 1024;
  pRom->m_AxisVFlag = AXIS_FLAG_X_PLUS;
  pRom->m_AxisHFlag = AXIS_FLAG_Y_PLUS;
  //
} /* InitRomData */

static void writeRomData( EEPROMDATA* pRom )
{
  int ii;
  char* pBuf = (char*)pRom;
  if( pRom == NULL )
  {
    return;
  }

  for( ii = 0; ii < sizeof(EEPROMDATA); ii++ )
  {
    EEPROM.write(ii, pBuf[ii]);
  }

} /* writeRomData */

static void buttonCheck(void)
{
  static unsigned int btn01 = 0x0000;
  static unsigned int btn02 = 0x0000;
  
  btn01 = (btn01 << 1);
  btn01 = btn01 & 0xFFFE;

  btn02 = (btn02 << 1);
  btn02 = btn02 & 0xFFFE;

  if( !digitalRead(BTN01_PIN) )
  {
    btn01 = btn01 | 0x0001;
  }

  if( digitalRead(BTN02_PIN)==0 )
  {
    // Serial.println("throttle Push.");
    btn02 = btn02 | 0x0001;
  }


  if( (btn01 & 0x7FFF) == 0x7FFF &&
      (btn01 & 0x8000) == 0 )
  {
    g_EEPEOM.m_modeNum += 1;
    if( g_EEPEOM.m_modeNum >= sizeof(g_mode_table) )
    {
      g_EEPEOM.m_modeNum = 0;
    }
    g_mode = g_mode_table[g_EEPEOM.m_modeNum];
    writeRomData( &g_EEPEOM );
  }

  if( (btn02 & 0x7FFF) == 0x7FFF &&
      (btn02 & 0x8000) == 0 )
  {
    /* Init */
    // Serial.println("throttle Init.");
    g_EEPEOM.m_szThrLow = g_recvData.throttle;
    g_EEPEOM.m_szThrHigh = g_recvData.throttle;
  }else if( (btn02 & 0xFFFF) == 0xFFFF ){
    // Serial.print("throttle ");
    // Serial.println(btn02);
    if( g_EEPEOM.m_szThrLow > g_recvData.throttle ){ g_EEPEOM.m_szThrLow = g_recvData.throttle; }
    if( g_EEPEOM.m_szThrHigh < g_recvData.throttle ){ g_EEPEOM.m_szThrHigh = g_recvData.throttle; }

  }else if( (btn02 & 0x8000) == 0x8000 &&
            (btn02 & 0x7FFF) == 0x0000 ){
    // Serial.println("throttle Write.");
    writeRomData( &g_EEPEOM );
  }else{
  }


} /* buttonCheck */
