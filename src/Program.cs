using System;
using System.Collections.Generic;


class Program {
    static void Main(string[] args) {
        RbyIntroSequence intro2 = new RbyIntroSequence(RbyStrat.NoPal, RbyStrat.GfSkip, RbyStrat.Hop0, RbyStrat.TitleSkip, RbyStrat.Continue, RbyStrat.Continue);
        RbyIntroSequence intro = new RbyIntroSequence(RbyStrat.GfSkip, RbyStrat.Intro0, RbyStrat.TitleSkip, RbyStrat.Continue, RbyStrat.Continue);
        List<(byte, byte, byte)> items = new List<(byte, byte, byte)> {
            (59, 34, 31), // candy
            (59, 35, 23), // rope
            (59, 3, 2), // moon stone
            (59, 5, 31), // wg
            (61, 28, 5), // mp
        };
        string path1 = "UUUUUUUUUUUUURRRRRRUUUUUUURRRRDRDDDDDDDDDDDDDDDDRRRRRRURRR";
        string path2 = "UUUUUUUUR";
        string path3 = "UUUUUUUUUUULUUUUUUUUULLLLLLLLLLDDDDLLLLLLLDDDD";
        string path4 = "DDLLLLLLLLRRRUUULUR";
        string path5 = "DDDDLLLRARRARRARRRUUDDDDDDLLLLLLULLLLUUUUUUUUUUUALL";
        string moonPath = path1 + path2 + path3 + path4 + path5;
        string path = "UUUUUUUUULULLLLLDADDADDDDADLADDADDDDDDADDDLLLLAUUALU";
        string altPath = "UUUUUUUULUULLLLLDADDADDDDADLADDADDDDDDDDADLLLLAUULU";

        RbyIGTChecker<Yellow>.CheckIGT("basesaves/yellow/pidgey_nopotion.gqs", intro, path, "PIDGEY", RbyIGTChecker<Yellow>.Empty, false, false);


        string entrMoon = "UAUUUUULLLLLLLLALDD";
        entrMoon += "RUUUUURRRRURUURURRRRRRRRUUUUUUURRRRDDRDDDDDADDDDDDDDDDRRRRRRURRR";
        entrMoon += "UUUUUUUURUUUUUUUUUUULUUUUULLLUUUULLDDLLLLLALLLLLLLDDDDDD";
        entrMoon += "LLALLDADLALLAL";
        entrMoon += "RRRUUULUR";
        entrMoon += "DDDDLLL";
        entrMoon += "URARRARRARRARU";
        entrMoon += "DDDADDALDLLLLALUULALUUUUUUUULLUAUULUULLL";
        entrMoon += "DADRARD";
        entrMoon += "DADDRRDDDDDDDDDRRRRRRRRRRRRRR";
        entrMoon += "RRUUURARRRDDRRRRRRUURRARDDDDDDDDLLLLDDDDDDDDDLLLLLLLLLLLLLLLLLLLLLLUUUUUUUUUUUAUUU";
        RbyIntroSequence entrMoonIntro = new RbyIntroSequence(RbyStrat.NoPalAB, RbyStrat.GfSkip, RbyStrat.Hop0, RbyStrat.TitleSkip, RbyStrat.Continue, RbyStrat.Continue);
        // RbyIGTChecker<Red>.CheckIGT("basesaves/red/entrmoon.gqs", entrMoonIntro, entrMoon, "PARAS", items, false, false);

        // RbyIGTChecker<Red>.CheckIGT("basesaves/red/char/r3pidgeymanip.gqs", intro2, "LLL", "SPEAROW", RbyIGTChecker<Red>.Empty, false, true);
        // Rt3Search.Start();
    }

    // public static void TestSearch() {
    //     Red gb = new Red(true);
    //     RbyMap b2f = gb.Maps["MtMoonB2F"];
    //     DFParameters<Red, RbyTile> parameters = new DFParameters<Red, RbyTile>() {
    //         EncounterSS = 114,
    //         MaxCost = 0,
    //         EndTiles = new RbyTile[] { b2f[10, 18], b2f[10, 17] },
    //         EncounterCallback = gb => {
    //             if(gb.Map.Id == 61 && gb.Tile.X == 10 && (gb.Tile.Y == 17 || gb.Tile.Y == 18)) {

    //             }
    //         },
    //         FoundCallback = state => {
    //             Console.WriteLine("Found an end path!!");
    //         }
    //     };
    //     // RandomPathSearch.GenerateRandomPath2<RbyTile>
    // }


    // public static void CrystalTest() {
    //     Crystal gb = new Crystal(false);
    //     gb.LoadState("basesaves/crystal/karen_xacc.gqs");
    //     gb.Record("test");
    //     gb.RunUntil("PlayCry");
    //     gb.RunUntil("GetJoypad");
    //     GscBag bag = gb.Bag;
    //     if(gb.CpuRead("wMenuCursorX") != 0x1) gb.MenuPress(Joypad.Left);
    //     gb.SelectMenuItem(2);
    //     gb.SwitchPocket(3);
    //     gb.AdvanceFrames(100);
    //     gb.Dispose();
    // }

    // public static void SilphSimulation() {
    //     SilphSimulations a = new SilphSimulations();
    //     string[] initialStates = new string[] {
    //         "basesaves/red/silpharbok.gqs",
    //         "basesaves/red/silphrival.gqs",
    //         "basesaves/red/cubonerocket.gqs",
    //         "basesaves/red/silphgio.gqs",
    //         "basesaves/red/juggler1.gqs",
    //         "basesaves/red/hypno.gqs",
    //         "basesaves/red/koga.gqs"
    //         };
    //     SilphSimulations.ActionCallback[] actions =
    //         new SilphSimulations.ActionCallback[] {
    //             a.Arbok,
    //             a.SilphRival,
    //             a.CuboneRocket,
    //             a.SilphGio,
    //             a.Juggler1,
    //             a.Hypno,
    //             a.Koga
    //         };

    //     SilphSimulations.ActionCallback[] actions2 =
    //         new SilphSimulations.ActionCallback[] {
    //             a.ArbokThrash,
    //             a.SilphRivalNormal,
    //             a.CuboneRocketNormal,
    //             a.SilphGio,
    //             a.Juggler1,
    //             a.Hypno,
    //             a.Koga
    //         };

    //     a.Simulate("simulation/red/silphbar/nosilphbar", 16, 25000, initialStates, actions2);
    // }


    // public static void runSimulation() {
    //     const int gbCount = 16;
    //     Crystal[] gbs = new Crystal[gbCount];
    //     for(int i = 0; i < gbCount; i++) {
    //         gbs[i] = new Crystal(true);
    //     }
    //     // gbs[0].Record("test");
    //     int numSims = 10000;

    //     GscSimulation a;
    //     for(ushort i = 52; i < 73; i++) {
    //         a = new GscSimulation(i);
    //         a.Simulate($"Simulation/crystal/karen/xacc_{i}", 16, numSims, "basesaves/crystal/karen_xacc.gqs", a.xacclowhp);
    //         // a.Simulate($"Simulation/crystal/karen/noxacc_{i}", gbs, numSims, "basesaves/crystal/karen_noxacc.gqs", a.noxacc);
    //     }

    //     // for(ushort i = 93; i < 170; i++) {
    //     //     a = new GscKaren(i);
    //     //     a.Simulate($"Simulation/crystal/karen/xacc_{i}", gbs, numSims, "basesaves/crystal/karen_xacc.gqs", a.xacc);
    //     // }

    //     // gbs[0].Dispose();
    // }
}
