using System.IO;
using System.Threading;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

class Program {
    static void Main(string[] args) {
        // Totodile.StartSearch(1);
        // Crystal gb = new Crystal(true);

        // gb.Record("test");
        // gb.LoadState("basesaves/crystal/karen_xacc.gqs");
        // gb.AdvanceFrames(1000);
        // gb.CpuWriteWord(gb.SYM["wBattleMonHP"], 1);
        // gb.CpuWriteWord(gb.SYM["wPartyMon1HP"], 1);
        // gb.RunUntil("PlayCry");
        // gb.RunUntil("GetJoypad");

        // gb.Swap(2);
        // gb.ClearBattleText();

        // gb.Swap(1, true);
        // gb.ClearBattleText();

        // gb.OpenPkmnMenu();
        // gb.RunUntil("InitPartyMenuWithCancel");
        // gb.Hold(Joypad.Down, "GetJoypad");
        // // gb.Press(Joypad.Down);
        // gb.InjectMenu(Joypad.Down);
        // gb.AdvanceFrame();

        // gb.RunUntil("GetJoypad");
        // gb.Press(Joypad.A);

        // gb.AdvanceFrames(100);


        // gb.UseItem("X SPECIAL");
        // gb.ClearBattleText(2);


        // gb.UseMove(2);
        // gb.ClearBattleText();


        // gb.Dispose();
        runSimulation();
    }

    public static void runSimulation() {
        const int gbCount = 16;
        Crystal[] gbs = new Crystal[gbCount];
        for(int i = 0; i < gbCount; i++) {
            gbs[i] = new Crystal(true);
        }
        // gbs[0].Record("test");
        int numSims = 10000;

        GscKaren a;
        for(ushort i = 58; i < 93; i++) {
            a = new GscKaren(i);
            a.Simulate($"Simulation/crystal/karen/xacc_half_{i}", gbs, numSims, "basesaves/crystal/karen_xacc.gqs", a.xacclowhp);
            a.Simulate($"Simulation/crystal/karen/noxacc_{i}", gbs, numSims, "basesaves/crystal/karen_noxacc.gqs", a.noxacc);
        }

        for(ushort i = 93; i < 170; i++) {
            a = new GscKaren(i);
            a.Simulate($"Simulation/crystal/karen/xacc_{i}", gbs, numSims, "basesaves/crystal/karen_xacc.gqs", a.xacc);
        }

        // gbs[0].Dispose();
    }
}
