using System;
using System.Collections.Generic;
using System.Linq;


// to do
    // create a savestate with starting deck for each trainer
    // create a method for TcgDuel that returns a heuristic of how good the duel is
    //   should be unique for each trainer
    //   based on basics in hand, some trainer cards, enemy basics in hand, etc
    // look for clusters of good fights

public static class Jennifer {

    public static void Test() {
        Tcg gb = new Tcg(false, "basesaves/tcg/jennifer.sav");
        gb.Record("test");

        Dictionary<int, List<int>> streaks = new Dictionary<int, List<int>>();
        int wins = 0;
        int totalWinTurns = 0;
        bool prevResult = false;
        int index = 0;
        int numDuels = 200;

        byte[] state = gb.SaveState();
        for(int i = 0; i < numDuels; i++) {
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
            // add delay frames here
            gb.AdvanceFrames(72+3);
            gb.Press(Joypad.A);
            gb.RunUntil("WaitForButtonAorB");
            // gb.SaveState("test3.gqs");
            byte rng = gb.CpuRead("wRNGCounter");
            Console.WriteLine("{0:X2}{1:X2}, {2:X2}", gb.CpuRead("wRNG1"), gb.CpuRead("wRNG2"), gb.CpuRead("wRNGCounter"));

            gb.ClearText();
            // gb.MyDeck.SortHand();
            gb.PlayBasics(true, gb.PredictOppActive());
            gb.MenuInput(Joypad.B);
            gb.ClearText();

            while(!gb.DoTurn()) {

            }
            bool won = gb.CpuRead("wDuelFinished") == 1 && gb.CpuRead("wWhoseTurn") == 0xc2;
            Console.WriteLine(i);
            Console.WriteLine("Duel: {0} turns, {1}", gb.CpuRead("wDuelTurns") / 2, won ? "won" : "lost");
            Console.WriteLine("Duel Result: {0}", won);

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

            // TcgDuelDeck myDeck = gb.MyDeck;
            // TcgDuelDeck oppDeck = gb.OppDeck;

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
        Console.WriteLine("Total wins: {0}/{1}, avg win turns: {2}", wins, numDuels, totalWinTurns/wins);
        gb.Dispose();
    }

    public static double GetScore(Tcg gb) {
        if(gb.OppDeck.BasicsInHand.Count() > 2) {
            return -1;
        // } else if(gb.OneTurnWin()) {
        //     return 1;
        } else if(gb.OppDeck.BasicsInHand.Count() == 1) {
            return 0.5;
        } else {
            return 0;
        }
    }
}
