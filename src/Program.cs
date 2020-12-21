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
        Crystal gb = new Crystal();
        // for(int i = 0; i < 0xff; i++) {
        //     Console.Write(i);
        //     try {
        //         Console.WriteLine(" {0}", gb.Species[i].Name);
        //     } catch {
        //         Console.WriteLine(" null");
        //     }
        // }
        // gb.Record("test");
        // gb.LoadState("basesaves/crystal/test.gqs");
        // gb.UseMove(2);
        // gb.ClearBattleText();

        // gb.UseMove(2);
        // gb.ClearBattleText();



        // gb.Dispose();
        // runSimulation();
    }

    public static void runSimulation() {
        const int gbCount = 1;
        Crystal[] gbs = new Crystal[gbCount];
        for(int i = 0; i < gbCount; i++) {
            gbs[i] = new Crystal();
        }
        gbs[0].Record("test");

        GscKaren a = new GscKaren(186);
        a.Simulate($"Simulation/crystal/karen/xacc", gbs, 10, "basesaves/crystal/test2.gqs", a.xacc);

        gbs[0].Dispose();
    }
}
