using System;
using System.Collections.Generic;


class Program {
    static void Main(string[] args) {
        Red gb = new Red(true);
        // gb.Record("test");
        gb.LoadState("basesaves/red/char/CharMoonip.gqs");
        RbyIntroSequence intro = new RbyIntroSequence(RbyStrat.NoPal, RbyStrat.GfSkip, RbyStrat.Hop0, RbyStrat.TitleSkip, RbyStrat.Continue, RbyStrat.Continue);
        List<RbyTile> itemTiles = new List<RbyTile> {
            gb.Maps["MtMoon1F"][34, 31],
            gb.Maps["MtMoon1F"][35, 23],
            gb.Maps["MtMoonB2F"][28, 5],
        };
        string path1 = "UUUUUUUUUUUUURRRRRRUUUUUUURRRRDRDDDDDDDDDDDDDDDDRRRRRRURRR";
        string path2 = "UUUUUUUUR";
        string path3 = "UUUUUUUUUUULUUUUUUUUULLLLLLLLLLDDDDLLLLLLLDDDD";
        string path4 = "DDLLLLLLLLRRRUUULUR";
        string path5 = "DDDDLLLRARRARRARRRUUDDDDDDLLLLLLULLLLUUUUUUUUUUUALL";

        RbyIGTChecker.CheckIGT(gb, intro, "", itemTiles, path1, path2, path3, path4, path5);
        gb.Dispose();
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
