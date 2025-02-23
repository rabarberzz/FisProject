#include "BLECallbacks.h"
#include <Arduino.h>
#include <TLBFISLib.h>
#include <sstream>
#include <vector>

// Private function declarations
void writeNaviTextInWorkspace(const std::string& value, TLBFISLib& fis);
void writeRadioTextInWorkspace(const std::string& value, TLBFISLib& fis);
std::vector<std::string> splitIncomingNaviData(const std::string& s, char delimiter);
void parseIncomingNaviData(const std::string& data, std::string& icon, std::string& address, std::string& time, std::string& total, std::string& turn);

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
    std::string icon, address, time, total, turn;
    parseIncomingNaviData(value, icon, address, time, total, turn);

    Serial.println("Icon: ");
    Serial.print(icon.c_str());
    Serial.println("Address: ");
    Serial.print(address.c_str());
    Serial.println("Time: ");
    Serial.print(time.c_str());
    Serial.println("Total: ");
    Serial.print(total.c_str());
    Serial.println("Turn: ");
    Serial.print(turn.c_str());

    fis.setWorkspace(0, 24, 64, 55, true);
    fis.drawRect(0, 0, 64, 55, TLBFISLib::NOT_FILLED);
    fis.setFont(TLBFISLib::COMPACT);
    fis.setLineSpacing(1);

    fis.setTextAlignment(TLBFISLib::LEFT);
    fis.writeText(2, 2, time.c_str());
    fis.writeMultiLineText(2, 10, total.c_str());

    fis.setTextAlignment(TLBFISLib::RIGHT);
    fis.writeMultiLineText(-1, 2, turn.c_str());

    fis.setTextAlignment(TLBFISLib::CENTER);
    fis.writeText(0, 46, address.c_str());

    fis.setFont(TLBFISLib::GRAPHICS);
    fis.writeMultiLineText(0, 10, icon.c_str());

    fis.resetWorkspace();
}

void writeRadioTextInWorkspace(const std::string& value, TLBFISLib& fis) {
    fis.setWorkspace(0, 0, 64, 24);
    fis.clear();
    fis.setFont(TLBFISLib::STANDARD);
    fis.writeMultiLineText(1, 1, value.c_str());
    fis.resetWorkspace();
}

std::vector<std::string> splitIncomingNaviData(const std::string& s, char delimiter) {
    std::vector<std::string> tokens;
    std::string token;
    std::istringstream tokenStream(s);
    while (std::getline(tokenStream, token, delimiter)) {
        tokens.push_back(token);
    }
    return tokens;
}

void parseIncomingNaviData(const std::string& data, std::string& icon, std::string& address, std::string& time, std::string& total, std::string& turn) {
    std::vector<std::string> tokens = splitIncomingNaviData(data, '/');
    Serial.println(tokens.size());
    if (tokens.size() == 5) {
        icon = tokens[0].substr(5); //substr removes prefixes like "icon_"
        address = tokens[1].substr(8);
        time = tokens[2].substr(5);
        total = tokens[3].substr(6);
        turn = tokens[4].substr(5);
    }
}
