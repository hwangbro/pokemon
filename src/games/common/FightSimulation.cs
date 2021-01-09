using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

public class SimulationResult {

    [JsonIgnore]
    public double Frames {
        get { return Cycles / GameBoy.SamplesPerFrame; }
    }

    public double Seconds {
        get { return Cycles / Math.Pow(2, 21); }
    }

    [JsonIgnore]
    public int Cycles;
}

public abstract class FightSimulation<Gb, Result> where Gb : GameBoy
                                                  where Result : SimulationResult, new() {

    public delegate bool ActionCallback(Gb gb, Dictionary<string, object> memory);
    public abstract void TransformInitialState(Gb gb, ref byte[] state);
    public abstract void SimulationStart(Gb gb, Dictionary<string, object> memory);
    public abstract bool HasSimulationEnded(Gb gb, Dictionary<string, object> memory);
    public abstract void SimulationEnd(Gb gb, Dictionary<string, object> memory, ref Result result);

    public List<Result> Results;
    // todo look into threadpools

    // all gb types used with this need to have a default constructor with 0 params. see red for ex
    public void Simulate(string name, int numThreads, int numSimulations, string initialState, ActionCallback actionCallback, bool write = true) {
        Results = new List<Result>();

        object writeLock = new object();
        int numSims = 0;

        bool[] threadsRunning = new bool[numThreads];
        Thread[] threads = new Thread[numThreads];

        // Load the starting save state and transform the initial state
        // This transformation is usually setting the starting HP and advancing to the battle menu
        byte[] state = File.ReadAllBytes(initialState);
        Gb initGb = (Gb) Activator.CreateInstance(typeof(Gb));
        initGb.Record("test");
        TransformInitialState(initGb, ref state);

        // Initialize a random object to vary the RNG in the simulations
        int seed = (int) DateTime.Now.Ticks & 0x0000FFFF;
        Random random = new Random(seed);

        for(int simCount = 0; simCount < numSimulations; simCount++) {
            int threadIndex;
            while((threadIndex = Array.IndexOf(threadsRunning, false)) == -1) {
                Thread.Sleep(50);
            }

            threadsRunning[threadIndex] = true;

            new Thread(parameter => {
                // Create a memory dictionary object to store information across multiple turns
                Dictionary<string, object> memory = new Dictionary<string, object>();

                // Create a new instance of the game being used
                // Currently requires a default constructor
                Gb gb = (Gb)Activator.CreateInstance(typeof(Gb));

                // Put a mutex lock on the random object and randomize the rng on the state
                lock(random) {
                    gb.LoadState(state);
                    gb.RandomizeRNG(random);
                    Console.WriteLine("starting sim #{0}", numSims++);
                }
                int start = gb.TimeNow;
                SimulationStart(gb, memory);

                // Main loop. actionCallback is the simulated fight function that returns false if you lose
                // This implementation requires that function to clear the text on its own after it chooses a move
                do {
                    if(!actionCallback(gb, memory)) {
                        gb.CpuWriteBE<ushort>("wBattleMonHP", 0);
                        break;
                    }
                } while(!HasSimulationEnded(gb, memory));

                lock(writeLock) {
                    Result result = new Result() {
                        Cycles = gb.TimeNow - start
                    };
                    SimulationEnd(gb, memory, ref result);
                    Results.Add(result);
                }
                threadsRunning[threadIndex] = false;
                initGb.Dispose();
            }).Start();

            if(write) {
                File.WriteAllText(name + ".json", JsonConvert.SerializeObject(Results));
            }
        }
    }

    public void Simulate2(string name, int numThreads, int numSimulations, string initialState, ActionCallback actionCallback, bool write = true) {
        Results = new List<Result>();

        object writeLock = new object();
        int numSims = 0;

        bool[] threadsRunning = new bool[numThreads];
        Thread[] threads = new Thread[numThreads];

        Gb[] gbs = MultiThread.MakeThreads<Gb>(numThreads);
        gbs[0].Record("test");

        // Load the starting save state and transform the initial state
        // This transformation is usually setting the starting HP and advancing to the battle menu
        byte[] state = File.ReadAllBytes(initialState);
        TransformInitialState(gbs[0], ref state);

        // Initialize a random object to vary the RNG in the simulations
        int seed = (int) DateTime.Now.Ticks & 0x0000FFFF;
        Random random = new Random(seed);

        MultiThread.For(numSimulations, gbs, (gb, iterator) => {
            Dictionary<string, object> memory;
            int start;

            lock(random) {
                gb.LoadState(state);
                gb.RandomizeRNG(random);
                memory = new Dictionary<string, object>();
                start = gb.TimeNow;
                Console.WriteLine("starting sim #{0}", numSims++);
            }

            SimulationStart(gb, memory);

            do {
                if(!actionCallback(gb, memory)) {
                    gb.CpuWriteBE<ushort>("wBattleMonHP", 0);
                    break;
                }
            } while(!HasSimulationEnded(gb, memory));

            lock(writeLock) {
                Result result = new Result() {
                    Cycles = gb.TimeNow - start
                };
                SimulationEnd(gb, memory, ref result);
                Results.Add(result);
                Console.WriteLine("end hp: {0}", memory["hpLeft"]);
            }
        });

        if(write) {
            File.WriteAllText(name + ".json", JsonConvert.SerializeObject(Results));
        }
        gbs[0].Dispose();
    }
}
