// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.IO;
// using Newtonsoft.Json;

// public class SilphResult {

//     [JsonIgnore]
//     public double Frames {
//         get { return Cycles / GameBoy.SamplesPerFrame; }
//     }

//     public double Seconds {
//         get { return Cycles / Math.Pow(2, 21); }
//     }

//     public bool[] FightResults;
//     public bool StillHaveParaHeals;
//     public bool Final;

//     [JsonIgnore]
//     public int Cycles;
//     public ushort HpLeft;
//     public bool Paralyzed;
// }

// public abstract class SilphSimulation<Gb, Result> where Gb : GameBoy
//                                                   where Result : SilphResult, new() {

//     public delegate bool ActionCallback(Gb gb, Dictionary<string, object> memory);
//     public abstract void TransformInitialState(Gb gb, ushort hp, ref byte[] state);
//     public abstract void SimulationStart(Gb gb, Dictionary<string, object> memory);
//     public abstract bool HasSimulationEnded(Gb gb, Dictionary<string, object> memory);
//     public abstract void SimulationEnd(Gb gb, Dictionary<string, object> memory, ref Result result);

//     public List<Result> Results;
//     // todo look into threadpools

//     public void Simulate(string name, int numThreads, int numSimulations, string[] initialStates, ActionCallback[] actionCallback, bool write = true) {
//         Results = new List<Result>();

//         object writeLock = new object();
//         object fileLock = new object();
//         object randomLock = new object();
//         int numSims = 0;

//         bool[] threadsRunning = new bool[numThreads];
//         Thread[] threads = new Thread[numThreads];

//         Gb[] gbs = MultiThread.MakeThreads<Gb>(numThreads);
//         // gbs[0].Record("test");

//         // Load the starting save state and transform the initial state
//         // This transformation is usually setting the starting HP and advancing to the battle menu
//         List<byte[]> states = new List<byte[]>();
//         for(int i = 0; i < initialStates.Length; i++) {
//             states.Add(File.ReadAllBytes(initialStates[i]));
//         }

//         // Initialize a random object to vary the RNG in the simulations
//         int seed = (int) DateTime.Now.Ticks & 0x0000FFFF;
//         Random random = new Random(seed);

//         MultiThread.For(numSimulations, gbs, (gb, iterator) => {
//             int curSim;
//             lock(writeLock) {
//                 curSim = numSims++;
//                 Console.WriteLine("Starting sim: {0}", curSim++);
//             }
//             ushort hp = 113;
//             Result result = new Result();
//             result.Cycles = 0;
//             result.FightResults = new bool[] {false, false, false, false, false, false, true};
//             result.StillHaveParaHeals = true;
//             Dictionary<string, object> memory = new Dictionary<string, object>();
//             List<byte[]> statesCopy = new List<byte[]>(states);
//             bool finished = false;
//             for(int i = 0; i < initialStates.Length; i++) {
//                 if(finished) {
//                     continue;
//                 }
//                 byte[] state;
//                 lock(fileLock) {
//                     state = statesCopy[i];
//                     TransformInitialState(gb, hp, ref state);
//                 }
//                 int start;

//                 lock(randomLock) {
//                     gb.LoadState(state);
//                     gb.RandomizeRNG(random);
//                     // Console.WriteLine("sim: #{0} rng: {1:X2} {2:X2} {3:X2}", curSim, gb.CpuRead(0xFF04), gb.CpuRead(0xFFD3), gb.CpuRead(0xFFD4));
//                     start = gb.TimeNow;
//                 }

//                 SimulationStart(gb, memory);

//                 do {
//                     if(!actionCallback[i](gb, memory)) {
//                         gb.CpuWriteBE<ushort>("wBattleMonHP", 0);
//                         break;
//                     }
//                 } while(!HasSimulationEnded(gb, memory));

//                 SimulationEnd(gb, memory, ref result);

//                 result.Cycles += (gb.TimeNow - start);
//                 hp = (ushort) memory["hpLeft"];
//                 if(hp == 0) {
//                     finished = true;
//                     continue;
//                 }

//                 result.FightResults[i] = true;
//                 if(!(bool)memory["para"]) {
//                     result.StillHaveParaHeals = false;
//                 }
//             }
//             lock(writeLock) {
//                 result.Final = (new List<bool>(result.FightResults).Where(item => !item).Count()) == 0;
//                 Results.Add(result);
//                 Console.WriteLine("sim: #{0} finished", curSim);
//             }
//         });

//         if(write) {
//             File.WriteAllText(name + ".json", JsonConvert.SerializeObject(Results));
//         }
//         gbs[0].Dispose();
//     }
// }

// public class SilphSimulations : SilphSimulation<Red, SilphResult> {

//     public SilphSimulations() { }

//     public bool Arbok(Red gb, Dictionary<string, object> memory) {
//         RbyPokemon battleMon = gb.BattleMon;
//         RbyBag bag = gb.Bag;
//         if(!battleMon.XAccuracyEffect) {
//             gb.UseItem("X ACCURACY");
//         } else if(battleMon.Paralyzed) {
//             if(!bag.Contains("PARLYZ HEAL")) {
//                 return false;
//             }
//             gb.UseItem("PARLYZ HEAL", 0);
//         } else {
//             gb.UseMove1();
//         }
//         gb.ClearText(false);

//         return true;
//     }

//     public bool ArbokThrash(Red gb, Dictionary<string, object> memory) {
//         gb.UseMove2();
//         gb.ClearText(false);
//         return true;
//     }

//     public bool SilphRival(Red gb, Dictionary<string, object> memory) {
//         RbyPokemon enemyMon = gb.EnemyMon;
//         RbyPokemon battleMon = gb.BattleMon;
//         RbyBag bag = gb.Bag;

//         if(enemyMon.Species.Name == "PIDGEOT") {
//             if(!battleMon.XAccuracyEffect) {
//                 gb.UseItem("X ACCURACY");
//             } else if(battleMon.SpeedModifider == 7) {
//                 gb.UseItem("X SPEED");
//             } else if(battleMon.SpecialModifider == 7) {
//                 // perfect setup
//                 // if(battleMon.HP <= 100)
//                 gb.UseItem("X SPECIAL");
//             } else {
//                 gb.UseMove1();
//             }
//         } else if(enemyMon.Species.Name == "GYARADOS") {
//             if(battleMon.SpecialModifider == 8) {
//                 if(battleMon.HP >= 100) {
//                     gb.UseItem("POKé FLUTE");
//                 } else if(battleMon.HP >= 89) {
//                     gb.UseItem("POTION", 0); // 0 index?
//                 } else if(battleMon.HP >= 76) {
//                     gb.UseItem("X SPECIAL");
//                 } else if(battleMon.HP >= 52) {
//                     gb.UseItem("SUPER POTION", 0);
//                 } else {
//                     gb.UseMove1();
//                 }
//             } else {
//                 if(battleMon.HP >= 100) {
//                     gb.UseItem("X SPECIAL");
//                 } else {
//                     gb.UseMove1();
//                 }
//             }
//         } else if(enemyMon.Species.Name == "GROWLITHE") {
//             gb.UseMove4();
//         } else {
//             gb.UseMove1();
//         }
//         gb.ClearText(false);

//         return true;
//     }

//     public bool SilphRivalNormal(Red gb, Dictionary<string, object> memory) {
//         RbyPokemon enemyMon = gb.EnemyMon;
//         RbyPokemon battleMon = gb.BattleMon;
//         RbyBag bag = gb.Bag;

//         if(enemyMon.Species.Name == "PIDGEOT") {
//             if(!battleMon.XAccuracyEffect) {
//                 gb.UseItem("X ACCURACY");
//             } else if(battleMon.SpeedModifider == 7) {
//                 gb.UseItem("X SPEED");
//             } else {
//                 gb.UseMove1();
//             }
//         } else {
//             gb.UseMove1();
//         }
//         gb.ClearText(false);

//         return true;
//     }

//     public bool CuboneRocket(Red gb, Dictionary<string, object> memory) {
//         RbyPokemon enemyMon = gb.EnemyMon;
//         RbyPokemon battleMon = gb.BattleMon;
//         RbyBag bag = gb.Bag;

//         if(!battleMon.XAccuracyEffect) {
//             gb.UseItem("X ACCURACY");
//         } else if(enemyMon.Species.Name == "CUBONE") {
//             gb.UseMove4();
//         } else {
//             gb.UseMove1();
//         }
//         gb.ClearText(false);

//         return true;
//     }

//     public bool CuboneRocketNormal(Red gb, Dictionary<string, object> memory) {
//         RbyPokemon enemyMon = gb.EnemyMon;
//         RbyPokemon battleMon = gb.BattleMon;
//         RbyBag bag = gb.Bag;

//         if(!battleMon.XAccuracyEffect) {
//             gb.UseItem("X ACCURACY");
//         } else if(enemyMon.Species.Name == "CUBONE") {
//             if(battleMon.HP < 78 && bag["ELIXER"].Quantity == 3) {
//                 gb.UseItem("ELIXER", 0);
//             } else {
//                 gb.UseMove4();
//             }
//         } else if(enemyMon.Species.Name == "DROWZEE") {
//             if(battleMon.HP >= 78 && bag["ELIXER"].Quantity == 3) {
//                 gb.UseItem("ELIXER", 0);
//             } else {
//                 gb.UseMove1();
//             }
//         } else {
//             gb.UseMove1();
//         }
//         gb.ClearText(false);

//         return true;
//     }

//     public bool SilphGio(Red gb, Dictionary<string, object> memory) {
//         RbyPokemon enemyMon = gb.EnemyMon;
//         RbyPokemon battleMon = gb.BattleMon;
//         RbyBag bag = gb.Bag;

//         if(!battleMon.XAccuracyEffect) {
//             gb.UseItem("X ACCURACY");
//         } else if(enemyMon.Species.Name == "RHYHORN") {
//             if(battleMon.PP[2] == 0) {
//                 gb.UseMove4();
//             } else if(battleMon.HP <= 37 && battleMon.HP >= 24 && battleMon.DefenseModifider == 7) {
//                 gb.UseMove3();
//             } else if(battleMon.HP <= 37 && battleMon.HP >= 29 && battleMon.DefenseModifider == 6) {
//                 gb.UseMove3();
//             } else {
//                 gb.UseMove4();
//             }
//         } else {
//             gb.UseMove1();
//         }
//         gb.ClearText(false);

//         return true;
//     }

//     public bool Juggler1(Red gb, Dictionary<string, object> memory) {
//         RbyPokemon battleMon = gb.BattleMon;

//         if(battleMon.PP[1] == 0) return false;
//         if(gb.CpuRead("wPlayerDisabledMoveNumber") == gb.Moves["EARTHQUAKE"].Id) {
//             gb.UseMove3();
//         } else {
//             gb.UseMove2();
//         }
//         gb.ClearText(false);
//         return true;
//     }

//     public bool Hypno(Red gb, Dictionary<string, object> memory) {
//         RbyPokemon enemyMon = gb.EnemyMon;
//         RbyPokemon battleMon = gb.BattleMon;
//         RbyBag bag = gb.Bag;

//         if(battleMon.PP[1] == 0) return false;

//         if(enemyMon.HP == enemyMon.MaxHP) {
//             if(gb.CpuRead("wPlayerDisabledMoveNumber") == gb.Moves["EARTHQUAKE"].Id) {
//                 gb.UseMove3();
//             } else {
//                 gb.UseMove2();
//             }
//         } else {
//             if(gb.CpuRead("wPlayerDisabledMoveNumber") == gb.Moves["THUNDERBOLT"].Id) {
//                 gb.UseMove2();
//             } else {
//                 gb.UseMove3();
//             }
//         }
//         gb.ClearText(false);
//         return true;
//     }

//     public bool Koga(Red gb, Dictionary<string, object> memory) {
//         RbyPokemon enemyMon = gb.EnemyMon;
//         RbyPokemon battleMon = gb.BattleMon;
//         RbyBag bag = gb.Bag;

//         if(enemyMon.Species.Name == "WEEZING") {
//             if(bag["ELIXER"].Quantity == 3) {
//                 gb.UseItem("ELIXER", 0);
//             } else if(bag["X SPECIAL"].Quantity >= 5) {
//                 gb.UseItem("X SPECIAL");
//             } else {
//                 gb.UseMove4();
//             }
//         } else {
//             if(battleMon.PP[1] == 0) return false;
//             if(gb.CpuRead("wPlayerDisabledMoveNumber") == gb.Moves["EARTHQUAKE"].Id) return false;
//             gb.UseMove2();
//         }
//         gb.ClearText(false);
//         return true;
//     }

//     public override void TransformInitialState(Red gb, ushort hp, ref byte[] state) {
//         gb.LoadState(state);
//         gb.AdvanceFrame();
//         gb.CpuWrite("wBattleMonPP", 0xFF);
//         gb.CpuWrite("wPartyMon1PP", 0xFF);
//         gb.CpuWriteBE<ushort>("wPartyMon1HP", hp);
//         gb.CpuWriteBE<ushort>("wBattleMonHP", hp);
//         gb.RunUntil("PlayCry");
//         gb.RunUntil("Joypad");
//         state = gb.SaveState();
//     }

//     public override bool HasSimulationEnded(Red gb, Dictionary<string, object> memory) {
//         return gb.BattleMon.HP == 0 || gb.CpuRead("wIsInBattle") == 0;
//     }

//     public override void SimulationStart(Red gb, Dictionary<string, object> memory) {
//         memory["starthp"] = gb.BattleMon.HP;
//     }

//     public override void SimulationEnd(Red gb, Dictionary<string, object> memory, ref SilphResult result) {
//         memory["hpLeft"] = gb.BattleMon.HP;
//         memory["para"] = gb.Bag.Contains("PARLYZ HEAL");
//         result.HpLeft = gb.BattleMon.HP;
//         result.Paralyzed = gb.BattleMon.Paralyzed;
//     }
// }

