using System;


class Program {
    static void Main(string[] args) {
        // CrystalTest();
        RedTest();
    }

    public static void CrystalTest() {
        Crystal gb = new Crystal(false);
        gb.LoadState("basesaves/crystal/karen_xacc.gqs");
        gb.Record("test");
        gb.RunUntil("PlayCry");
        gb.RunUntil("GetJoypad");
        GscBag bag = gb.Bag;
        if(gb.CpuRead("wMenuCursorX") != 0x1) gb.MenuPress(Joypad.Left);
        gb.SelectMenuItem(2);
        gb.SwitchPocket(3);
        gb.AdvanceFrames(100);
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

    public static void RedTest() {
        Red gb = new Red();
        gb.Record("test");

        RbyIntroSequence sequence = new RbyIntroSequence();
        sequence.Add(gb.IntroStrats["nopal"]);
        sequence.Add(gb.IntroStrats["gfSkip"]);
        sequence.Add(gb.IntroStrats["hop0"]);
        sequence.Add(gb.IntroStrats["title0"]);
        sequence.Add(gb.IntroStrats["cont"]);
        sequence.Add(gb.IntroStrats["cont"]);

        gb.ExecuteIntroSequence(sequence);
        gb.Execute("L L L U L L U A U L A L D L D L L D A D D A D D D L A L L A L U U A U"); // regular nido
        gb.Yoloball();
        // gb.Execute("L D U A L L U L L L L A U L L L L L A D D A D D L A D D D L A U U A U"); // pal nido
        Console.WriteLine(sequence);
        gb.AdvanceFrames(1000);
        gb.Dispose();
    }

    public static void YellowTest() {
        Yellow gb = new Yellow(false, "basesaves/yellow/nido.sav");
        gb.Record("test");
        RbyIntroSequence sequence = new RbyIntroSequence();
        sequence.Add(gb.IntroStrats["gfSkip"]);
        sequence.Add(gb.IntroStrats["intro1"]);
        sequence.Add(gb.IntroStrats["title0"]);
        sequence.Add(gb.IntroStrats["cont"]);
        sequence.Add(gb.IntroStrats["cont"]);

        gb.ExecuteIntroSequence(sequence);
        gb.Execute("U R A R U");
        gb.Yoloball();
        gb.AdvanceFrames(1000);
        gb.Dispose();
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
