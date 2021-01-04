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

        byte[] state = gb.SaveState();
        for(int i = 0; i < 1; i++) {
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
            gb.AdvanceFrames(i);
            gb.Press(Joypad.A);
            gb.RunUntil("WaitForButtonAorB");
            byte rng = gb.CpuRead("wRNGCounter");
            Console.WriteLine("{0:X2}{1:X2}, {2:X2}", gb.CpuRead("wRNG1"), gb.CpuRead("wRNG2"), gb.CpuRead("wRNGCounter"));

            gb.ClearText();
            TcgDuelDeck myDeck = gb.MyDeck;
            TcgDuelDeck oppDeck = gb.OppDeck;

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

            double score = GetScore(gb);
            if(score > 0) {
                if(score == 1) {
                    Console.WriteLine("winner winner");
                }
                continue;
            }

            foreach(TcgPkmnCard card in gb.MyDeck.BasicsInHand) {
                int index = gb.MyDeck.Hand.IndexOf(card);
                gb.UseHandCard(index);
                gb.ClearText();
            }

            gb.MenuInput(Joypad.B);
            gb.ClearText();
            gb.UseDuelMenuOption(TcgDuelMenu.Attack);

            Console.WriteLine("\n\nHand\n{0}", String.Join("\n", gb.MyDeck.Hand.Select(item => item.Name).ToArray()));
            gb.AdvanceFrames(100);
        }

        gb.Dispose();
    }

    public static void Test2() {
        Tcg gb = new Tcg();
        gb.LoadState("test.gqs");
        gb.Record("test");

        gb.DoTurn();
        gb.DoTurn();
        gb.DoTurn();

        gb.AdvanceFrames(100);

        gb.Dispose();
    }

    public static double GetScore(Tcg gb) {
        if(gb.OppDeck.BasicsInHand.Count() > 2) {
            return -1;
        } else if(gb.OneTurnWin()) {
            return 1;
        } else if(gb.OppDeck.BasicsInHand.Count() == 1) {
            return 0.5;
        } else {
            return 0;
        }
    }
}
