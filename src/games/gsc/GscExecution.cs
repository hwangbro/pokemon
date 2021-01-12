using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System;

public partial class Gsc {

    public override void Press(params Joypad[] joypads) {
        for(int i = 0; i < joypads.Length; i++) {
            Joypad joypad = joypads[i];
            if(Registers.PC == (SYM["OWPlayerInput"] & 0xffff)) {
                InjectOverworld(joypad);
                Hold(joypad, "GetJoypad");
            } else {
                Hold(joypad, "GetJoypad");
                Inject(joypad);
                AdvanceFrame(joypad);
            }
        }
    }

    public override void Inject(Joypad joypad) {
        CpuWrite("hJoypadDown", (byte) joypad);
    }

    public void InjectOverworld(Joypad joypad) {
        CpuWrite("hJoyPressed", (byte) joypad);
        CpuWrite("hJoyDown", (byte) joypad);
    }

    public override int Execute(params Action[] actions) {
        int ret = 0;
        foreach(Action action in actions) {
            switch(action & ~Action.A) {
                case Action.Right:
                case Action.Left:
                case Action.Up:
                case Action.Down:
                    Joypad input = (Joypad) action;
                    RunUntil("OWPlayerInput");
                    InjectOverworld(input);
                    ret = Hold(input, "CountStep", "ChooseWildEncounter.startwildbattle", "PrintLetterDelay", "DoPlayerMovement.BumpSound");
                    if(ret == SYM["CountStep"]) {
                        ret = Hold(input, "OWPlayerInput", "ChooseWildEncounter.startwildbattle");
                        if(ret != SYM["OWPlayerInput"]) {
                            return ret;
                        }
                    } else {
                        return ret;
                    }
                    InjectOverworld(Joypad.None);
                    break;
                case Action.StartB:
                    InjectOverworld(Joypad.Start);
                    AdvanceFrame(Joypad.Start);
                    Hold(Joypad.B, "GetJoypad");
                    Inject(Joypad.B);
                    ret = Hold(Joypad.B, "OWPlayerInput");
                    break;
                default:
                    break;
            }
        }

        return ret;
    }

    public override void ClearText(bool holdDuringText, params Joypad[] menuJoypads) {
        // A list of routines that prompt the user to advance the text with either A or B.
        int[] textAdvanceAddrs = {
            SYM["PromptButton.input_wait_loop"] + 0x6,
            SYM["WaitPressAorB_BlinkCursor.loop"] + 0xb,
            SYM["JoyWaitAorB.loop"] + 0x6,
            (SYM["BattlePack.loop"] + 0x3) & 0xFFFF,
        };

        int stackPointer;
        int[] stack = new int[2];

        int menuJoypadsIndex = 0;
        Joypad hold = Joypad.None;
        if(holdDuringText) hold = menuJoypads.Length > 0 ? menuJoypads[menuJoypadsIndex] ^ (Joypad) 0x3 : Joypad.B;

        while(true) {
            // Hold the specified input until the joypad state is polled.
            Hold(hold, "GetJoypad");

            // Read the current position of the stack.
            stackPointer = Registers.SP;

            // Every time a routine gets called, the address of the following instruction gets pushed on the stack (to then be jumped to once the routine call returns).
            // To figure out where the 'GetJoypad' call originated from, we use the top two addresses of the stack.
            for(int i = 0; i < stack.Length; i++) {
                stack[i] = CpuReadLE<ushort>(stackPointer + i * 2);
            }

            // 'PrintLetterDelay' directly calls 'GetJoypad', therefore it will always be on the top of the stack.
            if(stack[0] == SYM["PrintLetterDelay.checkjoypad"] + 0x3) {
                // If the 'GetJoypad' call originated from PrintLetterDelay, use the 'hold' input to advance a frame.
                Inject(hold);
                AdvanceFrame(hold);
            } else if(stack.Intersect(textAdvanceAddrs).Any()) {
                // One of the 'textAdvanceAddrs' has been hit, clear the text box with the opposite button used in the previous frame.
                byte previous = (byte) (CpuRead("hJoyDown") & (byte) (Joypad.A | Joypad.B));
                Joypad advance = previous == 0 ? Joypad.A   // If neither A or B have been pressed on the previous frame, default to clear the text box with A.
                                               : (Joypad) (previous ^ 0x3); // Otherwise clear with the opposite button. This is achieved by XORing the value by 3.
                                                                            // (Joypad.A) 01 xor 11 = 10 (Joypad.B)
                                                                            // (Joypad.B) 10 xor 11 = 01 (Joypad.A)
                Inject(advance);
                AdvanceFrame(advance);
            } else {
                // If the call originated from 'HandleMapTimeAndJoypad' and there is currently a sprite being moved by a script, don't break.
                if(stack[0] == (SYM["HandleMapTimeAndJoypad"] & 0xffff) + 0xc && CpuRead("wScriptMode") == 2) {
                    AdvanceFrame();
                } else if(menuJoypadsIndex < menuJoypads.Length) {
                    Inject(menuJoypads[menuJoypadsIndex]);
                    AdvanceFrame(menuJoypads[menuJoypadsIndex]);
                    menuJoypadsIndex++;
                    if(holdDuringText && menuJoypadsIndex != menuJoypads.Length) hold = menuJoypads[menuJoypadsIndex] ^ (Joypad) 0x3;
                } else {
                    break;
                }
            }
        }
    }

    public void Swap(int target) {
        if(CpuReadBE<ushort>("wBattleMonHP") != 0) {
            if(CpuRead("wMenuCursorX") != 0x2) MenuPress(Joypad.Right);
            SelectMenuItem(1);
        }

        SelectMenuItem(target);
        MenuPress(Joypad.A);
    }

    private void UseMove(int slot) {
        if(CpuRead("wMenuCursorX") != 0x1) MenuPress(Joypad.Left);
        SelectMenuItem(1);
        SelectMenuItem(slot);
    }

    public void UseMove1() {
        UseMove(1);
    }

    public void UseMove2() {
        UseMove(2);
    }

    public void UseMove3() {
        UseMove(3);
    }

    public void UseMove4() {
        UseMove(4);
    }

    public void UseItem(string item, int target = -1) {
        UseItem(Items[item], target);
    }

    public void UseItem(GscItem item, int target) {
        GscBag bag = ItemBag;
        if(CpuRead("wMenuCursorX") != 0x1) MenuPress(Joypad.Left);
        SelectMenuItem(2);
        SelectBagItem(bag.IndexOf(item) + 1);

        switch(item.ExecutionPointerLabel) {
            case "StatusHealingEffect":
            case "FullRestoreEffect":
            case "RestoreHPEffect":
                SelectMenuItem(target != -1 ? target : CpuRead("wMenuCursorY"));
                MenuPress(Joypad.A, Joypad.B);
                break;
            case "XAccuracyEffect":
            case "XItemEffect":
                RunUntil(SYM["WaitPressAorB_BlinkCursor"]);
                Inject(Joypad.B);
                AdvanceFrame(Joypad.B);
                break;
        }
    }

    // starts from 1
    public void SelectMenuItem(int target) {
        RunUntil("GetMenuJoypad");
        MenuScroll(target, CpuRead("wMenuCursorY"), CpuRead("w2DMenuNumRows"), (CpuRead("w2DMenuFlags1") & 0x20) > 0);
    }

    public void SelectBagItem(int target) {
        RunUntil("GetMenuJoypad");
        MenuScroll(target, CpuRead("wMenuCursorY") + CpuRead("wMenuScrollPosition"), CpuRead("wNumItems"), (CpuRead("w2DMenuFlags1") & 0x20) > 0);
        MenuPress(Joypad.A);
    }

    public void MenuScroll(int target, int current, int max, bool wrapping) {
        Joypad input = target < current ? Joypad.Up : Joypad.Down;
        int amount = Math.Abs(current - target);

        if(wrapping && amount > max / 2) {
            amount = max - amount;
            input ^= (Joypad) 0xc0;
        }

        for(int i = 0; i < amount; i++) {
            MenuPress(input);
        }

        MenuPress(Joypad.A);
    }

    public override int WalkTo(int targetX, int targetY) {
        GscMap map = Map;
        GscTile current = Tile;
        GscTile target = map[targetX, targetY];
        GscWarp warp = map.Warps[current.X, current.Y];
        bool original = false;
        if(warp != null) {
            original = warp.Allowed;
            warp.Allowed = true;
        }
        List<Action> path = Pathfinding.FindPath(map, current, 17, map.Tileset.LandPermissions, target);
        if(warp != null) {
            warp.Allowed = original;
        }
        return Execute(path.ToArray());
    }

    public void SetTimeSec(int timesec) {
        byte[] state = SaveState();
        state[SaveStateLabels["timesec"] + 0] = (byte) (timesec >> 24);
        state[SaveStateLabels["timesec"] + 1] = (byte) (timesec >> 16);
        state[SaveStateLabels["timesec"] + 2] = (byte) (timesec >> 8);
        state[SaveStateLabels["timesec"] + 3] = (byte) (timesec & 0xff);
        LoadState(state);
    }

    public byte[][] MakeIGTStates(int timesec, GscIntroSequence intro, int numIgts) {
        SetTimeSec(timesec);
        intro.ExecuteUntilIGT(this);
        byte[] igtState = SaveState();
        byte[][] owStates = new byte[numIgts][];
        for(int i = 0; i < numIgts; i++) {
            LoadState(igtState);
            CpuWrite("wGameTimeSeconds", (byte) (i / 60));
            CpuWrite("wGameTimeFrames", (byte) (i % 60));
            intro.ExecuteAfterIGT(this);
            owStates[i] = SaveState();
        }
        return owStates;
    }

    public static string CleanUpPathParallel<Gb>(Gb[] gbs, byte[][] states, int ss, Action[] path) where Gb : GameBoy {
        List<int> aPressIndices = new List<int>();
        for(int i = 0; i < path.Length; i++) {
            if((path[i] & Action.A) > 0) aPressIndices.Add(i);
        }

        foreach(int index in aPressIndices) {
            path[index] &= ~Action.A;
            int successes = states.Length;
            MultiThread.For(states.Length, gbs, (gb, igt) => {
                gb.LoadState(states[igt]);
                if(gb.Execute(path) != gb.SYM["OWPlayerInput"]) {
                    Interlocked.Decrement(ref successes);
                }
            });
            if(successes < ss) {
                path[index] |= Action.A;
            }
        }

        return ActionFunctions.ActionsToPath(path);
    }
}