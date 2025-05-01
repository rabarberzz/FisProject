#include <BLEDevice.h>
#include <BLEServer.h>
#include <TLBFISLib.h>

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
    
public:
    NaviCharacteristicCallbacks(TLBFISLib& fis, bool& navi_enabled): fis(fis), navi_enabled(navi_enabled) {}
    void onWrite(BLECharacteristic *pCharacteristic) override;
    void onNotify(BLECharacteristic *pCharacteristic) override;
};

class ConfigCharacteristicCallbacks: public BLECharacteristicCallbacks {
    public:
    ConfigCharacteristicCallbacks() {}
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