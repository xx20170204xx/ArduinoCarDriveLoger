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
#define BTN02_PIN A6

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
  size_t  size;
  char    m_modeNum;
  int     m_szThrLow;
  int     m_szThrHigh;
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

    if( g_EEPEOM.size != sizeof(EEPROMDATA) )
    {
      /* init ROMDATA */
      InitRomData(&g_EEPEOM);
      for( ii = 0; ii < sizeof(g_EEPEOM); ii++ )
      {
        EEPROM.write(ii, pBuf[ii]);
      }
    }
    g_mode = g_mode_table[g_EEPEOM.m_modeNum];
  }

  pinMode(BTN01_PIN, INPUT_PULLUP);
  pinMode(BTN02_PIN, INPUT_PULLUP);
}

void loop() {

  recvSerial();
  buttonCheck();

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
  char *ptr;
  float data[14];
  int ii = 0;

#if 0
  /* 受信データが存在しない場合、処理を抜ける */
  if(Serial.available() <= 0) { return; }
#endif  
  
  g_recvData.receved= false;
  memset( recvBuf, 0x00, sizeof(recvBuf) );

  /* データ受信＆分割 */
  Serial.readBytesUntil('\n',recvBuf, sizeof(recvBuf) );

  if( recvBuf[0] != 'D' )
  {
    return;
  }

  ptr = strtok(recvBuf, "\t");

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

} /* recvSerial */


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

  per = per - g_EEPEOM.m_szThrLow;
  per = per / (g_EEPEOM.m_szThrHigh - g_EEPEOM.m_szThrLow);
  per = per * 100;
  per = (per < 0.0f ? 0.0f : per);
  per = (per > 100.0f ? 100.0f : per);

  memset( buf, 0x00, sizeof(buf) );
  memset( throttleBuf, 0x00, sizeof(throttleBuf) );

  dtostrf(per,    3,0, throttleBuf ); // ZZ9
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

#if 0
void testdrawline() {
  int16_t i;

  display.clearDisplay(); // Clear display buffer

  for(i=0; i<display.width(); i+=4) {
    display.drawLine(0, 0, i, display.height()-1, SSD1306_WHITE);
    display.display(); // Update screen with each newly-drawn line
    delay(1);
  }
  for(i=0; i<display.height(); i+=4) {
    display.drawLine(0, 0, display.width()-1, i, SSD1306_WHITE);
    display.display();
    delay(1);
  }
  delay(250);

  display.clearDisplay();

  for(i=0; i<display.width(); i+=4) {
    display.drawLine(0, display.height()-1, i, 0, SSD1306_WHITE);
    display.display();
    delay(1);
  }
  for(i=display.height()-1; i>=0; i-=4) {
    display.drawLine(0, display.height()-1, display.width()-1, i, SSD1306_WHITE);
    display.display();
    delay(1);
  }
  delay(250);

  display.clearDisplay();

  for(i=display.width()-1; i>=0; i-=4) {
    display.drawLine(display.width()-1, display.height()-1, i, 0, SSD1306_WHITE);
    display.display();
    delay(1);
  }
  for(i=display.height()-1; i>=0; i-=4) {
    display.drawLine(display.width()-1, display.height()-1, 0, i, SSD1306_WHITE);
    display.display();
    delay(1);
  }
  delay(250);

  display.clearDisplay();

  for(i=0; i<display.height(); i+=4) {
    display.drawLine(display.width()-1, 0, 0, i, SSD1306_WHITE);
    display.display();
    delay(1);
  }
  for(i=0; i<display.width(); i+=4) {
    display.drawLine(display.width()-1, 0, i, display.height()-1, SSD1306_WHITE);
    display.display();
    delay(1);
  }

  delay(1000); // Pause for 2 seconds
}

void testdrawrect(void) {
  display.clearDisplay();

  for(int16_t i=0; i<display.height()/2; i+=2) {
    display.drawRect(i, i, display.width()-2*i, display.height()-2*i, SSD1306_WHITE);
    display.display(); // Update screen with each newly-drawn rectangle
    delay(1);
  }

  delay(2000);
}

void testfillrect(void) {
  display.clearDisplay();

  for(int16_t i=0; i<display.height()/2; i+=3) {
    // The INVERSE color is used so rectangles alternate white/black
    display.fillRect(i, i, display.width()-i*2, display.height()-i*2, SSD1306_INVERSE);
    display.display(); // Update screen with each newly-drawn rectangle
    delay(1);
  }

  delay(2000);
}

void testdrawcircle(void) {
  display.clearDisplay();

  for(int16_t i=0; i<max(display.width(),display.height())/2; i+=2) {
    display.drawCircle(display.width()/2, display.height()/2, i, SSD1306_WHITE);
    display.display();
    delay(1);
  }

  delay(2000);
}

void testfillcircle(void) {
  display.clearDisplay();

  for(int16_t i=max(display.width(),display.height())/2; i>0; i-=3) {
    // The INVERSE color is used so circles alternate white/black
    display.fillCircle(display.width() / 2, display.height() / 2, i, SSD1306_INVERSE);
    display.display(); // Update screen with each newly-drawn circle
    delay(1);
  }

  delay(2000);
}

void testdrawroundrect(void) {
  display.clearDisplay();

  for(int16_t i=0; i<display.height()/2-2; i+=2) {
    display.drawRoundRect(i, i, display.width()-2*i, display.height()-2*i,
      display.height()/4, SSD1306_WHITE);
    display.display();
    delay(1);
  }

  delay(2000);
}

void testfillroundrect(void) {
  display.clearDisplay();

  for(int16_t i=0; i<display.height()/2-2; i+=2) {
    // The INVERSE color is used so round-rects alternate white/black
    display.fillRoundRect(i, i, display.width()-2*i, display.height()-2*i,
      display.height()/4, SSD1306_INVERSE);
    display.display();
    delay(1);
  }

  delay(2000);
}

void testdrawtriangle(void) {
  display.clearDisplay();

  for(int16_t i=0; i<max(display.width(),display.height())/2; i+=5) {
    display.drawTriangle(
      display.width()/2  , display.height()/2-i,
      display.width()/2-i, display.height()/2+i,
      display.width()/2+i, display.height()/2+i, SSD1306_WHITE);
    display.display();
    delay(1);
  }

  delay(2000);
}

void testfilltriangle(void) {
  display.clearDisplay();

  for(int16_t i=max(display.width(),display.height())/2; i>0; i-=5) {
    // The INVERSE color is used so triangles alternate white/black
    display.fillTriangle(
      display.width()/2  , display.height()/2-i,
      display.width()/2-i, display.height()/2+i,
      display.width()/2+i, display.height()/2+i, SSD1306_INVERSE);
    display.display();
    delay(1);
  }

  delay(2000);
}

void testdrawchar(void) {
  display.clearDisplay();

  display.setTextSize(1);      // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE); // Draw white text
  display.setCursor(0, 0);     // Start at top-left corner
  display.cp437(true);         // Use full 256 char 'Code Page 437' font

  // Not all the characters will fit on the display. This is normal.
  // Library will draw what it can and the rest will be clipped.
  for(int16_t i=0; i<256; i++) {
    if(i == '\n') display.write(' ');
    else          display.write(i);
  }

  display.display();
  delay(2000);
}

void testdrawstyles(void) {
  display.clearDisplay();

  display.setTextSize(1);             // Normal 1:1 pixel scale
  display.setTextColor(SSD1306_WHITE);        // Draw white text
  display.setCursor(0,0);             // Start at top-left corner
  display.println(F("Hello, world!"));

  display.setTextColor(SSD1306_BLACK, SSD1306_WHITE); // Draw 'inverse' text
  display.println(3.141592);

  display.setTextSize(2);             // Draw 2X-scale text
  display.setTextColor(SSD1306_WHITE);
  display.print(F("0x")); display.println(0xDEADBEEF, HEX);

  display.display();
  delay(2000);
}

void testscrolltext(void) {
  display.clearDisplay();

  display.setTextSize(2); // Draw 2X-scale text
  display.setTextColor(SSD1306_WHITE);
  display.setCursor(10, 0);
  display.println(F("scroll"));
  display.display();      // Show initial text
  delay(100);

  // Scroll in various directions, pausing in-between:
  display.startscrollright(0x00, 0x0F);
  delay(2000);
  display.stopscroll();
  delay(1000);
  display.startscrollleft(0x00, 0x0F);
  delay(2000);
  display.stopscroll();
  delay(1000);
  display.startscrolldiagright(0x00, 0x07);
  delay(2000);
  display.startscrolldiagleft(0x00, 0x07);
  delay(2000);
  display.stopscroll();
  delay(1000);
}

void testdrawbitmap(void) {
  display.clearDisplay();

  display.drawBitmap(
    (display.width()  - LOGO_WIDTH ) / 2,
    (display.height() - LOGO_HEIGHT) / 2,
    logo_bmp, LOGO_WIDTH, LOGO_HEIGHT, 1);
  display.display();
  delay(1000);
}

#define XPOS   0 // Indexes into the 'icons' array in function below
#define YPOS   1
#define DELTAY 2

void testanimate(const uint8_t *bitmap, uint8_t w, uint8_t h) {
  int8_t f, icons[NUMFLAKES][3];

  // Initialize 'snowflake' positions
  for(f=0; f< NUMFLAKES; f++) {
    icons[f][XPOS]   = random(1 - LOGO_WIDTH, display.width());
    icons[f][YPOS]   = -LOGO_HEIGHT;
    icons[f][DELTAY] = random(1, 6);
    Serial.print(F("x: "));
    Serial.print(icons[f][XPOS], DEC);
    Serial.print(F(" y: "));
    Serial.print(icons[f][YPOS], DEC);
    Serial.print(F(" dy: "));
    Serial.println(icons[f][DELTAY], DEC);
  }

  for(;;) { // Loop forever...
    display.clearDisplay(); // Clear the display buffer

    // Draw each snowflake:
    for(f=0; f< NUMFLAKES; f++) {
      display.drawBitmap(icons[f][XPOS], icons[f][YPOS], bitmap, w, h, SSD1306_WHITE);
    }

    display.display(); // Show the display buffer on the screen
    delay(100);        // Pause for 1/10 second

    // Then update coordinates of each flake...
    for(f=0; f< NUMFLAKES; f++) {
      icons[f][YPOS] += icons[f][DELTAY];
      // If snowflake is off the bottom of the screen...
      if (icons[f][YPOS] >= display.height()) {
        // Reinitialize to a random position, just off the top
        icons[f][XPOS]   = random(1 - LOGO_WIDTH, display.width());
        icons[f][YPOS]   = -LOGO_HEIGHT;
        icons[f][DELTAY] = random(1, 6);
      }
    }
  }
}
#endif

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
  static int btn01 = 0x0000;
  static int btn02 = 0x0000;
  
  btn01 = (btn01 << 1);
  btn01 = btn01 & 0xFFFE;

  btn02 = (btn02 << 1);
  btn02 = btn02 & 0xFFFE;

  if( !digitalRead(BTN01_PIN) )
  {
    btn01 = btn01 | 0x0001;
  }

  if( !digitalRead(BTN02_PIN) )
  {
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
    g_EEPEOM.m_szThrLow = g_recvData.throttle;
    g_EEPEOM.m_szThrHigh = g_recvData.throttle;
  }else if( (btn02 & 0xFFFF) ){
    if( g_EEPEOM.m_szThrLow > g_recvData.throttle ){ g_EEPEOM.m_szThrLow = g_recvData.throttle; }
    if( g_EEPEOM.m_szThrHigh < g_recvData.throttle ){ g_EEPEOM.m_szThrHigh = g_recvData.throttle; }

  }else if( (btn02 & 0x8000) == 0x8000 &&
            (btn02 & 0x7FFF) == 0x0000 ){
    writeRomData( &g_EEPEOM );
  }else{
  }


} /* buttonCheck */
