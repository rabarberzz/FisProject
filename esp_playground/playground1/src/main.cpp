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

// ==== definitions ====
// = BLE =
static BLEUUID serviceUUID("f15aaf00-fc20-47c7-a574-9411948aed62"); // device/service UUID
static BLEUUID charUUID("f15aaf01-fc20-47c7-a574-9411948aed62"); // text characteristic UUID
static BLEUUID naviUUID("f15aaf02-fc20-47c7-a574-9411948aed62"); // navigation characteristic UUID
static BLEUUID configUUID("f15aaf03-fc20-47c7-a574-9411948aed62"); // configuration characteristic UUID
const String DeviceName = "ESP32-BT-Server";
bool connected = false;
bool navi_enabled = true; // Flag to indicate if navigation is enabled
bool speed_enabled = true;


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

  // setup led pin mode
  pinMode(LED_BUILTIN, OUTPUT);

  // setup pulse pin mode
  pinMode(PULSE_PIN, INPUT);

  //Start TLBFISLib and initialize the screen.
  FIS.begin();
  //FIS.initScreen(SCREEN_SIZE);
  //FIS.clear();
  //FIS.writeRadioText(0, "TEST");
  //FIS.errorFunction(errorHandler); // fis disruption handler handle
  FIS.setTextAlignment(TLBFISLib::CENTER);
  FIS.setFont(TLBFISLib::COMPACT);
  FIS.clearRadioText(); // Clear the radio text area (turns off that section)
  Serial.println("TLBFISLib initialized.");

  // test drawings
  //FISDraw.drawRuler();
  //drawTestLayout();
  //drawNavigation();
  //FISDraw.drawNavigation2();

  pcnt_init_and_start(); // Initialize and start the pulse counter

  init_hw_timer(); // Initialize the hardware timer

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

  // counting works now :)
  /*int16_t count;
  pcnt_init_and_start();
  delay(1000);
  unsigned long lastMillis = 0;
  
  pcnt_get(&count);

  if (count > 0){
    Serial.printf("Pulses: %d\n", count);
    drawSpeedFromPulses(count, true);
  }
  pcnt_clear();
  if (millis() - lastMillis >= 1000) {
    lastMillis = millis();
  }*/
  
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

void errorHandler(unsigned long _)
{
  (void) _;
  FIS.initScreen(SCREEN_SIZE);
  FIS.writeMultiLineText(0, 0, "SCREEN GOT\nDISRUPTED");
  Serial.println("FIS error handler called.");
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
  naviCharacteristic->setCallbacks(new NaviCharacteristicCallbacks(FIS, navi_enabled)); // Register the navigation callback

  BLECharacteristic *configCharacteristic = pService->createCharacteristic(
    configUUID,
    BLECharacteristic::PROPERTY_READ |
    BLECharacteristic::PROPERTY_WRITE |
    BLECharacteristic::PROPERTY_NOTIFY
  );
  configCharacteristic->setCallbacks(new ConfigCharacteristicCallbacks()); // Register the config callback

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
    //FIS.clearRadioText();
    String speedString = String(pulses);
    FIS.writeRadioText(0, speedString.c_str());
    FIS.writeRadioText(1, "KM/H");
}
