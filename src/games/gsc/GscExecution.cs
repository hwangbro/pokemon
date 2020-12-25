using System;
using System.Collections.Generic;

public static class GscExecution {

    public static void ClearText(this Crystal gb, byte lastSlot = 1) {
        int ret;
        // disablelcd?
        while((ret = gb.Hold(Joypad.B, gb.SYM["PromptButton"], gb.SYM["BattleMenu"], gb.SYM["InitPartyMenuWithCancel"], gb.SYM["GiveExperiencePoints.skip_exp_bar_animation"])) == gb.SYM["PromptButton"]) {
            gb.Hold(Joypad.B, "GetJoypad");
            gb.InjectMenu(Joypad.A | Joypad.B);

            if(gb.CpuRead("wBattleEnded") == 1) {
                return;
            }
        }
        gb.RunUntil("GetJoypad");
        gb.AdvanceFrame();
    }

    public static void Swap(this Crystal gb, int targetPokeIndex, bool dead = false) {
        int curSlot = Math.Max(gb.CpuRead("wPartyMenuCursor"), (byte) 1);
        Joypad joypad = targetPokeIndex > curSlot ? Joypad.Down : Joypad.Up;
        int numScrolls = Math.Abs(curSlot - targetPokeIndex);

        if(!dead) {
            gb.OpenPkmnMenu();
            gb.Hold(joypad, "InitPartyMenuWithCancel");
        }

        gb.RunUntil("GetJoypad");

        for(int i = 0; i < numScrolls; i++) {
            gb.ScrollBag(joypad);
        }
        gb.Press(Joypad.A, Joypad.None, Joypad.A);
    }

    public static void UseMove(this Crystal gb, int slot, int numMoves = 4) {
        gb.OpenFightMenu();
        int currentSlot = gb.CpuRead("wCurMoveNum") + 1;
        int difference = currentSlot - slot;
        int numSlots = difference == 0 ? 0 : slot % 2 == currentSlot % 2 ? (int)(numMoves/2) : 1;
        Joypad joypad = (((Math.Abs(difference * numMoves) + difference) % numMoves) & 2) != 0 ? Joypad.Down : Joypad.Up;
        switch(numSlots) {
            case 0: gb.Press(Joypad.None); break;
            case 1: gb.Press(joypad); break;
            case 2: gb.Press(joypad, Joypad.None, joypad); break;
            default: gb.Press(Joypad.None); break;
        }
        gb.Press(Joypad.A);
    }

    public static void UseItem(this Crystal gb, string name, int targetPokeIndex = -1) {
        Dictionary<string, byte> bag = gb.GetBag();
        if(!bag.ContainsKey(name)) {
            throw new Exception($"Item {name} does not exist in bag");
        }
        gb.ScrollToItem(bag[name]);
        gb.Press(Joypad.A, Joypad.None, Joypad.A);

        if(targetPokeIndex != -1) {
            gb.RunUntil("InitPartyMenuWithCancel");
            gb.AdvanceFrame();
            gb.RunUntil("GetJoypad");
            gb.AdvanceFrame();
            gb.RunUntil("GetJoypad");
            gb.Press(Joypad.A);
        }

    }

    public static void ScrollToItem(this Crystal gb, int slot) {
        int curBagPos = Math.Max(gb.CpuRead("wItemsPocketCursor"), (byte) 1);
        int curScreenScroll = gb.CpuRead("wItemsPocketScrollPosition");
        int curSlot = curBagPos + curScreenScroll;
        Joypad joypad = slot > curSlot ? Joypad.Down : Joypad.Up;
        int numScrolls = Math.Abs(curSlot - slot);

        gb.OpenItemBag();
        gb.Hold(joypad, "ScrollingMenu");
        gb.RunUntil("GetJoypad");

        for(int i = 0; i < numScrolls; i++) {
            gb.ScrollBag(joypad);
        }
    }

    public static Dictionary<string, byte> GetBag(this Crystal gb) {
        Dictionary<string, byte> bag = new Dictionary<string, byte>();

        int addr = gb.SYM["wItems"];
        byte index = 1;
        while(gb.CpuRead(addr) != 0xFF) {
            GscItem item = gb.Items[gb.CpuRead(addr++) - 1];
            byte quantity = gb.CpuRead(addr++);
            bag[item.Name] = index++;
        }

        return bag;
    }

    public static void ScrollBag(this Crystal gb, Joypad joypad) {
        gb.Hold(joypad, "GetJoypad");
        gb.InjectMenu(joypad);
        gb.AdvanceFrame();
    }

    public static void OpenFightMenu(this Crystal gb) {
        if(gb.CpuRead("wMenuCursorY") == 2) gb.Press(Joypad.Up);
        if(gb.CpuRead("wMenuCursorX") == 2) gb.Press(Joypad.Left);
        gb.Press(Joypad.A);
    }

    public static void OpenPkmnMenu(this Crystal gb) {
        if(gb.CpuRead("wMenuCursorY") == 2) gb.Press(Joypad.Up);
        if(gb.CpuRead("wMenuCursorX") == 1) gb.Press(Joypad.Right);
        gb.Press(Joypad.A);
    }

    public static void OpenItemBag(this Crystal gb) {
        if(gb.CpuRead("wMenuCursorY") == 1) gb.Press(Joypad.Down);
        if(gb.CpuRead("wMenuCursorX") == 2) gb.Press(Joypad.Left);
        gb.Press(Joypad.A);
    }
}