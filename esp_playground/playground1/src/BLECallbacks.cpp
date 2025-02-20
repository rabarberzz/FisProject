#include "BLECallbacks.h"
#include <Arduino.h>
#include <TLBFISLib.h>

// Private function declarations
void writeNaviTextInWorkspace(const std::string& value, TLBFISLib& fis);
void writeRadioTextInWorkspace(const std::string& value, TLBFISLib& fis);

void TextCharacteristicCallbacks::onWrite(BLECharacteristic *pCharacteristic) {
    std::string value = pCharacteristic->getValue();
    Serial.printf("Text Characteristic written: %s\n", value.c_str());
    
    writeRadioTextInWorkspace(value, fis);
}

void TextCharacteristicCallbacks::onNotify(BLECharacteristic *pCharacteristic) {
    std::string value = pCharacteristic->getValue();
    Serial.printf("Text Characteristic notified: %s\n", value.c_str());
    
    writeRadioTextInWorkspace(value, fis);
}

void NaviCharacteristicCallbacks::onWrite(BLECharacteristic *pCharacteristic) {
    std::string value = pCharacteristic->getValue();
    Serial.printf("Navigation Characteristic written: %s\n", value.c_str());
    
    writeNaviTextInWorkspace(value, fis);
}

void NaviCharacteristicCallbacks::onNotify(BLECharacteristic *pCharacteristic) {
    std::string value = pCharacteristic->getValue();
    Serial.printf("Navigation Characteristic notified: %s\n", value.c_str());
    
    writeNaviTextInWorkspace(value, fis);
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

void writeNaviTextInWorkspace(const std::string& value, TLBFISLib& fis) {
    fis.setWorkspace(0, 24, 64, 55);
    fis.clear();
    fis.drawRect(0, 0, 64, 55, TLBFISLib::NOT_FILLED);
    fis.setFont(TLBFISLib::GRAPHICS);
    fis.writeMultiLineText(1, 1, value.c_str());
    fis.resetWorkspace();
}

void writeRadioTextInWorkspace(const std::string& value, TLBFISLib& fis) {
    fis.setWorkspace(0, 0, 64, 24);
    fis.clear();
    fis.setFont(TLBFISLib::STANDARD);
    fis.writeMultiLineText(1, 1, value.c_str());
    fis.resetWorkspace();
}
