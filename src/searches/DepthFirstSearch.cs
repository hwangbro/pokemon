using System;
using System.Linq;
using System.Collections.Generic;

public class DFParameters<Gb, T> where Gb : GameBoy
                                 where T : Tile<T> {

    public bool PruneAlreadySeenStates = true;
    public int MaxCost = 0;
    public int NoEncounterSS = 1;
    public int EncounterSS = 0;
    public string LogStart = "";
    public T[] EndTiles = null;
    public int EndEdgeSet = 0;
    public byte EndMapId = 0;
    public Func<Gb, bool> EncounterCallback = null;
    public Action<DFState<T>> FoundCallback = null;
}

public class DFState<T> where T : Tile<T> {

    public T Tile;
    public byte MapId;
    public int EdgeSet;
    public int WastedFrames;
    public Action BlockedActions;
    public IGTResults IGT;
    public string Log;
    public byte APressCounter;

    public override int GetHashCode() {
        unchecked {
            const int prime = 92821;
            int hash = prime + Tile.X;
            hash = hash * prime + Tile.Y;
            hash = hash * prime + Tile.Collision;
            hash = hash * prime + IGT.MostCommonHRA;
            hash = hash * prime + IGT.MostCommonHRS;
            hash = hash * prime + IGT.MostCommonDivider;
            return hash;
        }
    }
}

public static class DepthFirstSearch {

    public static void StartSearch<Gb, T>(Gb[] gbs, DFParameters<Gb, T> parameters, T startTile, int startEdgeSet, byte[][] states) where Gb : GameBoy
                                                                                                                                    where T : Tile<T> {
        IGTResults initialState = new IGTResults(states.Length);
        for(int i = 0; i < states.Length; i++) {
            initialState[i] = new IGTState();
            initialState[i].State = states[i];
            initialState[i].HRA = -1;
            initialState[i].HRS = -1;
            initialState[i].Divider = -1;
        }

        RecursiveSearch2(gbs, parameters, new DFState<T> {
            Tile = startTile,
            EdgeSet = startEdgeSet,
            WastedFrames = 0,
            Log = parameters.LogStart,
            BlockedActions = Action.A,
            IGT = initialState,
            APressCounter = 0,
        }, new HashSet<int>());
    }

    private static void RecursiveSearch<Gb, T>(Gb[] gbs, DFParameters<Gb, T> parameters, DFState<T> state, HashSet<int> seenStates) where Gb : GameBoy
                                                                                                                                    where T : Tile<T> {
        if(parameters.EndTiles != null && state.EdgeSet == parameters.EndEdgeSet && parameters.EndTiles.Any(t => t.X == state.Tile.X && t.Y == state.Tile.Y)) {
            if(parameters.FoundCallback != null) parameters.FoundCallback(state);
            else Console.WriteLine(state.Log);
        }

        if(parameters.PruneAlreadySeenStates && !seenStates.Add(state.GetHashCode())) {
            return;
        }

        byte[][] states = state.IGT.States;

        foreach(Edge<T> edge in state.Tile.Edges[state.EdgeSet]) {
            if(state.WastedFrames + edge.Cost > parameters.MaxCost) continue;
            if((state.BlockedActions & edge.Action) > 0) continue;

            IGTResults results = GameBoy.IGTCheckParallel<Gb>(gbs, states, gb => gb.Execute(edge.Action) == gb.OverworldLoopAddress, parameters.EncounterCallback == null ? parameters.NoEncounterSS : 0);

            DFState<T> newState = new DFState<T>() {
                Tile = edge.NextTile,
                EdgeSet = edge.NextEdgeset,
                Log = state.Log + edge.Action.LogString() + " ",
                IGT = results,
                WastedFrames = state.WastedFrames + edge.Cost,
            };

            int noEncounterSuccesses = results.TotalSuccesses;
            if(parameters.EncounterCallback != null) {
                int encounterSuccesses = results.TotalFailures;
                for(int i = 0; i < results.NumIGTFrames && encounterSuccesses >= parameters.EncounterSS; i++) {
                    gbs[0].LoadState(results.States[i]);
                    if(parameters.EncounterCallback(gbs[0])) encounterSuccesses++;
                }

                if(encounterSuccesses >= parameters.EncounterSS) {
                    if(parameters.FoundCallback != null) parameters.FoundCallback(newState);
                    else Console.WriteLine(state.Log);
                }
            }

            if(noEncounterSuccesses >= parameters.NoEncounterSS) {
                Action blockedActions = state.BlockedActions;

                if(edge.Action == Action.A) blockedActions |= Action.StartB;
                if((edge.Action & Action.A) > 0) blockedActions |= Action.A;
                else blockedActions &= ~(Action.A | Action.StartB);

                newState.BlockedActions = blockedActions;
                RecursiveSearch(gbs, parameters, newState, seenStates);
            }
        }
    }

    private static void RecursiveSearch2<Gb, T>(Gb[] gbs, DFParameters<Gb, T> parameters, DFState<T> state, HashSet<int> seenStates) where Gb : GameBoy
                                                                                                                                    where T : Tile<T> {
        if(parameters.EndTiles != null && state.MapId == parameters.EndMapId && state.EdgeSet == parameters.EndEdgeSet && parameters.EndTiles.Any(t => t.X == state.Tile.X && t.Y == state.Tile.Y)) {
            if(parameters.FoundCallback != null) parameters.FoundCallback(state);
            else Console.WriteLine(state.Log);
            return;
        }

        List<(byte, byte, byte)> itemPickups = new List<(byte, byte, byte)> {
            (59, 34, 31), // candy
            (59, 35, 23), // rope
            (59, 3, 2), // moon stone
            (59, 5, 31), // wg
            (61, 28, 5), // mp
        };

        if(parameters.PruneAlreadySeenStates && !seenStates.Add(state.GetHashCode())) {
            return;
        }

        byte[][] states = state.IGT.States;
        byte mapId = 0;

        foreach(Edge<T> edge in state.Tile.Edges[state.EdgeSet]) {
            if(state.WastedFrames + edge.Cost > parameters.MaxCost) continue;
            if((state.BlockedActions & edge.Action) > 0) continue;
            if(state.APressCounter > 0 && edge.Action == Action.A) continue;
            byte APressCounter = state.APressCounter;
            if(edge.Action == Action.A) {
                APressCounter = 2;
            } else {
                APressCounter = (byte) Math.Max(0, (int) (APressCounter) - 1);
            }

            IGTResults results = GameBoy.IGTCheckParallel<Gb>(gbs, states,
                gb => {
                    int ret = gb.Execute(edge.Action);
                    mapId = gb.GetMap().Id;
                    if(ret != gb.SYM["CalcStats"] && itemPickups.Contains((mapId, edge.NextTile.X, edge.NextTile.Y)))
                        gb.PickupItem();
                    return ret == gb.OverworldLoopAddress;
                }, parameters.EncounterCallback == null ? parameters.NoEncounterSS : 0);

            Console.WriteLine($"Tile: {mapId}#{edge.NextTile}, Edgeset: {edge.NextEdgeset}, Log: {state.Log + edge.Action.LogString()}, Success: {results.TotalSuccesses}, WastedFrames: {state.WastedFrames + edge.Cost}");

            DFState<T> newState = new DFState<T>() {
                Tile = edge.NextTile,
                EdgeSet = edge.NextEdgeset,
                MapId = mapId,
                Log = state.Log + edge.Action.LogString() + " ",
                IGT = results,
                WastedFrames = state.WastedFrames + edge.Cost,
                APressCounter = APressCounter,
            };

            int noEncounterSuccesses = results.TotalSuccesses;
            if(parameters.EncounterCallback != null) {
                int encounterSuccesses = results.TotalFailures;
                for(int i = 0; i < results.NumIGTFrames && encounterSuccesses >= parameters.EncounterSS; i++) {
                    gbs[0].LoadState(results.States[i]);
                    if(parameters.EncounterCallback(gbs[0])) encounterSuccesses++;
                }

                if(encounterSuccesses >= parameters.EncounterSS) {
                    if(parameters.FoundCallback != null) parameters.FoundCallback(newState);
                    else Console.WriteLine(state.Log);
                }
            }

            if(noEncounterSuccesses >= parameters.NoEncounterSS) {
                Action blockedActions = state.BlockedActions;

                if(edge.Action == Action.A) {
                    blockedActions |= Action.StartB;
                    blockedActions |= Action.A;
                }
                // if((edge.Action & Action.A) > 0) blockedActions |= Action.A;
                else blockedActions &= ~(Action.A | Action.StartB);

                newState.BlockedActions = blockedActions;
                RecursiveSearch2(gbs, parameters, newState, seenStates);
            }
        }
    }
}