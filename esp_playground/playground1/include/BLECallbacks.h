#include <BLEDevice.h>
#include <BLEServer.h>
#include <TLBFISLib.h>

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