using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


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

        // CheckRNG();
        // return;

        gb.LoadState("basesaves/tcg/bulb/jessica.gqs");
        gb.SoftReset();

        // gb.Record("test");

        Dictionary<int, List<int>> streaks = new Dictionary<int, List<int>>();
        int wins = 0;
        int totalWinTurns = 0;
        bool prevResult = false;
        int index = 0;
        int numDuels = 360;

        byte[] state = gb.SaveState();
        for(int i = 3; i < numDuels; i++) {
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

            // kristin/isaac/sara/joseph have an extra textbox
            // gb.Press(Joypad.A);
            // gb.RunUntil("WaitForButtonAorB");

            // add delay frames here
            gb.AdvanceFrames(i);
            // return;

            gb.Press(Joypad.A);
            gb.RunUntil("WaitForButtonAorB");
            int start = gb.TimeNow;
            // gb.SaveState($"test{i}.gqs");
            // continue;

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
            Console.WriteLine("Duel #{2}: {0} turns, {1}, RNG: {3:X2}, {4:0} seconds", gb.CpuRead("wDuelTurns") / 2, won ? "won" : "lost", i, rng, (gb.TimeNow - start) / Math.Pow(2, 21));
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

            // read packs
            // gb.RunUntil("WaitForWideTextBoxInput.wait_A_or_B_loop");
            // gb.ClearText();
            // ReadPack(gb);
            // gb.RunUntil("DisplayCardList.wait_button");
            // gb.Press(Joypad.B);
            // Console.WriteLine("\n");
            // gb.ClearText();
            // ReadPack(gb);
        }
        Console.WriteLine("Total wins: {0}/{1}, avg win turns: {2}", wins, numDuels, (float) ((float) totalWinTurns/ (float) wins));
        gb.Dispose();
    }

    public static void PrintHands() {
        StreamWriter startWriter = new StreamWriter("hitmon_michael.txt");

        Tcg gb = new Tcg(true);
        gb.LoadState("basesaves/tcg/bulb/michael-hitmonchanonly.gqs");
        gb.SoftReset();
        byte[] state = gb.SaveState();
        string output = "";

        for(int i = 276; i < 283; i++) {
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

            gb.Press(Joypad.A);
            gb.RunUntil("WaitForButtonAorB");
            byte rng = gb.CpuRead("wRNGCounter");

            gb.ClearText();

            bool coinFlip = gb.PredictCoinFlip();
            output += String.Format("Frame: {0}, RNG: {1:X2}, Going First? {2}\n", i, rng, coinFlip);
            output += String.Format("{0,-25}{1,-25}\n", "My Hand", "Opp Hand\n");
            // Console.WriteLine("Frame: {0}, RNG: {1:X2}, Going First? {2}", i, rng, coinFlip);
            // Console.WriteLine("{0,-25}{1,-25}\n", "My Hand", "Opp Hand");
            List<TcgCard> hand = gb.MyDeck.Hand;
            List<TcgCard> oppHand = gb.OppDeck.Hand;

            List<TcgCard> deck = gb.MyDeck.Deck;
            List<TcgCard> oppDeck = gb.OppDeck.Deck;
            for(int j = 0; j < 7; j++) {
                output += String.Format("{0,-25}{1,-25}\n", hand[j].Name, oppHand[j].Name);
                // Console.WriteLine("{0,-25}{1,-25}", hand[j].Name, oppHand[j].Name);
            }
            output += "\nTop 3 cards\n";
            // Console.WriteLine("\nTop 3 cards");
            for(int j = 0; j < 3; j++) {
                output += String.Format("{0,-25}{1,-25}\n", deck[j].Name, oppDeck[j].Name);
                // Console.WriteLine("{0,-25}{1,-25}", deck[j].Name, oppDeck[j].Name);
            }
            output += "\n\n";
            // Console.WriteLine("\n\n");
        }

        startWriter.Write(output);
        startWriter.Flush();
    }

    public static void CheckRNG() {
        Tcg gb = new Tcg(true);
        gb.LoadState("test.gqs");
        Console.WriteLine("{0:X2}", gb.CpuRead("wRNGCounter"));
        return;
    }

    public static void ReadPack(Tcg gb) {
        for(int i = 0; i < 10; i++) {
            Console.WriteLine(gb.Cards[gb.CpuRead(0xc400 + i)].Name);
        }
    }
}
