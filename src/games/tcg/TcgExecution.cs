using System;
using System.Collections.Generic;
using System.Linq;

public partial class Tcg {

    // ReadJoypad, at various points, reads data from joypad and stores
    // into hKeysHeld, hKeysPressed, and hKeysReleased
    // these values are not read right away, and are not read at the top of
    // ReadJoypad.

    // SaveButtonsHeld is also a major part of where these inputs are stored,
    // at least for hKeysHeld
    public override void Inject(Joypad joypad) {
        // readjoypad = 00:04de
        // 00:050E
        // = readjoypad + 0x30
        CpuWrite("hKeysPressed", (byte) joypad);
    }

    public void InjectKeysHeld(Joypad joypad) {
        //00:0510
        // = ReadJoypad + 0x32
        // savebuttonsheld = 00:0522
        // stored in hkeysheld = 00:0523
        // 00:5227 = savebuttonsheld + 0x04
        CpuWrite("hKeysHeld", (byte) joypad);
    }

    public void InjectDPadRepeat(Joypad joypad) {
        Inject(joypad);
        InjectKeysHeld(joypad);
        CpuWrite("hDPadHeld", (byte) joypad);
    }

    public override void Press(params Joypad[] joypads) {
        foreach(Joypad joypad in joypads) {
            // 07:536A = input check on intro screen 1 IntroCutsceneJoypad
            // Func_1d078.asm_1d0b8 = input check on title screen TitleScreenJoypad
            // HandleMenuInput.check_A_or_B
            // HandlePlayerModeMoveInput.skipMoving = interacting with ow sprites

            // need to check everywhere that reads from FF91 (hjoypressed)

            int[] addrs = {
                // 0x01D36A, // introcutscenejoypad, 07:536a
                // 0x01D0b8, // titlescreenjoypad 07:50b8
                // SYM["HandleMenuInput.check_A_or_B"], // a/b press on regular menu
                SYM["SaveButtonsHeld"] + 0x05, // overworld movement
                // SYM["HandleYesOrNoMenu"] + 0x1b, //a/b for yes/no
            };

            RunUntil(addrs);
            Inject(joypad);
            AdvanceFrame();
        }
    }

    public void ScrollYesNoMenu(Joypad joypad) {
        RunUntil("HandleDPadRepeat");
        InjectDPadRepeat(joypad);
        AdvanceFrame();
    }

    public void SayYes() {
        AdvanceFrame();
        if(CpuRead("wCurMenuItem") != 0) ScrollYesNoMenu(Joypad.Left);
        Press(Joypad.A);
        AdvanceFrame();
    }

    public void SayNo() {
        AdvanceFrame();
        if(CpuRead("wCurMenuItem") != 1) ScrollYesNoMenu(Joypad.Right);
        Press(Joypad.A);
        AdvanceFrame();
    }

    public void RunMovement(params Joypad[] joypads) {
        foreach(Joypad joypad in joypads) {
            do {
                RunFor(1);
                Hold(Joypad.B, "ReadJoypad"); // requires B to be held every frame to run
            } while(CpuRead(SYM["wPlayerCurrentlyMoving"]) != 0);
            RunUntil(SYM["SaveButtonsHeld"] + 0x05);
            InjectKeysHeld(joypad);
        }
    }

    public void ClearText() {
        // CardListFunction wGameEvent
        // return the exit address?
        int[] addr = {
            SYM["WaitForButtonAorB"], // clear OW textbox breakpoint
            SYM["HandleYesOrNoMenu"], // yes/no breaks and are handled separately
            SYM["HandlePlayerMoveModeInput"], // OW loop should break
            SYM["CheckSkipDelayAllowed"], // in duel skippable with B
            SYM["WaitForWideTextBoxInput.wait_A_or_B_loop"], //
            SYM["DisplayCardList.wait_button"], // in hand/display
            SYM["HandleDuelMenuInput"], // in main duel menu
            SYM["Func_8aaa"] + 0x4f, // prize check
        };

        while(true) {
            int ret = RunUntil(addr);
            if (ret == SYM["WaitForButtonAorB"] || ret == SYM["WaitForWideTextBoxInput.wait_A_or_B_loop"]) {
                Press(Joypad.A);
            } else if(ret == SYM["CheckSkipDelayAllowed"]) {
                InjectKeysHeld(Joypad.B);
                AdvanceFrame();
            } else {
                break;
            }
        }
    }

    // run if opp arena card hp is 0?
    public void PickPrize() {
        RunUntil(SYM["Func_8aaa"] + 0x4f);
        Press(Joypad.A);
    }

    public void HandScroll(int slot) {
        int curSlot = CpuRead("wCurMenuItem") + CpuRead("wListScrollOffset");

        Joypad direction = slot > curSlot ? Joypad.Down : Joypad.Up;
        int numScrolls = Math.Abs(slot - curSlot);
        for(int i = 0; i < numScrolls; i++) {
            RunUntil("HandleMenuInput");
            RunUntil(SYM["SaveButtonsHeld"] + 0x05);
            InjectDPadRepeat(direction);
            AdvanceFrame();
        }
    }

    public void MenuInput(Joypad joypad) {
        RunUntil("HandleMenuInput");
        Press(joypad);
    }

    public void MenuScroll(int slot) {
        int maxItemIndex = CpuRead("wNumMenuItems") - 1;
        int numScrolls = slot;
        Joypad dir = Joypad.Down;
        if(slot > maxItemIndex / 2) {
            dir = Joypad.Up;
            numScrolls = Math.Max(1, maxItemIndex - slot + 1);
        }
        for(int i = 0; i < numScrolls; i++) {
            RunUntil("HandleMenuInput");
            RunUntil(SYM["SaveButtonsHeld"] + 0x05);
            InjectDPadRepeat(dir);
            AdvanceFrame();
        }
    }

    // scrolls down to specified slot and presses A twice on the item
    public void UseHandCard(int slot, int cardSlot = -1) {
        UseDuelMenuOption(TcgDuelMenu.Hand);
        HandScroll(slot);
        MenuInput(Joypad.A);
        RunUntil("HandleMenuInput");
        Press(Joypad.A);
        if(cardSlot != -1) {
            MenuScroll(cardSlot);
            Press(Joypad.A);
            ClearText();
        }
    }

    // Presses A on one of the main duel options
    public void UseDuelMenuOption(TcgDuelMenu option) {
        DuelMenuScroll((byte) option);
        DuelMenuInput(Joypad.A);
    }

    public void UseAttack(int slot, bool discard) {
        UseDuelMenuOption(TcgDuelMenu.Attack);
        MenuScroll(slot);
        MenuInput(Joypad.A);
        if(discard) {
            RunUntil("HandleMenuInput");
            MenuInput(Joypad.A);
        }
    }

    private void DuelMenuInput(Joypad joypad) {
        RunUntil("HandleDuelMenuInput");
        Press(joypad);
    }

    private void DuelMenuScroll(byte slot) {
        int curSlot = CpuRead("wCurrentDuelMenuItem");
        if(curSlot == slot) return;

        if(slot % 2 != curSlot % 2) {
            RunUntil("HandleDuelMenuInput");
            InjectDPadRepeat(Joypad.Up);
            AdvanceFrame();
        }
        int numScrolls = Math.Abs(curSlot - slot) / 2;
        Joypad direction = curSlot > slot ? Joypad.Left : Joypad.Right;

        if(numScrolls > 1) {
            numScrolls = 1;
            direction ^= (Joypad) 0x30;
        }
        for(int i = 0; i < numScrolls; i++) {
            RunUntil("HandleDuelMenuInput");
            InjectDPadRepeat(direction);
            AdvanceFrame();
        }
    }

    public void ClearIntro() {
        RunUntil("Start");
        Press(Joypad.A, Joypad.A);
        RunUntil("Func_1d078.asm_1d0b8"); // title screen joypad
        Press(Joypad.A);
        RunUntil("HandleMenuInput.check_A_or_B");
        Press(Joypad.A);
        RunUntil("HandlePlayerMoveModeInput");
    }
}
