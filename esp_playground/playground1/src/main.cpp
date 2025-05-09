#include <Arduino.h>
#include "BluetoothSerial.h"
#include <TLBFISLib.h>
#include <SPI.h>
#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEServer.h>
#include <BLECallbacks.h>
#include <FISDrawings.h>
#include <hw_timer.h>
#include <Preferences.h>

// ==== global definitions ====
#define SPI_INSTANCE SPI
#define ENA_PIN 4
#define SCREEN_SIZE TLBFISLib::HALFSCREEN
#define LED_BUILTIN 2 // Define the built-in LED pin for ESP32
#define PULSE_PIN 21 // Pin where the pulses will be sent
#define PCNT_UNIT PCNT_UNIT_0 // Use PCNT unit 0

// ==== put function declarations here ====
void sendFunction(uint8_t data);
void beginFunction();
void errorHandler(unsigned long _);
//void drawSpeedFromPulses(int16_t pulses); // Function to draw speed from pulse count
void initBLE();
void onConfigChangedCallback(bool newSpeedEnabled);
void setUpErrorHandler();

// ==== definitions ====
// = BLE =
static BLEUUID serviceUUID("f15aaf00-fc20-47c7-a574-9411948aed62"); // device/service UUID
static BLEUUID charUUID("f15aaf01-fc20-47c7-a574-9411948aed62"); // text characteristic UUID
static BLEUUID naviUUID("f15aaf02-fc20-47c7-a574-9411948aed62"); // navigation characteristic UUID
static BLEUUID configUUID("f15aaf03-fc20-47c7-a574-9411948aed62"); // configuration characteristic UUID
const String DeviceName = "ESP32-BT-Server";
bool connected = false;
bool navi_enabled = false; // Flag to indicate if navigation is enabled
bool speed_enabled; // Speed output enabled
float speed_ratio; // Speed ratio for conversion
Preferences preferences;


// ==== inits ====
TLBFISLib FIS(ENA_PIN, sendFunction, beginFunction);
FISDrawings FISDraw(FIS); // Create an instance of the FISDrawings class

// ==== check bt availability ====
#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

// ==== Check Serial Port Profile ====
#if !defined(CONFIG_BT_SPP_ENABLED)
#error Serial Port Profile for Bluetooth is not available or not enabled. It is only available for the ESP32 chip.
#endif

void setup()
{
  // setup serial
  Serial.begin(115200);

  //setup persistance / preferences
  preferences.begin("FisControl", false); // Initialize preferences with a namespace

  speed_ratio = preferences.getFloat("speed_ratio", 0.900); 
  speed_enabled = preferences.getBool("speed_enabled", true);

  // setup led pin mode
  pinMode(LED_BUILTIN, OUTPUT);

  // setup pulse pin mode
  pinMode(PULSE_PIN, INPUT);

  //Start TLBFISLib and initialize the screen.
  FIS.begin();
  //FIS.initScreen(SCREEN_SIZE);
  //FIS.clear();
  //FIS.writeRadioText(0, "TEST");
  FIS.errorFunction(errorHandler); // fis disruption handler handle
  FIS.clearRadioText(); // Clear the radio text area (turns off that section)
  Serial.println("TLBFISLib initialized.");

  // test drawings
  //FISDraw.drawRuler();
  //drawTestLayout();
  //drawNavigation();
  //FISDraw.drawNavigation2();

  pcnt_init_and_start(); // Initialize the pulse counter

  init_hw_timer(); // Initialize the hardware timer

  if (speed_enabled)
  {
    pcnt_start();
    resume_counter_task();
  }

  initBLE(); // Initialize BLE service
}

void loop()
{
  // ble stuff
  if (connected)
  {
    // set led on when client connected
    digitalWrite(LED_BUILTIN, HIGH);
  }
  else
  {
    // set led off when client disconnected
    digitalWrite(LED_BUILTIN, LOW);
  }

  //Maintain the FIS connection.
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

void setUpErrorHandler()
{
  FIS.errorFunction(errorHandler); // Set the error handler function
}

void errorHandler(unsigned long errorMs)
{
  if (navi_enabled)
  {
    FIS.initScreen(SCREEN_SIZE);
    FIS.clear();
  } 
  else 
  {
    FIS.turnOff(); // Return to the "trip computer" mode
  }
  
  //FIS.writeMultiLineText(0, 0, "SCREEN GOT\nDISRUPTED");
  Serial.println("FIS error handler called: " + String(errorMs) + " ms");
}

void initBLE()
{
  // setup ble
  Serial.println("Starting BLE service");
  BLEDevice::init(DeviceName.c_str());
  BLEServer *pServer = BLEDevice::createServer();
  pServer->setCallbacks(new MyBLEServerCallbacks(connected));

  BLEService *pService = pServer->createService(serviceUUID);

  BLECharacteristic *textCharacteristic = pService->createCharacteristic(
    charUUID,
    BLECharacteristic::PROPERTY_READ |
    BLECharacteristic::PROPERTY_WRITE |
    BLECharacteristic::PROPERTY_NOTIFY
  );
  textCharacteristic->setValue("BLE TEXT INIT");
  textCharacteristic->setCallbacks(new TextCharacteristicCallbacks(FIS)); // Register the text callback

  BLECharacteristic *naviCharacteristic = pService->createCharacteristic(
    naviUUID,
    BLECharacteristic::PROPERTY_READ |
    BLECharacteristic::PROPERTY_WRITE |
    BLECharacteristic::PROPERTY_NOTIFY
  );
  naviCharacteristic->setCallbacks(new NaviCharacteristicCallbacks(FIS, navi_enabled, setUpErrorHandler)); // Register the navigation callback

  BLECharacteristic *configCharacteristic = pService->createCharacteristic(
    configUUID,
    BLECharacteristic::PROPERTY_READ |
    BLECharacteristic::PROPERTY_WRITE |
    BLECharacteristic::PROPERTY_NOTIFY
  );
  String configValue = "speed_" + String(speed_enabled) + "/ratio_" + String(speed_ratio, 3);
  configCharacteristic->setValue(configValue.c_str()); // Register the config callback
  configCharacteristic->setCallbacks(new ConfigCharacteristicCallbacks(speed_ratio, preferences, onConfigChangedCallback));

  pService->start();

  BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
  pAdvertising->addServiceUUID(serviceUUID);
  pAdvertising->setScanResponse(true);
  pAdvertising->setMinPreferred(0x06);  // functions that help with iPhone connections issue
  pAdvertising->setMinPreferred(0x12);
  BLEDevice::startAdvertising();

  Serial.println("BLE service started and advertising.");
}

void drawSpeedFromPulses(int16_t pulses)
{
    float speed = pulses * speed_ratio; // Convert pulses to speed (example conversion factor)
    int roundedSpeed = round(speed); // Round the speed to the nearest integer
    //FIS.clearRadioText();
    String speedString = String(roundedSpeed);
    FIS.writeRadioText(0, speedString.c_str());
    FIS.writeRadioText(1, "KM/H");
}

void onConfigChangedCallback(bool newSpeedEnabled)
{
  if (speed_enabled != newSpeedEnabled)
  {
    if (newSpeedEnabled)
    {
      pcnt_start();
      resume_counter_task();
    }
    else
    {
      pcnt_stop();
      pause_counter_task();
      FIS.clearRadioText(); // Clear the radio text area (turns off that section)
    }
  }
  speed_enabled = newSpeedEnabled; // Update the speed_enabled variable
}
