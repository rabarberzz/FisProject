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
    
public:
    NaviCharacteristicCallbacks(TLBFISLib& fis): fis(fis) {}
    void onWrite(BLECharacteristic *pCharacteristic) override;
    void onNotify(BLECharacteristic *pCharacteristic) override;
};

class MyBLEServerCallbacks: public BLEServerCallbacks {
private:
    bool& connected;

public:
    MyBLEServerCallbacks(bool& connected): connected(connected) {}
    void onConnect(BLEServer* pServer) override;
    void onDisconnect(BLEServer* pServer) override;
};