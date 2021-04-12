using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;


// to do
    // create a savestate with starting deck for each trainer
    // create a method for TcgDuel that returns a heuristic of how good the duel is
    //   should be unique for each trainer
    //   based on basics in hand, some trainer cards, enemy basics in hand, etc
    // look for clusters of good fights

public struct DuelResult {
    public bool Result;
    public int Turns;
    public double Seconds;
    public byte RNG;
    public int Frame;
}

public struct HandData {
    public int Frame;
    public byte RNG;
    public bool CoinFlip;
    public List<TcgCard> MyHand;
    public List<TcgCard> OppHand;
    public List<TcgCard> MyDraw;
    public List<TcgCard> OppDraw;
    public List<TcgCard> MyPrizes;
    public List<TcgCard> OppPrizes;
}


public static class TcgManips {

    public static void SearchDuels() {
        // start flowtimer manip from ow or inside hand, not on new deck
        // reset timers
        int numThreads = 32;
        Tcg[] gbs = MultiThread.MakeThreads<Tcg>(numThreads);
        gbs[0].LoadState("basesaves/tcg/bulb/jessica.gqs");
        gbs[0].SoftReset();
        byte[] state = gbs[0].SaveState();

        Dictionary<int, List<int>> streaks = new Dictionary<int, List<int>>();
        List<DuelResult> DuelResults = new List<DuelResult>();
        object writeLock = new object();
        int wins = 0;
        int totalWinTurns = 0;
        double totalWinTime = 0;
        // bool prevResult = false;
        // int index = 0;
        int numDuels = 360;


        MultiThread.For(360, gbs, (gb, iterator) => {
            int numTurns = 0;
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
            gb.AdvanceFrames(iterator);
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
                numTurns++;
                finished = gb.DoTurn();
            }
            double duration = (gb.TimeNow - start) / Math.Pow(2, 21);
            bool won = (gb.CpuRead("wDuelFinished") == 1 && gb.CpuRead("wWhoseTurn") == 0xc2) || (gb.CpuRead("wDuelFinished") == 2 && gb.CpuRead("wWhoseTurn") != 0xc2);

            DuelResult res = new DuelResult();
            res.Frame = iterator;
            res.Result = won;
            res.Turns = numTurns;
            res.Seconds = duration;
            res.RNG = rng;

            lock(writeLock) {
                DuelResults.Add(res);
                if(won) {
                    wins++;
                    totalWinTurns += numTurns;
                    totalWinTime += duration;
                }
            }

            // trying to record streaks?

            // if(won) {
            //     wins++;
            //     totalWinTurns += numTurns;
            //     totalWinTime += duration;
            //     if(prevResult) {
            //         streaks[index].Add(numTurns);
            //     } else {
            //         index = iterator;
            //         streaks[index] = new List<int>();
            //         streaks[index].Add(numTurns);
            //     }
            // }
            // prevResult = won;


            // read packs

            // gb.RunUntil("WaitForWideTextBoxInput.wait_A_or_B_loop");
            // gb.ClearText();
            // ReadPack(gb);
            // gb.RunUntil("DisplayCardList.wait_button");
            // gb.Press(Joypad.B);
            // Console.WriteLine("\n");
            // gb.ClearText();
            // ReadPack(gb);


            // to-do
            // AI improvements
            // better trainer uses (CS, PP)
            // look into coin flip manipulations
            // better prize selection
            // better knowledge on when to setup bench pokes
            // can start by looking at AI https://github.com/pret/poketcg/blob/master/src/engine/bank08.asm
        });

        // print results
        foreach(DuelResult res in DuelResults.OrderBy(x => x.Frame)) {
            Console.WriteLine("Duel #{2}: {0} turns, {1}, RNG: {3:X2}, {4:0} seconds", res.Turns, res.Result ? "won" : "lost", res.Frame, res.RNG, res.Seconds);
            // separate losses in streaks
            if(!res.Result) {
                Console.WriteLine("\n\n");
            }
        }

        Console.WriteLine("Total wins: {0}/{1}, avg win turns: {2}", wins, numDuels, (float) ((float) totalWinTurns/ (float) wins));
    }

    public static void PrintHands() {
        StreamWriter startWriter = new StreamWriter("hitmon_michael.txt");

        int numThreads = 32;
        Tcg[] gbs = MultiThread.MakeThreads<Tcg>(numThreads);
        gbs[0].LoadState("basesaves/tcg/bulb/michael-hitmonchanonly.gqs");
        gbs[0].SoftReset();
        byte[] state = gbs[0].SaveState();

        string output = "";
        bool verbose = false;
        List<HandData> results = new List<HandData>();
        object writeLock = new object();

        MultiThread.For(360, gbs, (gb, iterator) => {
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
            gb.AdvanceFrames(iterator);

            gb.Press(Joypad.A);
            gb.RunUntil("WaitForButtonAorB");
            byte rng = gb.CpuRead("wRNGCounter");

            gb.ClearText();

            bool coinFlip = gb.PredictCoinFlip();
            HandData data = new HandData();
            data.RNG = rng;
            data.CoinFlip = coinFlip;
            data.Frame = iterator;
            data.MyHand = gb.MyDeck.Hand;
            data.OppHand = gb.OppDeck.Hand;
            data.MyDraw = gb.MyDeck.Deck.GetRange(0, 3);
            data.OppDraw = gb.OppDeck.Deck.GetRange(0, 3);

            lock(writeLock) {
                results.Add(data);
            }
        });

        // print data
        foreach(HandData data in results.OrderBy(x => x.Frame)) {
            string curOutput = "";
            curOutput += String.Format("Frame: {0}, RNG: {1:X2}, Going First? {2}\n", data.Frame, data.RNG, data.CoinFlip);
            String.Format("{0,-25}{1,-25}\n", "My Hand", "Opp Hand\n");
            for(int i = 0; i < 7; i++) {
                curOutput += String.Format("{0,-25}{1,-25}\n", data.MyHand[i].Name, data.OppHand[i].Name);
            }
            curOutput += "\nTop 3 cards\n";
            for(int i = 0; i < 3; i++) {
                curOutput += String.Format("{0,-25}{1,-25}\n", data.MyDraw[i].Name, data.OppDraw[i].Name);
            }
            curOutput += "\n\n";

            // restrict data visibility
            // if(data.Frame >= 100 && data.Frame <= 105) {
            //     output += curOutput;
            // }

            output += curOutput;
            if(verbose) {
                Console.WriteLine(curOutput);
            }
        }

        startWriter.Write(output);
        startWriter.Flush();
    }

    public static void CheckRNG() {
        // prints current wRNGCounter value
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
