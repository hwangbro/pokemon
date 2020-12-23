using System;
using System.Collections.Generic;


class Program {
    static void Main(string[] args) {
        Tcg gb = new Tcg(true, "basesaves/tcg/Jennifer.sav");
        gb.Record("test");

        byte[] intro = gb.SaveState();

        for(int i = 0; i < 1; i++) {
            gb.LoadState(intro);
            gb.RunUntil("IntroCutsceneJoypad");
            gb.AdvanceFrames(i); // delay
            gb.Press(Joypad.A);
            gb.Press(Joypad.A);
            gb.Press(Joypad.A, Joypad.A);

            gb.ClearText();
            gb.SayYes();
            gb.ClearText();


            // TcgDuelDeck myDeck = gb.CreateDuelDeck();
            // TcgDuelDeck oppDeck = gb.CreateDuelDeck(true);

            // foreach(TcgCard card in myDeck.Hand) {
            //     Console.WriteLine(card.Name);
            // }

            // Console.WriteLine("{0} Basics in hand\n", myDeck.BasicsInHand);
            // gb.Press(Joypad.A);
            gb.AdvanceFrames(100);
        }

        gb.Dispose();
    }

    public static void SilphSimulation() {
        SilphSimulations a = new SilphSimulations();
        string[] initialStates = new string[] {
            "basesaves/red/silpharbok.gqs",
            "basesaves/red/silphrival.gqs",
            "basesaves/red/cubonerocket.gqs",
            "basesaves/red/silphgio.gqs",
            "basesaves/red/juggler1.gqs",
            "basesaves/red/hypno.gqs",
            "basesaves/red/koga.gqs"
            };
        SilphSimulations.ActionCallback[] actions =
            new SilphSimulations.ActionCallback[] {
                a.Arbok,
                a.SilphRival,
                a.CuboneRocket,
                a.SilphGio,
                a.Juggler1,
                a.Hypno,
                a.Koga
            };

        SilphSimulations.ActionCallback[] actions2 =
            new SilphSimulations.ActionCallback[] {
                a.ArbokThrash,
                a.SilphRivalNormal,
                a.CuboneRocketNormal,
                a.SilphGio,
                a.Juggler1,
                a.Hypno,
                a.Koga
            };

        a.Simulate("simulation/red/silphbar/nosilphbar", 16, 25000, initialStates, actions2);
    }

    public static void runSimulation() {
        const int gbCount = 16;
        Crystal[] gbs = new Crystal[gbCount];
        for(int i = 0; i < gbCount; i++) {
            gbs[i] = new Crystal(true);
        }
        // gbs[0].Record("test");
        int numSims = 10000;

        GscSimulation a;
        for(ushort i = 52; i < 73; i++) {
            a = new GscSimulation(i);
            a.Simulate($"Simulation/crystal/karen/xacc_{i}", 16, numSims, "basesaves/crystal/karen_xacc.gqs", a.xacclowhp);
            // a.Simulate($"Simulation/crystal/karen/noxacc_{i}", gbs, numSims, "basesaves/crystal/karen_noxacc.gqs", a.noxacc);
        }

        // for(ushort i = 93; i < 170; i++) {
        //     a = new GscKaren(i);
        //     a.Simulate($"Simulation/crystal/karen/xacc_{i}", gbs, numSims, "basesaves/crystal/karen_xacc.gqs", a.xacc);
        // }

        // gbs[0].Dispose();
    }
}
