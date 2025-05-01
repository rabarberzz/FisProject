#include "TLBFISLib.h"
#include "FISDrawings.h"

char roundabout[] =   // \x11\x12\x13\x7A\x1D\x1E\x1F\x20\x21\x7A\x2A\x2B\x2C\x2D\x2E\x7A\x39\x3A\x3B\x7A\x41
              "\x11\x12\x13" GRAPHICS_NEWLINE 
          "\x1D\x1E\x1F\x20\x21" GRAPHICS_NEWLINE
          "\x2A\x2B\x2C\x2D\x2E" GRAPHICS_NEWLINE
              "\x39\x3A\x3B" GRAPHICS_NEWLINE
                  "\x41";
char leftTurn[] = // \x30\x33\x33\x7A\x3E\x41\x3A\x7A\x74\x74\x3A
      "\x30\x33\x33" GRAPHICS_NEWLINE
      "\x3E\x41\x3A" GRAPHICS_NEWLINE
      "\x74\x74\x3A";

void FISDrawings::drawScreen()
{
    //Write a message at position X0, Y1.
    FIS.writeText(0, 1, "FIRST LINE");

    //Claim a rectangle at position X5, Y10 with width = 9 and height = 9.
    FIS.setWorkspace(5, 10, 9, 9);

    //Clear the claimed area with the primary color, filling in the rectangle.
    FIS.clear(TLBFISLib::INVERTED);
    
    //You can also set a workspace and clear it in the same instruction like: FIS.setWorkspace(5, 10, 9, 9, true, TLBFISLib::INVERTED);

    //Write a smiley face in that area (on position X2, Y1, where the coordinates start from the top-left corner of the claimed area, not of the screen),
    //setting the text color to TLBFISLib::INVERTED to make it visible, then back to TLBFISLib::NORMAL for the next commands.
    FIS.setDrawColor(TLBFISLib::INVERTED);
    FIS.writeText(2, 1, ":)");
    FIS.setDrawColor(TLBFISLib::NORMAL);

    //Reclaim the entire screen and write a message at position X0, Y41.
    FIS.resetWorkspace();
    FIS.writeText(0, 41, "LAST LINE");

    //Claim only the right side of the screen and write a message there at position X0, Y0.
    //You will notice the message starts from the middle of the screen, as that is the origin X coordinate.
    FIS.setWorkspace(32, 0, 32, 48);
    FIS.writeText(0, 14, "OFFSET");

    //Claim an area and write a message inside of it.
    //You will notice that the text is cut off vertically and horizontally, because
    //1. the height of characters is 7 pixels, and in the workspace only 6 pixels are claimed vertically
    //2. the width of this string is longer than 42 pixels, but only 42 are claimed horizontally
    FIS.setWorkspace(16, 30, 42, 6);
    FIS.writeText(0, 0, "SMALL AREA");
}

void FISDrawings::drawNavigation()
{
    FIS.clear();
    FIS.setFont(TLBFISLib::COMPACT);
    FIS.setLineSpacing(1);
    FIS.drawLine(0, 24, 64);
    FIS.setTextAlignment(TLBFISLib::LEFT);
    FIS.writeText(0, 26, "17:29");
    FIS.writeMultiLineText(0, 34, "78\nKM");
    FIS.setTextAlignment(TLBFISLib::RIGHT);
    FIS.writeMultiLineText(0, 26, "0.7\nKM");
    FIS.setTextAlignment(TLBFISLib::CENTER);
    FIS.writeText(0, 68, "SALDUS 3B");
    FIS.drawLine(0, 24, 74);
    FIS.setFont(TLBFISLib::GRAPHICS);
    FIS.writeMultiLineText(0, 34, leftTurn);
}

void FISDrawings::drawNumbersInRoundabout()
{
    FIS.writeChar(0, 18, '1');
    sleep(1);
    FIS.writeChar(1, 18, '2');
    sleep(1);
    FIS.writeChar(1, 18, '3');
    sleep(1);
    FIS.writeChar(1, 18, '4');
    sleep(1);
    FIS.writeChar(1, 18, '5');
    sleep(1);
    FIS.writeChar(1, 18, '6');
    sleep(1);
    FIS.writeChar(1, 18, '7');
}

void FISDrawings::drawNavigation2()
{
    std::string icon, address, time, total, turn;

    address = "SALDUS 3B";
    time = "17:29";
    total = "78\nKM";
    turn = "0.7\nKM";

    FIS.setWorkspace(0, 0, 64, 55);
    //FIS.drawRect(0, 0, 64, 55, TLBFISLib::NOT_FILLED);
    //FIS.drawLine(0, 0, 64);
    FIS.setTextAlignment(TLBFISLib::CENTER);
    FIS.setFont(TLBFISLib::GRAPHICS);
    FIS.writeMultiLineText(0, 8, roundabout);
    
    FIS.setFont(TLBFISLib::COMPACT);
    FIS.setLineSpacing(1);

    drawNumbersInRoundabout();
    
    FIS.setTextAlignment(TLBFISLib::LEFT);
    FIS.writeText(0, 1, time.c_str());
    FIS.writeMultiLineText(0, 9, total.c_str());

    FIS.setTextAlignment(TLBFISLib::RIGHT);
    FIS.writeMultiLineText(0, 1, turn.c_str());

    FIS.setTextAlignment(TLBFISLib::CENTER);
    FIS.writeText(0, 40, address.c_str());

    

    //FIS.drawLine(0, 55, 64);
    FIS.setFont(TLBFISLib::COMPACT);
    FIS.resetWorkspace();
}

void FISDrawings::drawRuler()
{
    FIS.clear();
    FIS.setFont(TLBFISLib::COMPACT);
    FIS.setLineSpacing(1);
    FIS.setTextAlignment(TLBFISLib::LEFT);
    FIS.writeMultiLineText(0, 0, "0\n8\n16\n24\n32\n40\n48\n56\n64\n72\n80\n88");
}

void FISDrawings::drawTestLayout()
{
    FIS.clear();
    FIS.setWorkspace(0, 24, 64, 55);
    FIS.drawRect(0, 0, 64, 55, TLBFISLib::NOT_FILLED);
    FIS.setFont(TLBFISLib::COMPACT);
    FIS.writeText(2, 2, "TEST LAYOUT");
    FIS.resetWorkspace();
}