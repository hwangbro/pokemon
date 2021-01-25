using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public static class RbyIGTChecker {
    // todo figure out continuous paths with item pickups
    // threaded
    // design better params for string/item pickups
    // flag for checking 60 vs 3600
    // yoloball manips, dv checking
    // flag for verbosity
    // store results intelligently in data structure to print out igt charts
    public static void CheckIGT(Rby gb, RbyIntroSequence intro, string targetPoke, List<RbyTile> itemsPickups, params string[] paths) {
        gb.HardReset();
        intro.ExecuteUntilIGT(gb);
        byte[] igtState = gb.SaveState();
        int noEncounter = 0;

        int numThreads = 0;

        bool[] threadsRunning = new bool[numThreads];
        Thread[] threads = new Thread[numThreads];


        for(byte i = 0; i < 60; i++) {
            gb.LoadState(igtState);
            gb.CpuWrite(gb.SYM["wPlayTimeSeconds"], 0);
            gb.CpuWrite(gb.SYM["wPlayTimeFrames"], i);
            intro.ExecuteAfterIGT(gb);

            int ret = 0;
            foreach(string path in paths) {
                ret = gb.Execute(spacePath(path));
                if(itemsPickups.Contains(gb.Tile))
                    gb.PickupItem();
                if(ret != gb.SYM["JoypadOverworld"]) {
                    break;
                }
            }

            if(ret == gb.SYM["CalcStats"]) {
                bool caught = gb.Yoloball();
                RbyPokemon enemyMon = gb.EnemyMon;
                RbyTile curTile = gb.Tile;
                if(caught) {
                    Console.WriteLine("Frame #{0}: encounter {1} on tile {2}, DVs: {3:X4} YOLOBALL SUCCESS", i, enemyMon.Species.Name, curTile, enemyMon.DVs);
                } else {
                    Console.WriteLine("Frame #{0}: encounter {1} on tile {3}, DVs: {3:X4} YOLOBALL FAILURE", i, enemyMon.Species.Name, curTile, enemyMon.DVs);
                }
            } else {
                Console.WriteLine("Frame #{0}: no encounter", i);
                noEncounter++;
            }
        }
        // print out manip success
        Console.WriteLine("no encounter igt: {0}/60", noEncounter);
    }

    public static string spacePath(string path) {
        string output = "";

        string[] validActions = new string[] { "A", "U", "D", "L", "R", "S_B" };
        while(path.Length > 0) {
            if (validActions.Any(path.StartsWith)) {
                if (path.StartsWith("S")) {
                    output += "S_B";
                    path = path.Remove(0, 3);
                } else {
                    output += path[0];
                    path = path.Remove(0, 1);
                }

                output += " ";
            } else {
                throw new Exception(String.Format("Invalid Path Action Recieved: {0}", path));
            }
        }

        return output.Trim();
    }
}
