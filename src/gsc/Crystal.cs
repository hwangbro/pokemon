using System;
using System.Collections.Generic;

public class Crystal : Gsc {

    public Crystal(bool speedup = false, string rom = "roms/pokecrystal.gbc")
        : base(rom, speedup ? SpeedupFlags.All : SpeedupFlags.None) { }

    // public override void Inject(Joypad joypad) {
    //     CpuWrite(0xFFA7, (byte) joypad);
    //     CpuWrite(0xFFA8, (byte) joypad);
    // }

    // public override void InjectMenu(Joypad joypad) {
    //     CpuWrite(0xFFA4, (byte) joypad);
    // }

    public void ClearBattleText(byte lastSlot = 1) {
        while(Hold(Joypad.A | Joypad.B, "GetJoypad", "BattleMenu", "InitPartyMenuWithCancel", "DisableLCD") == SYM["GetJoypad"]) {
            InjectMenu(Joypad.A | Joypad.B);
            AdvanceFrame(Joypad.A | Joypad.B);
            AdvanceFrame(Joypad.A);
        }

        while(CpuRead("wMenuCursorY") != lastSlot) {
            RunUntil("GetJoypad");
            AdvanceFrame();
        }
        RunUntil("GetJoypad");
        AdvanceFrame();
    }

    public void Press(params Joypad[] joypads) {
        foreach(Joypad joypad in joypads) {
            RunUntil("GetJoypad");
            InjectMenu(joypad);
            AdvanceFrame();
        }
    }

    public override void RandomizeRNG(Random random) {
        byte[] randomValues = new byte[3];
        random.NextBytes(randomValues);

        byte[] savestate = SaveState();
        savestate[642 + 0x104] = randomValues[0]; // rdiv
        savestate[642 + 0x1E1] = randomValues[1]; // hRandomAdd
        savestate[642 + 0x1E2] = randomValues[2]; // hRandomSub
        LoadState(savestate);
    }

    // slot goes from 1 -> 4
    public void UseMove(int slot, int numMoves = 4) {
        OpenFightMenu();
        int currentSlot = CpuRead("wCurMoveNum") + 1;
        int difference = currentSlot - slot;
        int numSlots = difference == 0 ? 0 : slot % 2 == currentSlot % 2 ? (int)(numMoves/2) : 1;
        Joypad joypad = (((Math.Abs(difference * numMoves) + difference) % numMoves) & 2) != 0 ? Joypad.Down : Joypad.Up;
        switch(numSlots) {
            case 0: Press(Joypad.None); break;
            case 1: Press(joypad); break;
            case 2: Press(joypad, Joypad.None, joypad); break;
            default: Press(Joypad.None); break;
        }
        Press(Joypad.A);
    }

    public void UseItem(string name, int targetPokeIndex = -1) {
        Dictionary<string, byte> bag = GetBag();
        if(!bag.ContainsKey(name)) {
            throw new Exception("Item does not exist");
        }
        int targetSlot = bag[name];
        ScrollToItem(targetSlot);
        Press(Joypad.A, Joypad.None, Joypad.A);

        if(targetPokeIndex != -1) {
            RunUntil("InitPartyMenuWithCancel");
            AdvanceFrame();
            RunUntil("GetJoypad");
            AdvanceFrame();
            RunUntil("GetJoypad");
            Press(Joypad.A);
        }
    }

    public void ScrollToItem(int slot) {
        int curBagPos = Math.Max(CpuRead("wItemsPocketCursor"), (byte) 1);
        int curScreenScroll = CpuRead("wItemsPocketScrollPosition");
        int curSlot = curBagPos + curScreenScroll;
        Joypad joypad = slot > curSlot ? Joypad.Down : Joypad.Up;
        int numScrolls = Math.Abs(curSlot - slot);

        OpenItemBag();
        Hold(joypad, "ScrollingMenu");
        RunUntil("GetJoypad");

        for(int i = 0; i < numScrolls; i++) {
            ScrollBagSlot(joypad);
        }
    }

    public void ScrollBagSlot(Joypad joypad) {
        Hold(joypad, "GetJoypad");
        InjectMenu(joypad);
        AdvanceFrame();
    }

    // returns item name, what slot item is in
    public Dictionary<string, byte> GetBag() {
        Dictionary<string, byte> bag = new Dictionary<string, byte>();

        int addr = SYM["wItems"];
        byte index = 1;
        while(CpuRead(addr) != 0xFF) {
            GscItem item = Items[CpuRead(addr++) - 1];
            byte quantity = CpuRead(addr++);
            bag[item.Name] = index++;
        }

        return bag;
    }

    public void OpenFightMenu() {
        if(CpuRead("wMenuCursorY") == 2) Press(Joypad.Up);
        if(CpuRead("wMenuCursorX") == 2) Press(Joypad.Left);
        Press(Joypad.A);
    }

    public void OpenItemBag() {
        if(CpuRead("wMenuCursorY") == 1) Press(Joypad.Down);
        if(CpuRead("wMenuCursorX") == 2) Press(Joypad.Left);
        Press(Joypad.A);
    }

    public GscPokemon GetBattleMon(bool enemy) {
        return new GscPokemon(this, enemy);
    }
}
