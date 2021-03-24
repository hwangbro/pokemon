using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.Threading;

public static class Rt3Search {
    public static StreamWriter startWriter = new StreamWriter("rt3_manip.txt");
    public static RbyMap Route3;
    public static RbyMap Moon1F;
    public static RbyMap MoonB1F;
    public static RbyMap MoonB2F;
    public static string baseMovement = RbyIGTChecker<Red>.spacePath("RRRRRRRRURRUUUUUARRRRRRRRRRRRDDDDDRRRRRRRARUURRUUUUUUUUUURRRRUUUUUUUUUURRRRRUU");
    public static string candidateMovement = RbyIGTChecker<Red>.spacePath("UUUUULLLLLLLLLDDRRRRUURRRRRUUUUUUURRRRRRURUUUUUURRRDDDDDDDDDRDDDDDDDDRRRRRRRRURUUUUUUUURULUUUUUUUUUUUULLULUUUUUULLDLLLLDDDLLLLLLLLDDDD");

    public static List<(byte, byte, byte)> itemPickups = new List<(byte, byte, byte)> {
            (59,34, 31), // candy
            (59,35, 23), // rope
            (59,3, 2), // moon stone
            (59,5, 31), // wg
            (61, 28, 5), // mp
        };

    public static void Start() {
        BuildGraph();

        StartSearch(32);
        // IGT(RbyIGTChecker<Red>.spacePath("UUUUUULLLLLLLLLDDRRRRUURRRRRUUUUUUURRRRRRURUUUUUURRRDDDDDDDDDRDDDDDDDDRRRRRRRRURUUUUUUUURULUUUUUUUUUUUULLULUUUUUULLDLLLLDDDLLLLLLLLDDDDD"));
    }

    public static void BuildGraph() {
        Red gb = new Red();
        Moon1F = gb.Maps["MtMoon1F"];
        MoonB1F = gb.Maps["MtMoonB1F"];
        MoonB2F = gb.Maps["MtMoonB2F"];

        Pathfinding.GenerateEdges(Moon1F, 0, 17, Moon1F.Tileset.LandPermissions, Action.Left | Action.Up | Action.Down | Action.A, Moon1F[5, 31]);
        Pathfinding.GenerateEdges(Moon1F, 1, 17, Moon1F.Tileset.LandPermissions, Action.Right | Action.Up | Action.Down | Action.Left | Action.A, Moon1F[34, 31]);
        Pathfinding.GenerateEdges(Moon1F, 2, 17, Moon1F.Tileset.LandPermissions, Action.Right | Action.Up | Action.Down | Action.Left | Action.A, Moon1F[35, 23]);
        Pathfinding.GenerateEdges(Moon1F, 3, 17, Moon1F.Tileset.LandPermissions, Action.Right | Action.Up | Action.Down | Action.Left | Action.A, Moon1F[17, 11]);
        Pathfinding.GenerateEdges(MoonB1F, 3, 17, MoonB1F.Tileset.LandPermissions, Action.Left | Action.Down | Action.A, MoonB1F[17,11]);
        Pathfinding.GenerateEdges(MoonB2F, 3, 17, MoonB2F.Tileset.LandPermissions, Action.Right | Action.Left | Action.Up | Action.A, MoonB2F[28, 5]);
        Pathfinding.GenerateEdges(MoonB2F, 4, 17, MoonB2F.Tileset.LandPermissions, Action.Left | Action.Down | Action.A, MoonB2F[25, 9]);
        Pathfinding.GenerateEdges(MoonB1F, 4, 17, MoonB1F.Tileset.LandPermissions, Action.Right | Action.Up | Action.A, MoonB1F[25, 9]);
        Pathfinding.GenerateEdges(Moon1F, 4, 17, Moon1F.Tileset.LandPermissions, Action.Left | Action.Up | Action.Down | Action.A, Moon1F[3, 2]);
        Pathfinding.GenerateEdges(Moon1F, 5, 17, Moon1F.Tileset.LandPermissions, Action.Down | Action.Right | Action.A, Moon1F[5, 5]);
        Pathfinding.GenerateEdges(MoonB1F, 5, 17, MoonB1F.Tileset.LandPermissions, Action.Down | Action.Right | Action.A, MoonB1F[21, 17]);
        Pathfinding.GenerateEdges(MoonB2F, 5, 17, MoonB2F.Tileset.LandPermissions, Action.Right | Action.Left | Action.Up | Action.Down | Action.A, MoonB2F[10, 17]);

        // SEGMENT 0 TO WATERGUN
        Moon1F[5, 30].RemoveEdge(0, Action.A);
        Moon1F[5, 30].RemoveEdge(0, Action.Down);
        Moon1F[6, 31].RemoveEdge(0, Action.Left);
        Moon1F[4, 31].RemoveEdge(0, Action.Right);

        Moon1F[6, 29].RemoveEdge(0, Action.Down);
        Moon1F[7, 29].RemoveEdge(0, Action.Down);
        Moon1F[8, 29].RemoveEdge(0, Action.Down);

        Moon1F[5, 30].AddEdge(0, new Edge<RbyTile> { Action = Action.Down, Cost = 0, NextEdgeset = 1, NextTile = Moon1F[5, 31] });


        // SEGMENT 1 TO CANDY
        Moon1F[5, 31].RemoveEdge(1, Action.A);

        Moon1F[33, 32].RemoveEdge(1, Action.Right);
        Moon1F[33, 31].RemoveEdge(1, Action.A);
        Moon1F[33, 31].RemoveEdge(1, Action.Right);
        Moon1F[33, 31].AddEdge(1, new Edge<RbyTile> { Action = Action.Right, Cost = 0, NextEdgeset = 2, NextTile = Moon1F[34, 31]});


        // SEGMENT 2 TO ROPE
        Moon1F[34, 31].RemoveEdge(2, Action.A);

        Moon1F[35, 24].RemoveEdge(2, Action.Up);
        Moon1F[34, 23].RemoveEdge(2, Action.A);
        Moon1F[34, 23].RemoveEdge(2, Action.Right);
        Moon1F[34, 23].AddEdge(2, new Edge<RbyTile> { Action = Action.Right, Cost = 0, NextEdgeset = 3, NextTile = Moon1F[35, 23]});


        // SEGMENT 3 TO MP
        Moon1F[35, 23].RemoveEdge(3, Action.A);

        Moon1F[26, 3].RemoveEdge(3, Action.Down);
        Moon1F[27, 3].RemoveEdge(3, Action.Down);
        Moon1F[28, 3].RemoveEdge(3, Action.Down);

        Moon1F[25, 3].RemoveEdge(3, Action.Left);
        Moon1F[17, 10].GetEdge(3, Action.Down).NextTile = MoonB1F[25, 9];
        MoonB1F[25, 9].AddEdge(3, new Edge<RbyTile> { Action = Action.Left, Cost = 0, NextEdgeset = 3, NextTile = MoonB1F[24, 9]});
        MoonB1F[25, 9].AddEdge(3, new Edge<RbyTile> { Action = Action.Down, Cost = 0, NextEdgeset = 3, NextTile = MoonB1F[25, 10]});
        MoonB1F[17, 10].AddEdge(3, new Edge<RbyTile> { Action = Action.Down, Cost = 0, NextEdgeset = 3, NextTile = MoonB2F[25, 9]});
        MoonB1F[18, 11].AddEdge(3, new Edge<RbyTile> { Action = Action.Left, Cost = 0, NextEdgeset = 3, NextTile = MoonB2F[25, 9]});
        MoonB2F[25, 9].AddEdge(3, new Edge<RbyTile> { Action = Action.Right, Cost = 0, NextEdgeset = 3, NextTile = MoonB2F[26, 9]});

        // MoonB1F[24, 9].RemoveEdge(3, Action.Down);
        // MoonB1F[23, 9].RemoveEdge(3, Action.Down);
        // MoonB1F[22, 9].RemoveEdge(3, Action.Down);
        // MoonB1F[21, 9].RemoveEdge(3, Action.Down);
        // MoonB1F[20, 9].RemoveEdge(3, Action.Down);
        // MoonB1F[19, 9].RemoveEdge(3, Action.Down);
        // MoonB1F[18, 9].RemoveEdge(3, Action.Down);

        MoonB2F[28, 6].RemoveEdge(3, Action.Up);
        MoonB2F[27, 5].RemoveEdge(3, Action.A);
        MoonB2F[28, 6].GetEdge(3, Action.Left).Cost = 0;
        MoonB2F[27, 5].RemoveEdge(3, Action.Right);
        MoonB2F[27, 5].AddEdge(3, new Edge<RbyTile> { Action = Action.Right, Cost = 0, NextEdgeset = 4, NextTile = MoonB2F[28, 5]});


        // SEGMENT 4 TO MOON STONE

        MoonB2F[28, 5].RemoveEdge(4, Action.A);
        MoonB2F[25, 8].AddEdge(4, new Edge<RbyTile> { Action = Action.Down, Cost = 0, NextEdgeset = 4, NextTile = MoonB1F[17, 11]});
        MoonB2F[26, 9].AddEdge(4, new Edge<RbyTile> { Action = Action.Left, Cost = 0, NextEdgeset = 4, NextTile = MoonB1F[17, 11]});
        MoonB1F[17, 11].AddEdge(4, new Edge<RbyTile> { Action = Action.Up, Cost = 0, NextEdgeset = 4, NextTile = MoonB1F[17, 10]});
        MoonB1F[17, 11].AddEdge(4, new Edge<RbyTile> { Action = Action.Right, Cost = 0, NextEdgeset = 4, NextTile = MoonB1F[18, 11]});
        MoonB1F[24, 9].AddEdge(4, new Edge<RbyTile> { Action = Action.Right, Cost = 0, NextEdgeset = 4, NextTile = Moon1F[17, 11]});
        MoonB1F[25, 10].AddEdge(4, new Edge<RbyTile> { Action = Action.Up, Cost = 0, NextEdgeset = 4, NextTile = Moon1F[17, 11]});

        Moon1F[17, 11].AddEdge(4, new Edge<RbyTile> { Action = Action.Down, Cost = 0, NextEdgeset = 5, NextTile = Moon1F[17, 12]});
        Moon1F[3, 3].RemoveEdge(4, Action.Up);
        Moon1F[4, 2].RemoveEdge(4, Action.A);
        Moon1F[3, 2].RemoveEdge(4, Action.A);
        Moon1F[4, 2].RemoveEdge(4, Action.Left);
        Moon1F[4, 2].AddEdge(4, new Edge<RbyTile> { Action = Action.Left, Cost = 0, NextEdgeset = 5, NextTile = Moon1F[3, 2]});


        // MOON STONE TO END
        Moon1F[5, 4].AddEdge(5, new Edge<RbyTile> { Action = Action.Down, Cost = 0, NextEdgeset = 5, NextTile = MoonB1F[5,5]});
        Moon1F[4, 5].AddEdge(5, new Edge<RbyTile> { Action = Action.Right, Cost = 0, NextEdgeset = 5, NextTile = MoonB1F[5,5]});
        MoonB1F[5, 5].AddEdge(5, new Edge<RbyTile> { Action = Action.Down, Cost = 0, NextEdgeset = 5, NextTile = MoonB1F[5,6]});
        MoonB1F[5, 5].AddEdge(5, new Edge<RbyTile> { Action = Action.Right, Cost = 0, NextEdgeset = 5, NextTile = MoonB1F[6,5]});

        MoonB1F[20, 17].AddEdge(5, new Edge<RbyTile> { Action = Action.Right, Cost = 0, NextEdgeset = 5, NextTile = MoonB2F[21, 17]});
        MoonB1F[21, 16].AddEdge(5, new Edge<RbyTile> { Action = Action.Down, Cost = 0, NextEdgeset = 5, NextTile = MoonB2F[21, 17]});

        MoonB2F[21, 17].AddEdge(5, new Edge<RbyTile> { Action = Action.Right, Cost = 0, NextEdgeset = 5, NextTile = MoonB2F[22,17]});
        MoonB2F[21, 17].AddEdge(5, new Edge<RbyTile> { Action = Action.Up, Cost = 0, NextEdgeset = 5, NextTile = MoonB2F[21,16]});
        // Pathfinding.DebugDrawEdges(Moon1F, 5);
        // Pathfinding.DebugDrawEdges(Moon1F, 5);
    }

    public static void StartSearch(int numThreads) {
        Red[] gbs = MultiThread.MakeThreads<Red>(numThreads);
        // gbs[0].Show();
        byte[][] initialStates = new byte[120][];

        Red gb = new Red(true);
        gb.LoadState("basesaves/red/rt3moon.gqs");
        gb.HardReset();
        // List<RbyTile> endTiles = new List<RbyTile>() { MoonB2F[10, 17], MoonB2F[10, 18]};
        List<RbyTile> endTiles = new List<RbyTile>() { Moon1F[3, 2] };


        RbyIntroSequence intro = new RbyIntroSequence(RbyStrat.PalHold, RbyStrat.GfSkip, RbyStrat.Hop0, RbyStrat.TitleSkip, RbyStrat.Continue, RbyStrat.Continue);
        intro.ExecuteUntilIGT(gb);
        byte[] state = gb.SaveState();
        for(int frame = 36; frame <= 37; frame++) {
            for(int sec = 0; sec < 60; sec++) {
                if(frame == 36 && sec == 57) continue;
                gb.LoadState(state);
                gb.CpuWrite("wPlayTimeFrames", (byte) frame);
                gb.CpuWrite("wPlayTimeSeconds", (byte) sec);
                intro.ExecuteAfterIGT(gb);
                gb.Execute(baseMovement);
                int ret = 0;
                foreach(string step in candidateMovement.Split()) {
                    ret = gb.Execute(step);
                    if(itemPickups.Contains((gb.Map.Id, gb.Tile.X, gb.Tile.Y)))
                        gb.PickupItem();
                    if(ret != gb.SYM["JoypadOverworld"]) break;
                }
                if (ret != gb.OverworldLoopAddress) continue;
                initialStates[((frame-36)*60) + sec] = gb.SaveState();
            }
        }
        RbyTile tile = gb.Tile;

        DFParameters<Red, RbyTile> parameters = new DFParameters<Red, RbyTile>() {
            NoEncounterSS = 115,
            MaxCost = 12,
            EndTiles = endTiles.ToArray(),
            EndEdgeSet = 5,
            EndMapId = 59,
            FoundCallback = state => {
                Console.WriteLine("Found a manip!");
                startWriter.WriteLine($"Log: {candidateMovement} {state.Log}, Success: {state.IGT.TotalSuccesses}, Cost: {state.WastedFrames}");
                startWriter.Flush();
            }
        };
        DepthFirstSearch.StartSearch(gbs, parameters, tile, 3, initialStates);
    }

    public static void IGT(string path) {
        Red gb = new Red(true);
        // gb.Show();
        gb.LoadState("basesaves/red/rt3moon.gqs");
        gb.HardReset();


        RbyIntroSequence intro = new RbyIntroSequence(RbyStrat.PalHold, RbyStrat.GfSkip, RbyStrat.Hop0, RbyStrat.TitleSkip, RbyStrat.Continue, RbyStrat.Continue);
        intro.ExecuteUntilIGT(gb);
        byte[] state = gb.SaveState();
        for(int frame = 36; frame <= 37; frame++) {
            for(int sec = 0; sec < 60; sec++) {
                if(frame == 36 && sec == 57) continue;
                gb.LoadState(state);
                gb.CpuWrite("wPlayTimeFrames", (byte) frame);
                gb.CpuWrite("wPlayTimeSeconds", (byte) sec);
                intro.ExecuteAfterIGT(gb);
                int ret = 0;
                foreach(string step in RbyIGTChecker<Red>.spacePath("RRRRRRRRURRUUUUUARRRRRRRRRRRRDDDDDRRRRRRRARUURRUUUUUUUUUURRRRUUUUUUUUUURRRRRU").Split()) {
                    ret = gb.Execute(step);
                    if(ret != gb.SYM["JoypadOverworld"]) break;
                }
                if(ret == gb.SYM["CalcStats"]) {
                    continue;
                }
                foreach(string step in path.Split()) {
                    ret = gb.Execute(step);
                    if(itemPickups.Contains((gb.Map.Id, gb.Tile.X, gb.Tile.Y)))
                        gb.PickupItem();
                    if(ret != gb.SYM["JoypadOverworld"]) break;
                }
                if(ret != gb.SYM["CalcStats"]) {
                    Console.WriteLine("Success");
                    Console.WriteLine("RNG: 0x{0:X2}{1:X2}", gb.CpuRead("hRandomAdd"), gb.CpuRead("hRandomSub"));
                } else {
                    Console.WriteLine("Failed");
                    Console.WriteLine("RNG: 0x{0:X2}{1:X2}", gb.CpuRead("hRandomAdd"), gb.CpuRead("hRandomSub"));
                }
            }
        }
    }
}