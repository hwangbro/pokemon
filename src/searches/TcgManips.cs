using System;
using System.Collections.Generic;
using System.Linq;


// to do
    // create a savestate with starting deck for each trainer
    // create a method for TcgDuel that returns a heuristic of how good the duel is
    //   should be unique for each trainer
    //   based on basics in hand, some trainer cards, enemy basics in hand, etc
    // look for clusters of good fights

public static class TcgManips {

    public static void SearchDuels() {
        // start flowtimer manip from ow or inside hand, not on new deck
        // reset timers
        Tcg gb = new Tcg(true);

        CheckRNG();
        return;

        // gb.LoadState("basesaves/tcg/jennifer.gqs");
        gb.LoadState("bulb_amanda.gqs");
        gb.SoftReset();

        gb.Record("test");

        Dictionary<int, List<int>> streaks = new Dictionary<int, List<int>>();
        int wins = 0;
        int totalWinTurns = 0;
        bool prevResult = false;
        int index = 0;
        int numDuels = 360;

        byte[] state = gb.SaveState();
        for(int i = 264; i < 274; i++) {
            gb.LoadState(state);
            gb.ClearIntro();

            // talk to npc
            gb.Press(Joypad.A);

            // clear text until yes no
            gb.ClearText();

            // yes no
            gb.SayYes();

            // clear text box
            gb.RunUntil("WaitForButtonAorB");

            // kristin/isaac have an extra textbox
            // gb.Press(Joypad.A);
            // gb.RunUntil("WaitForButtonAorB");

            // add delay frames here
            gb.AdvanceFrames(i);
            // return;

            gb.Press(Joypad.A);
            gb.RunUntil("WaitForButtonAorB");

            byte rng = gb.CpuRead("wRNGCounter");

            gb.ClearText();
            gb.PlayBasics(true, gb.PredictOppActive());
            gb.MenuInput(Joypad.B);
            gb.ClearText();

            bool finished = gb.CpuRead("wDuelFinished") != 0;

            while(!finished) {
                finished = gb.DoTurn();
            }
            bool won = (gb.CpuRead("wDuelFinished") == 1 && gb.CpuRead("wWhoseTurn") == 0xc2) || (gb.CpuRead("wDuelFinished") == 2 && gb.CpuRead("wWhoseTurn") != 0xc2);
            Console.WriteLine("Duel #{2}: {0} turns, {1}, RNG: {3:X2}", gb.CpuRead("wDuelTurns") / 2, won ? "won" : "lost", i, rng);
            if(!won) {
                Console.WriteLine("\n\n");
            }

            int turns = gb.CpuRead("wDuelTurns") / 2;
            if(won) {
                wins++;
                totalWinTurns += turns;
                if(prevResult) {
                    streaks[index].Add(turns);
                } else {
                    index = i;
                    streaks[index] = new List<int>();
                    streaks[index].Add(turns);
                }
            }
            prevResult = won;

            // need to predict more than one turn wins, or refine one turn wins
            // can start by looking at AI https://github.com/pret/poketcg/blob/master/src/engine/bank08.asm
            // or implement my own version
            //    play most "powerful" mon (check for resists?)
            //    try to play all trainers if applicable
            //    always do highest damaging move
            //    need some way to keep track of energies, either track myself or find wram value
            // need to work on more fight execution
            // routines for attaching energies (need algo to find good energies)
            // rank what basic pokemon to put on bench first
            // use trainer cards if applicable


            gb.AdvanceFrames(100);
        }
        Console.WriteLine("Total wins: {0}/{1}, avg win turns: {2}", wins, numDuels, (float) ((float) totalWinTurns/ (float) wins));
        gb.Dispose();
    }

    public static void CheckRNG() {
        Tcg gb = new Tcg(true);
        gb.LoadState("test.gqs");
        Console.WriteLine("{0:X2}", gb.CpuRead("wRNGCounter"));
        return;
    }
}
