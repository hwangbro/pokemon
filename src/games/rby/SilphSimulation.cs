using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

public class SilphResult {

    [JsonIgnore]
    public double Frames {
        get { return Cycles / GameBoy.SamplesPerFrame; }
    }

    public double Seconds {
        get { return Cycles / Math.Pow(2, 21); }
    }

    public bool[] FightResults;
    public bool StillHaveParaHeals;

    [JsonIgnore]
    public int Cycles;
}

public abstract class SilphSimulation<Gb, Result> where Gb : GameBoy
                                                  where Result : SilphResult, new() {

    public delegate bool ActionCallback(Gb gb, Dictionary<string, object> memory);
    public abstract void TransformInitialState(Gb gb, ushort hp, ref byte[] state);
    public abstract void SimulationStart(Gb gb, Dictionary<string, object> memory);
    public abstract bool HasSimulationEnded(Gb gb, Dictionary<string, object> memory);
    public abstract void SimulationEnd(Gb gb, Dictionary<string, object> memory, ref Result result);

    public List<Result> Results;
    // todo look into threadpools

    public void Simulate(string name, int numThreads, int numSimulations, string[] initialStates, ActionCallback[] actionCallback, bool write = true) {
        Results = new List<Result>();

        object writeLock = new object();
        object fileLock = new object();
        object randomLock = new object();
        int numSims = 0;

        bool[] threadsRunning = new bool[numThreads];
        Thread[] threads = new Thread[numThreads];

        Gb[] gbs = MultiThread.MakeThreads<Gb>(numThreads);
        gbs[0].Record("test");

        // Load the starting save state and transform the initial state
        // This transformation is usually setting the starting HP and advancing to the battle menu
        List<byte[]> states = new List<byte[]>();
        for(int i = 0; i < 5; i++) {
            states.Add(File.ReadAllBytes(initialStates[i]));
        }
        // byte[] state = File.ReadAllBytes("basesaves/red/silpharbok.gqs");
        // TransformInitialState(gbs[0], 113, ref state);

        // Initialize a random object to vary the RNG in the simulations
        int seed = (int) DateTime.Now.Ticks & 0x0000FFFF;
        Random random = new Random(seed);


        MultiThread.For(numSimulations, gbs, (gb, iterator) => {
            ushort hp = 113;
            Result result = new Result();
            result.Cycles = 0;
            result.FightResults = new bool[] {false, false, false, false, false};
            result.StillHaveParaHeals = true;
            Dictionary<string, object> memory = new Dictionary<string, object>();
            List<byte[]> statesCopy = new List<byte[]>(states);
            bool finished = false;
            for(int i = 0; i < initialStates.Length; i++) {
                if(finished) {
                    continue;
                }
                byte[] state;
                lock(fileLock) {
                    state = statesCopy[i];
                    TransformInitialState(gb, hp, ref state);
                }
                int start;

                lock(randomLock) {
                    gb.LoadState(state);
                    gb.RandomizeRNG(random);
                    start = gb.TimeNow;
                }

                SimulationStart(gb, memory);

                do {
                    if(!actionCallback[i](gb, memory)) {
                        gb.CpuWriteBE<ushort>("wBattleMonHP", 0);
                        break;
                    }
                } while(!HasSimulationEnded(gb, memory));

                SimulationEnd(gb, memory, ref result);

                result.Cycles += (gb.TimeNow - start);
                hp = (ushort) memory["hpLeft"];
                if((ushort) memory["hpLeft"] == 0) {
                    finished = true;
                    continue;
                } else if(i == 3) {
                    hp += 3;
                }

                result.FightResults[i] = true;
                if(!(bool)memory["para"]) {
                    result.StillHaveParaHeals = false;
                }
            }
            lock(writeLock) {
                Results.Add(result);
            }
        });

        if(write) {
            File.WriteAllText(name + ".json", JsonConvert.SerializeObject(Results));
        }
        gbs[0].Dispose();
    }
}
