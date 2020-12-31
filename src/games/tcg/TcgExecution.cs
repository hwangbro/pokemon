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

    public void InjectMenu(Joypad joypad) {
        //00:0510
        // = ReadJoypad + 0x32
        // savebuttonsheld = 00:0522
        // stored in hkeysheld = 00:0523
        // 00:5227 = savebuttonsheld + 0x04
        CpuWrite("hKeysHeld", (byte) joypad);
    }

    public override void Press(params Joypad[] joypads) {
        foreach(Joypad joypad in joypads) {
            // 07:536A = input check on intro screen 1 IntroCutsceneJoypad
            // Func_1d078.asm_1d0b8 = input check on title screen TitleScreenJoypad
            // HandleMenuInput.check_A_or_B
            // HandlePlayerModeMoveInput.skipMoving = interacting with ow sprites

            // Step();

            // need to check everywhere that reads from FF91 (hjoypressed)

            int[] addrs = {
                // 0x01D36A, // introcutscenejoypad, 07:536a
                // 0x01D0b8, // titlescreenjoypad
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
        RunUntil("HandleYesOrNoMenu.wait_DPadJoypad"); // handleyesornomenu + 0x21
        CpuWrite("hDPadHeld", (byte) joypad);
        AdvanceFrame();
    }

    public void SayYes() {
        ScrollYesNoMenu(Joypad.Left);
        Press(Joypad.A);
        AdvanceFrame();
    }

    public void SayNo() {
        ScrollYesNoMenu(Joypad.Right);
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
            InjectMenu(joypad);
        }
    }

    public void ClearText(params Joypad[] joypads) {
        int ret = RunUntil("WaitForButtonAorB.Joypad", "WaitForWideTextBoxInput.Joypad");
        while(ret == SYM["WaitForButtonAorB.Joypad"] || ret == SYM["WaitForWideTextBoxInput.Joypad"]) {
            Inject(Joypad.A | Joypad.B);
            AdvanceFrame();
            ret = RunUntil("WaitForButtonAorB.Joypad", // waitforbuttonaorb + 0x06
                                   "WaitForWideTextBoxInput.Joypad", // 2abe = orig + 10
                                   "HandleYesOrNoMenu.wait_Joypad",
                                   "HandlePlayerMoveModeInput.skipMoving",
                                   "StartDuel",
                                   "CardListFunction"); //wGameEvent
        }
    }
}