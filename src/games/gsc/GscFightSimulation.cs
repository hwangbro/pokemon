using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

public static class MultiThread {

    public delegate void MultiThreadRunFn<Gb>(Gb gb, int iterator) where Gb : GameBoy;

    public static void For<Gb>(int count, Gb[] gbs, MultiThreadRunFn<Gb> fn) where Gb : GameBoy {
        Dictionary<Gb, bool> threadsRunning = new Dictionary<Gb, bool>();
        foreach(Gb gb in gbs) {
            threadsRunning[gb] = false;
        }

        object gbSelectionLock = new object();

        Parallel.For(0, count, new ParallelOptions() { MaxDegreeOfParallelism = gbs.Length }, (iterator) => {
            Gb gb = null;
            lock(gbSelectionLock) {
                foreach(Gb gameboy in gbs) {
                    if(!threadsRunning[gameboy]) {
                        gb = gameboy;
                        threadsRunning[gb] = true;
                        break;
                    }
                }
            }

            fn(gb, iterator);
            threadsRunning[gb] = false;
        });
    }
}

public class SimulationResult {

    [JsonIgnore]
    public double Frames {
        get { return Cycles / GameBoy.SamplesPerFrame; }
    }

    [JsonIgnore]
    public double Seconds {
        get { return Cycles / Math.Pow(2, 21); }
    }

    public ulong Cycles;
}

public abstract class GscFightSimulation<Gb, Result> where Gb : GameBoy
                                                  where Result : SimulationResult, new() {
    public delegate bool ActionCallback(Gb gb, Dictionary<string, object> memory);

    public List<Result> Results;

    public void Simulate(string name, Gb[] gbs, int numSimulations, string initialState, ActionCallback actionCallback, bool write = true) {
        Results = new List<Result>();

        object frameLock = new object();
        object writeLock = new object();

        byte[] state = File.ReadAllBytes(initialState);
        TransformInitialState(gbs[0], ref state);

        int seed = (int) DateTime.Now.Ticks & 0x0000FFFF;
        Random random = new Random(seed);
        Console.WriteLine("Random seed: " + seed);
        uint numSims = 0;

        MultiThread.For(numSimulations, gbs, (gb, iterator) => {
            Dictionary<string, object> memory;
            int start;
            uint curSim;
            lock(frameLock) {
                gb.LoadState(state);
                gb.RandomizeRNG(random);
                byte[] rng = new byte[3] {gb.CpuRead(0xFF04), gb.CpuRead(0xFFE1), gb.CpuRead(0xFFE2)};
                gb.RunUntil("PlayCry");
                gb.RunUntil("GetJoypad");
                memory = new Dictionary<string, object>();
                start = gb.GetCycleCount();
                SimulationStart(gb, memory);
                curSim = numSims++;
                Console.WriteLine("{0}, {1:X2} {2:X2} {3:X2}", curSim, rng[0], rng[1], rng[2]);
            }

            actionCallback(gb, memory);
            while(!HasSimulationEnded(gb, memory)) {
                if(!actionCallback(gb, memory)) {
                    int addr = gb.SYM["wBattleMonHP"];
                    gb.CpuWriteWord(addr, 0);
                    break;
                }
            }

            lock(writeLock) {
                Console.WriteLine("{0}, fin", curSim);
                Result result = new Result() {
                    Cycles = (ulong) (gb.GetCycleCount() - start),
                };
                SimulationEnd(gb, memory, ref result);
                Results.Add(result);
            }
        });
        if(write) {
            File.WriteAllText(name + ".json", JsonConvert.SerializeObject(Results));
        }
    }

    public abstract void TransformInitialState(Gb gb, ref byte[] state);
    public abstract void SimulationStart(Gb gb, Dictionary<string, object> memory);
    public abstract bool HasSimulationEnded(Gb gb, Dictionary<string, object> memory);
    public abstract void SimulationEnd(Gb gb, Dictionary<string, object> memory, ref Result result);
}