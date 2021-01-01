using System;
using System.Collections.Generic;

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
        // wSkipDelayAllowed to check for b clears
        // OW text clears are fine, duel text clears need cleaning up
        // CardListFunction wGameEvent
        // int[] addr = {
        //     SYM["WaitForPlayerToAdvanceText"], // actual clear condition, should clear text
        //     SYM["HandleYesOrNoMenu"], // yes no menu needs to be handled separately
        //     SYM["HandlePlayerMoveModeInput"], // OW loop, should exit
        // };
        string[] addr = {
            "WaitForButtonAorB", // clear OW textbox breakpoint
            "HandleYesOrNoMenu", // yes/no breaks and are handled separately
            "HandlePlayerMoveModeInput", // OW loop should break
            "CheckSkipDelayAllowed", // in duel skippable with B
            "WaitForWideTextBoxInput.wait_A_or_B_loop", //
            "DisplayCardList.wait_button" // in hand/display
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
