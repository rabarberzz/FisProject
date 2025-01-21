#include "BLECallbacks.h"
#include <Arduino.h>
#include <TLBFISLib.h>


void TextCharacteristicCallbacks::onWrite(BLECharacteristic *pCharacteristic) {
    std::string value = pCharacteristic->getValue();
    Serial.printf("Text Characteristic written: %s\n", value.c_str());
    fis.clear();
    fis.writeMultiLineText(0, 1, value.c_str());
}

void TextCharacteristicCallbacks::onNotify(BLECharacteristic *pCharacteristic) {
    std::string value = pCharacteristic->getValue();
    Serial.printf("Text Characteristic notified: %s\n", value.c_str());
    fis.clear();
    fis.writeMultiLineText(0, 1, value.c_str());
}

void NaviCharacteristicCallbacks::onWrite(BLECharacteristic *pCharacteristic) {
    std::string value = pCharacteristic->getValue();
    Serial.printf("Navigation Characteristic written: %s\n", value.c_str());
    // Handle navigation characteristic write
}

void NaviCharacteristicCallbacks::onNotify(BLECharacteristic *pCharacteristic) {
    std::string value = pCharacteristic->getValue();
    Serial.printf("Navigation Characteristic notified: %s\n", value.c_str());
    // Handle navigation characteristic notify
}

void MyBLEServerCallbacks::onConnect(BLEServer* pServer) {
    connected = true;
    Serial.println("BLE client connected");
    pServer->getAdvertising()->stop();
}

void MyBLEServerCallbacks::onDisconnect(BLEServer* pServer) {
    connected = false;
    Serial.println("BLE client disconnected");
    pServer->startAdvertising();
}



