#include "BLECallbacks.h"
#include <Arduino.h>
#include <TLBFISLib.h>
#include <sstream>
#include <vector>

// Private function declarations
void writeNaviTextInWorkspace(const std::string& value, TLBFISLib& fis);
void writeRadioTextInWorkspace(const std::string& value, TLBFISLib& fis);
std::vector<std::string> parseIncomingData(const std::string& s, char delimiter);
void parseIncomingNaviData(const std::string& data, std::string& icon, std::string& address, 
    std::string& time, std::string& total, std::string& turn, std::string& exit, std::string& clear);
void parseIncomingConfigData(const std::string& data, std::string& speedEna, std::string& speedRatio);

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

    if (value == "clear" && navi_enabled == true)
    {
        fis.turnOff();
        navi_enabled = false;
        return;
    }

    if (value == "clear" && navi_enabled == false)
    {
        return;
    }

    if (value != "clear" && navi_enabled == false)
    {
        fis.initScreen();
        fis.clear();
        navi_enabled = true;
    }
    
    writeNaviTextInWorkspace(value, fis);
}

void NaviCharacteristicCallbacks::onNotify(BLECharacteristic *pCharacteristic) {
    std::string value = pCharacteristic->getValue();
    Serial.printf("Navigation Characteristic notified: %s\n", value.c_str());

    writeNaviTextInWorkspace(value, fis);
}

void ConfigCharacteristicCallbacks::onWrite(BLECharacteristic *pCharacteristic) {
    std::string value = pCharacteristic->getValue();
    Serial.printf("Config Characteristic written: %s\n", value.c_str());
    
    std::string speedEna, speedRatio;
    parseIncomingConfigData(value, speedEna, speedRatio);

    try {
        speed_ratio = std::stof(speedRatio);
    } catch (const std::invalid_argument& e) {
        Serial.println("Invalid speed ratio value received.");
        return;
    } catch (const std::out_of_range& e) {
        Serial.println("Speed ratio value out of range.");
        return;
    }

    bool newEnabled = speedEna == "1" ? true : false;

    if (callback)
    {
        callback(newEnabled);
    }

    preferences.putBool("speed_enabled", newEnabled);
    preferences.putFloat("speed_ratio", speed_ratio);
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
    std::string icon, address, time, total, turn, exit, clear;
    parseIncomingNaviData(value, icon, address, time, total, turn, exit, clear);

    if (clear == "true")
    {
        fis.setWorkspace(0, 0, 64, 55, true);
    }
    else
    {
        fis.setWorkspace(0, 0, 64, 55);
    }

    fis.setTextAlignment(TLBFISLib::CENTER);
    fis.setFont(TLBFISLib::GRAPHICS);
    fis.writeMultiLineText(0, 8, icon.c_str());

    fis.setFont(TLBFISLib::COMPACT);
    fis.setLineSpacing(1);

    if (exit != "0")
    {
        if (exit == "1")
        {
            fis.writeChar(0, 18, exit[0]);
        }
        else
        {
            fis.writeChar(0, 18, exit[0]);
        }
    }
    

    fis.setTextAlignment(TLBFISLib::LEFT);
    fis.writeText(0, 1, time.c_str());
    fis.writeMultiLineText(0, 9, total.c_str());

    fis.setTextAlignment(TLBFISLib::RIGHT);
    fis.writeMultiLineText(0, 1, turn.c_str());

    fis.setTextAlignment(TLBFISLib::CENTER);
    fis.writeText(0, 40, address.c_str());

    fis.resetWorkspace();
}

void writeRadioTextInWorkspace(const std::string& value, TLBFISLib& fis) {
    fis.setWorkspace(0, 0, 64, 24);
    fis.clear();
    fis.setFont(TLBFISLib::STANDARD);
    fis.writeMultiLineText(1, 1, value.c_str());
    fis.resetWorkspace();
}

std::vector<std::string> parseIncomingData(const std::string& s, char delimiter) {
    std::vector<std::string> tokens;
    std::string token;
    std::istringstream tokenStream(s);
    while (std::getline(tokenStream, token, delimiter)) {
        tokens.push_back(token);
    }
    return tokens;
}

void parseIncomingNaviData(const std::string& data, std::string& icon, 
    std::string& address, std::string& time, std::string& total, 
    std::string& turn, std::string& exit, std::string& clear) {
    std::vector<std::string> tokens = parseIncomingData(data, '/');
    Serial.println(tokens.size());
    if (tokens.size() == 7) {
        icon = tokens[0].substr(5); //substr removes prefixes like "icon_"
        address = tokens[1].substr(8);
        time = tokens[2].substr(5);
        total = tokens[3].substr(6);
        turn = tokens[4].substr(5);
        exit = tokens[5].substr(5);
        clear = tokens[6].substr(6);
    }
}

void parseIncomingConfigData(const std::string& data, std::string& speedEna, std::string& speedRatio) {
    std::vector<std::string> tokens = parseIncomingData(data, '/');

    if (tokens.size() == 2) {
        speedEna = tokens[0].substr(6); //substr removes prefix "speed_"
        speedRatio = tokens[1].substr(6); //substr removes prefix "ratio_"
    }
}
