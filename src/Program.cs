using System;


class Program {
    static void Main(string[] args) {
        // Red gb = new Red();

        RedSimulation2 a = new RedSimulation2();
        string[] initialStates = new string[] {"basesaves/red/silpharbok.gqs", "basesaves/red/silphrival.gqs", "basesaves/red/cubonerocket.gqs",
                                               "basesaves/red/silphgio.gqs", "basesaves/red/hypno.gqs"};
        RedSimulation2.ActionCallback[] actions = new RedSimulation2.ActionCallback[] {a.Arbok, a.SilphRival, a.CuboneRocket, a.SilphGio, a.Hypno};

        a.Simulate("simulation/red/silphrival", 1, 10, initialStates, actions);
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
}
