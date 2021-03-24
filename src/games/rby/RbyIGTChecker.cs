using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;

public static class RbyIGTChecker<Gb> where Gb : Rby {
    // todo fix edge case: yellow pidgey manip picks up item after yoloball on last tile
    // flag for verbosity

    class IGTResult {
        public RbyPokemon Mon;
        public RbyMap Map;
        public RbyTile Tile;
        public bool Yoloball;

        public override string ToString() {
            return String.Format("[{0}] [{1}]: {2} on {3}, Yoloball: {4}", IGTSec, IGTFrame, Mon, Tile, Yoloball);
        }
        public byte IGTSec;
        public byte IGTFrame;
    }

    public static List<(byte, byte, byte)> Empty = new List<(byte, byte, byte)>();

    public static void CheckIGT(string statePath, RbyIntroSequence intro, string path, string targetPoke, List<(byte, byte, byte)> itemPickups, bool check3600, bool checkDV) {
        byte[] state = File.ReadAllBytes(statePath);

        int numThreads = 32;
        bool verbose = false;

        bool[] threadsRunning = new bool[numThreads];
        Thread[] threads = new Thread[numThreads];

        Gb[] gbs = MultiThread.MakeThreads<Gb>(numThreads);

        gbs[0].LoadState(state);
        gbs[0].HardReset();
        // gbs[0].Record("test");
        intro.ExecuteUntilIGT(gbs[0]);
        byte[] igtState = gbs[0].SaveState();

        List<IGTResult> manipResults = new List<IGTResult>();
        Dictionary<string, int> manipSummary = new Dictionary<string, int>();
        byte seconds = check3600 ? (byte) 60 : (byte) 1;

        object frameLock = new object();
        object writeLock = new object();
        int igtCount = 0;

        MultiThread.For(seconds*60, gbs, (gb, iterator) => {
            IGTResult res = new IGTResult();
            lock(frameLock) {
                gb.LoadState(igtState);
                res.IGTSec = (byte)(igtCount / 60);
                res.IGTFrame = (byte)(igtCount % 60);
                gb.CpuWrite("wPlayTimeSeconds", res.IGTSec);
                gb.CpuWrite("wPlayTimeFrames", res.IGTFrame);
                igtCount++;
                if(verbose) Console.WriteLine(igtCount);
            }

            intro.ExecuteAfterIGT(gb);
            int ret = 0;
            foreach(string step in spacePath(path).Split()) {
                ret = gb.Execute(step);
                if(itemPickups.Contains((gb.Tile.Map.Id, gb.Tile.X, gb.Tile.Y)))
                    gb.PickupItem();
                if(ret != gb.SYM["JoypadOverworld"]) break;
            }

            if(ret == gb.SYM["CalcStats"]) {
                res.Yoloball = gb.Yoloball();
                res.Mon = gb.EnemyMon;
            }
            res.Tile = gb.Tile;
            res.Map = gb.Map;

            lock(writeLock) {
                manipResults.Add(res);
            }
        });

        // print out manip success
        int success = 0;
        manipResults.Sort(delegate(IGTResult a, IGTResult b) {
            return (a.IGTSec*60 + a.IGTFrame).CompareTo(b.IGTSec*60 + b.IGTFrame);
        });

        foreach(var item in manipResults) {
            if(verbose) Console.WriteLine(item);
            if((String.IsNullOrEmpty(targetPoke) && item.Mon == null) ||
                (item.Mon != null && item.Mon.Species.Name.ToLower() == targetPoke.ToLower() && item.Yoloball)) {
                success++;
            }
            string summary;
            if(item.Mon != null) {
                summary = $", Tile: {item.Map.Id}#{item.Tile.ToString()}, Yoloball: {item.Yoloball}";
                summary = checkDV ? item.Mon + summary : "L" + item.Mon.Level + " " + item.Mon.Species.Name + summary;
                // summary = checkDV ? item.Mon + ", Tile: " + item.Map.Id.ToString() + " " + item.Tile.ToString() + ", Yoloball: " + item.Yoloball.ToString() : item.Mon.Species.Name + ", Tile: " + item.Tile.ToString() + ", Yoloball: " + item.Yoloball.ToString();
            } else {
                summary = "No Encounter";
            }
            if(!manipSummary.ContainsKey(summary)) {
                manipSummary.Add(summary, 1);
            } else {
                manipSummary[summary]++;
            }
        }

        foreach(var item in manipSummary) {
            Console.WriteLine("{0}, {1}/{2}", item.Key, item.Value, seconds * 60);
        }

        Console.WriteLine("Success: {0}/{1}", success, seconds * 60);
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
