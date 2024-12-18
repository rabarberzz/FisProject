#include <Arduino.h>
#include "BluetoothSerial.h"
#include <TLBFISLib.h>
#include <SPI.h>
#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEScan.h>
#include <BLEAdvertisedDevice.h>

// ==== global definitions ====
#define SPI_INSTANCE SPI
#define ENA_PIN 4
#define SCREEN_SIZE TLBFISLib::FULLSCREEN
#define LED_BUILTIN 2 // Define the built-in LED pin for ESP32

// ==== put function declarations here ====
void sendFunction(uint8_t data);
void beginFunction();
void errorHandler(unsigned long _);
void drawScreen();
void drawNavigation();
void drawRuler();

// ==== definitions ====
BluetoothSerial SerialBT;
char BtInputText[20];
// = BLE =
static BLEUUID serviceUUIDs[] = {
  BLEUUID("0000180a-0000-1000-8000-00805f9b34fb"),
  BLEUUID("f15aaf00-fc20-47c7-a574-9411948aed62")
};
static BLEUUID charUUID("f15aaf01-fc20-47c7-a574-9411948aed62");
static BLEAddress *pServerAddress;
static BLERemoteCharacteristic *pRemoteCharacteristic;
static BLEClient *pClient;
static BLEScan *pBLEScan;

// ==== inits ====
const String DeviceName = "ESP32-BT-Slave";
TLBFISLib FIS(ENA_PIN, sendFunction, beginFunction);
bool sendEntryString = false; // dont forget this
String receivedData = "";
char roundabout[] =
              "\x11\x12\x13" GRAPHICS_NEWLINE
          "\x1D\x1E\x1F\x20\x21" GRAPHICS_NEWLINE
          "\x2A\x2B\x2C\x2D\x2E" GRAPHICS_NEWLINE
              "\x39\x3A\x3B" GRAPHICS_NEWLINE
                  "\x41";
char leftTurn[] =
      "\x30\x33\x33" GRAPHICS_NEWLINE
      "\x3E\x41\x3A" GRAPHICS_NEWLINE
      "\x74\x74\x3A";
bool doConnect = false;
bool connected = false;

// ==== check bt availability ====
#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

// ==== Check Serial Port Profile ====
#if !defined(CONFIG_BT_SPP_ENABLED)
#error Serial Port Profile for Bluetooth is not available or not enabled. It is only available for the ESP32 chip.
#endif

class MyAdvertisedDeviceCallbacks: public BLEAdvertisedDeviceCallbacks {
    void onResult(BLEAdvertisedDevice advertisedDevice) {
      Serial.printf("Advertised Device: %s \n", advertisedDevice.toString().c_str());
      // Check if the advertised device is the one we are looking for
      for (BLEUUID serviceUUID : serviceUUIDs) 
      {
        if (advertisedDevice.haveServiceUUID() && (advertisedDevice.getServiceUUID().equals(serviceUUID) ))
        {
          advertisedDevice.getScan()->stop();
          pServerAddress = new BLEAddress(advertisedDevice.getAddress());
          doConnect = true;
          Serial.println("Device found. Connecting...");
        }
      }
    }
};

void setup()
{
  // setup serial
  Serial.begin(115200);

  // setup bt serial
  SerialBT.begin(DeviceName);
  //SerialBT.deleteAllBondedDevices(); // Uncomment this to delete paired devices; Must be called after begin
  Serial.printf("The device with name \"%s\" is started.\nNow you can pair it with Bluetooth!\n", DeviceName.c_str());

  //Start the library and initialize the screen.
  FIS.begin();
  FIS.initScreen(SCREEN_SIZE);

  drawRuler();

  /* //Set font options, which will persist for every subsequent text command.
  FIS.setFont(TLBFISLib::COMPACT);
  FIS.setTextAlignment(TLBFISLib::CENTER);
  FIS.setLineSpacing(3);

  //Write the message at position X0, Y1.
  FIS.writeText(0, 0, "READY");

  FIS.setFont(TLBFISLib::GRAPHICS);
  FIS.writeMultiLineText(0, 25, roundabout);
  FIS.setFont(TLBFISLib::COMPACT); */

  // init ble
  BLEDevice::init(DeviceName.c_str());
  pBLEScan = BLEDevice::getScan();
  pBLEScan->setAdvertisedDeviceCallbacks(new MyAdvertisedDeviceCallbacks());
  pBLEScan->setActiveScan(true);
  pBLEScan->start(30);
}

void loop()
{
  // ble stuff
  if (doConnect)
  {
    pClient = BLEDevice::createClient();
    pClient->connect(*pServerAddress);
    Serial.println("Connected to server");
    pRemoteCharacteristic = pClient->getService(serviceUUIDs[1])->getCharacteristic(charUUID);
    // Set the ESP32 LED on
    pinMode(LED_BUILTIN, OUTPUT);
    digitalWrite(LED_BUILTIN, HIGH);
    if (pRemoteCharacteristic->canRead())
    {
      std::string value = pRemoteCharacteristic->readValue();
      Serial.printf("Start Characteristic value: %s\n", value.c_str());
    }

    pRemoteCharacteristic->registerForNotify([](BLERemoteCharacteristic* pBLERemoteCharacteristic, uint8_t* pData, size_t length, bool isNotify)
    {
      std::string value((char*)pData, length);
      Serial.printf("Characteristic value: %s\n", value.c_str());
      FIS.clear();
      FIS.writeMultiLineText(0, 1, value.c_str());
    });
    doConnect = false;
    connected = true;
    }

  // fis disruption handler handle
  //FIS.errorFunction(errorHandler);
  // serial prompt
  if (sendEntryString)
  {
    Serial.println("Try to input something");
    SerialBT.println("Try to input something");
    sendEntryString = false;
  }
  
  // check if bt client has sent something
  //while (SerialBT.available())
  while (false)
  {
    char incomingChar = SerialBT.read();

    if (incomingChar == '\n' || incomingChar == '\r')
    {
      // write new line
      SerialBT.write('\n');

      // convert to uppercase
      //receivedData.toUpperCase();
      receivedData.toCharArray(BtInputText, sizeof(BtInputText));
      SerialBT.println("Tried to write: " + receivedData);

      FIS.clear(); // clear screen before writing, otherwise it overwrites
      FIS.writeMultiLineText(0, 1, BtInputText); // write to fis

      receivedData = ""; // clear buffer
      sendEntryString = true; // allow entry string
    }
    else
    {
      receivedData += incomingChar;
      Serial.write(incomingChar);
      SerialBT.write(incomingChar);
    }
  }

  //Maintain the connection.
  FIS.update();
}

// ==== functions ====
//Define the function to be called when the library needs to send a byte.
void sendFunction(uint8_t data)
{
  SPI_INSTANCE.beginTransaction(SPISettings(125000, MSBFIRST, SPI_MODE3));
  SPI_INSTANCE.transfer(data);
  SPI_INSTANCE.endTransaction();
}

//Define the function to be called when the library is initialized by begin().
void beginFunction()
{
  SPI_INSTANCE.begin();
}

void errorHandler(unsigned long _)
{
  (void) _;

  FIS.initScreen(SCREEN_SIZE);
  FIS.writeMultiLineText(0, 0, "SCREEN GOT\nDISRUPTED");
}

void drawScreen()
{
  //Write a message at position X0, Y1.
  FIS.writeText(0, 1, "FIRST LINE");

  //Claim a rectangle at position X5, Y10 with width = 9 and height = 9.
  FIS.setWorkspace(5, 10, 9, 9);

  //Clear the claimed area with the primary color, filling in the rectangle.
  FIS.clear(TLBFISLib::INVERTED);
  
  //You can also set a workspace and clear it in the same instruction like: FIS.setWorkspace(5, 10, 9, 9, true, TLBFISLib::INVERTED);

  //Write a smiley face in that area (on position X2, Y1, where the coordinates start from the top-left corner of the claimed area, not of the screen),
  //setting the text color to TLBFISLib::INVERTED to make it visible, then back to TLBFISLib::NORMAL for the next commands.
  FIS.setDrawColor(TLBFISLib::INVERTED);
  FIS.writeText(2, 1, ":)");
  FIS.setDrawColor(TLBFISLib::NORMAL);

  //Reclaim the entire screen and write a message at position X0, Y41.
  FIS.resetWorkspace();
  FIS.writeText(0, 41, "LAST LINE");

  //Claim only the right side of the screen and write a message there at position X0, Y0.
  //You will notice the message starts from the middle of the screen, as that is the origin X coordinate.
  FIS.setWorkspace(32, 0, 32, 48);
  FIS.writeText(0, 14, "OFFSET");

  //Claim an area and write a message inside of it.
  //You will notice that the text is cut off vertically and horizontally, because
  //1. the height of characters is 7 pixels, and in the workspace only 6 pixels are claimed vertically
  //2. the width of this string is longer than 42 pixels, but only 42 are claimed horizontally
  FIS.setWorkspace(16, 30, 42, 6);
  FIS.writeText(0, 0, "SMALL AREA");
}

void drawNavigation()
{
  FIS.clear();
  FIS.setFont(TLBFISLib::COMPACT);
  FIS.setLineSpacing(1);
  FIS.drawLine(0, 24, 64);
  FIS.setTextAlignment(TLBFISLib::LEFT);
  FIS.writeText(0, 26, "17:29");
  FIS.writeMultiLineText(0, 34, "78\nKM");
  FIS.setTextAlignment(TLBFISLib::RIGHT);
  FIS.writeMultiLineText(0, 26, "0.7\nKM");
  FIS.setTextAlignment(TLBFISLib::CENTER);
  FIS.writeText(0, 68, "SALDUS 3B");
  FIS.drawLine(0, 24, 74);
  FIS.setFont(TLBFISLib::GRAPHICS);
  FIS.writeMultiLineText(0, 34, leftTurn);
}

void drawRuler()
{
  FIS.clear();
  FIS.setFont(TLBFISLib::COMPACT);
  FIS.setLineSpacing(1);
  FIS.setTextAlignment(TLBFISLib::LEFT);
  FIS.writeMultiLineText(0, 0, "0\n8\n16\n24\n32\n40\n48\n56\n64\n72\n80\n88");
}
