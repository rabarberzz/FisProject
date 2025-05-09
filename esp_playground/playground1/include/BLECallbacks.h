#include <BLEDevice.h>
#include <BLEServer.h>
#include <TLBFISLib.h>
#include <Preferences.h>

#define NAVI_DATA_CODING "/icon_{0}/address_{1}/time_{2}/total_{3}/turn_{4}";

class TextCharacteristicCallbacks: public BLECharacteristicCallbacks {
private:
    TLBFISLib& fis;

public:
    TextCharacteristicCallbacks(TLBFISLib& fis): fis(fis) {}
    void onWrite(BLECharacteristic *pCharacteristic) override;
    void onNotify(BLECharacteristic *pCharacteristic) override;
};

class NaviCharacteristicCallbacks: public BLECharacteristicCallbacks {
private:
    TLBFISLib& fis;
    bool& navi_enabled; // Flag to indicate if navigation is enabled
    std::function<void()> fisErrorSetter;
    
public:
    NaviCharacteristicCallbacks(TLBFISLib& fis, bool& navi_enabled, std::function<void()> fisErrorSetter): fis(fis), navi_enabled(navi_enabled), fisErrorSetter(fisErrorSetter) {}
    void onWrite(BLECharacteristic *pCharacteristic) override;
    void onNotify(BLECharacteristic *pCharacteristic) override;
};

class ConfigCharacteristicCallbacks: public BLECharacteristicCallbacks {
private:
    float& speed_ratio;
    Preferences& preferences;
    std::function<void(bool)> callback;

public:
    ConfigCharacteristicCallbacks(float& speedRatio, Preferences& prefs,std::function<void(bool)> cb)
    : speed_ratio(speedRatio), preferences(prefs), callback(cb) {}
    void onWrite(BLECharacteristic *pCharacteristic) override;
};

class MyBLEServerCallbacks: public BLEServerCallbacks {
private:
    bool& connected;

public:
    MyBLEServerCallbacks(bool& connected): connected(connected) {}
    void onConnect(BLEServer* pServer) override;
    void onDisconnect(BLEServer* pServer) override;
};