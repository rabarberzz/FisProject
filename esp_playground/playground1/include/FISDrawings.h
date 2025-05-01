#define FISDRAWINGS_H

#include "TLBFISLib.h"
#include "characters.h"
#include <string>

class FISDrawings {
private:
    TLBFISLib& FIS;

public:
    // Constructor
    FISDrawings(TLBFISLib& fis) : FIS(fis) {}

    // Drawing functions
    void drawScreen();
    void drawNavigation();
    void drawNavigation2();
    void drawNumbersInRoundabout();
    void drawRuler();
    void drawTestLayout();
};