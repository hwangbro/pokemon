// using System.Collections.Generic;
// using System;

// public class GscResult : SimulationResult {
//     public bool Victory {
//         get { return HpLeft > 0; }
//     }
//     public ushort HpLeft;
//     public int UmbreonTurns;
//     public bool KenyaSwap;
// }

// public class GscSimulation : FightSimulation<Crystal, GscResult> {
//     public ushort StartHP;
//     public GscSimulation(ushort startHP) => StartHP = startHP;

//     public bool xacc(Crystal gb, Dictionary<string, object> memory) {
//         GscPokemon enemyMon = gb.EnemyMon;
//         GscPokemon battleMon = gb.BattleMon;
//         GscPocket bag = gb.Bag.Items;

//         // 93 - 169

//         if(enemyMon.Species.Name == "UMBREON") {
//             memory["umbreonTurns"] = (int) memory["umbreonTurns"] + 1;
//             if(battleMon.SpecialAttackModifider != 9) {
//                 gb.UseItem("X SPECIAL");
//             } else if(!battleMon.XAccuracyEffect && battleMon.AccuracyModifider != 7) {
//                 gb.UseItem("X ACCURACY");
//             } else if(battleMon.Confused) {
//                 if(bag.Contains("FULL HEAL")) {
//                     gb.UseItem("FULL HEAL", 1);
//                 } else {
//                     gb.UseItem("FULL RESTORE", 1);
//                 }
//             } else {
//                 gb.UseMove(4);
//             }
//         } else if(enemyMon.Species.Name == "VILEPLUME") {
//             gb.UseMove(4);
//         } else if(enemyMon.Species.Name == "MURKROW") {
//             gb.UseMove(2);
//         } else {
//             gb.UseMove(1);
//         }

//         gb.ClearText();

//         return true;
//     }

//     public bool xacclowhp(Crystal gb, Dictionary<string, object> memory) {
//         GscPokemon enemyMon = gb.EnemyMon;
//         GscPokemon battleMon = gb.BattleMon;
//         GscPocket bag = gb.Bag.Items;

//         // 52-92
//         List<string> sparkPokemon = new List<string> {"HOUNDOOM", "GENGAR"};

//         if(enemyMon.Species.Name == "UMBREON") {
//             memory["umbreonTurns"] = (int) memory["umbreonTurns"] + 1;
//             if(enemyMon.HP == enemyMon.MaxHP) {
//                 gb.UseMove(1);
//             } else if(battleMon.SpecialAttackModifider != 9) {
//                 if(battleMon.HP < 18) {
//                     gb.UseItem("FULL RESTORE", 1);
//                 } else {
//                     gb.UseItem("X SPECIAL");
//                 }
//             } else if(!battleMon.XAccuracyEffect && battleMon.AccuracyModifider != 7) {
//                 gb.UseItem("X ACCURACY");
//             } else if(battleMon.Confused) {
//                 if(bag.Contains("FULL HEAL")) {
//                     gb.UseItem("FULL HEAL", 1);
//                 } else {
//                     gb.UseItem("FULL RESTORE", 1);
//                 }
//             } else {
//                 gb.UseMove(1);
//             }
//         } else if(sparkPokemon.Contains(enemyMon.Species.Name)) {
//             gb.UseMove(1);
//         } else if(enemyMon.Species.Name == "VILEPLUME") {
//             gb.UseMove(4);
//         } else if(enemyMon.Species.Name == "MURKROW") {
//             if(battleMon.HP < 12) {
//                 gb.UseItem("FULL RESTORE", 1);
//             } else {
//                 gb.UseMove(2);
//             }
//         }

//         gb.ClearText();

//         return true;
//     }

//     public bool noxacc(Crystal gb, Dictionary<string, object> memory) {
//         // spark, xspec*2, spark, hp, spark, spark, ts
//         // sa turn 1, swap kenya  (spam move 2), swap back
//         // sand attacked during setup, yolo hit through
//         // FR on murkrow if < 12 hp
//         // going into houndoom and sanded, less than 3 sparks, strength *2
//         //   if more than 3, use spark until you have 2, then strength
//         // gengar, if less than 4 sparks, HP if you have 4. if you have 3, then spark
//         // if out of pp, gg

//         // 52-92

//         GscPokemon enemyMon = gb.EnemyMon;
//         GscPokemon battleMon = gb.BattleMon;
//         GscPocket bag = gb.Bag.Items;

//         if(battleMon.Species.Name == "SPEAROW") {
//             memory["umbreonTurns"] = (int) memory["umbreonTurns"] + 1;
//             gb.UseMove(2);
//             gb.ClearText();
//             if(gb.BattleMon.HP == 0) {
//                 gb.Swap(1);
//                 gb.ClearText();
//             }
//         } else if(enemyMon.Name == "UMBREON") {
//             memory["umbreonTurns"] = (int) memory["umbreonTurns"] + 1;
//             if(enemyMon.HP == enemyMon.MaxHP) {
//                 gb.UseMove(1);
//             } else if(battleMon.SpecialAttackModifider == 7 && battleMon.AccuracyModifider != 7) {
//                 memory["kenyaSwap"] = true;
//                 gb.Swap(2);
//                 gb.ClearText();
//                 if(gb.BattleMon.HP == 0) {
//                     memory["umbreonTurns"] = (int) memory["umbreonTurns"] + 1;
//                     gb.Swap(1);
//                 }
//             } else if(battleMon.HP < 18) {
//                 if(battleMon.SpecialAttackModifider == 9 && battleMon.AccuracyModifider == 7) {
//                     if(battleMon.PP[0] == 0) return false;
//                     gb.UseMove(1);
//                 } else {
//                     gb.UseItem("FULL RESTORE", 1);
//                 }
//             } else if(battleMon.SpecialAttackModifider != 9) {
//                 gb.UseItem("X SPECIAL");
//             } else if(battleMon.Confused) {
//                 if(bag.Contains("FULL HEAL")) {
//                     gb.UseItem("FULL HEAL", 1);
//                 } else {
//                     gb.UseItem("FULL RESTORE", 1);
//                 }
//             } else {
//                 if(battleMon.PP[0] == 0) {
//                     return false;
//                 }
//                 gb.UseMove(1);
//             }
//         } else if(enemyMon.Name == "VILEPLUME") {
//             if(battleMon.PP[3] < 3) return false;
//             gb.UseMove(4);
//         } else if(enemyMon.Name == "GENGAR") {
//             if(battleMon.AccuracyModifider != 7) {
//                 if(battleMon.PP[3] > 3) {
//                     gb.UseMove(4);
//                 } else {
//                     if(battleMon.PP[0] <= 2) {
//                         if(battleMon.PP[1] == 0) return false;
//                         gb.UseMove(2);
//                     } else {
//                         gb.UseMove(1);
//                     }
//                 }
//             } else {
//                 gb.UseMove(1);
//             }
//         } else if(enemyMon.Name == "HOUNDOOM") { //43-51
//             // if 2 sparks/3 hp, double str houndoom
//             if(battleMon.AccuracyModifider < 7) {
//                 if(battleMon.PP[0] <= 2) {
//                     if(battleMon.HP < 61) {
//                         gb.UseItem("FULL RESTORE", 1);
//                     } else if(battleMon.PP[2] == 0) {
//                         if(battleMon.PP[1] == 0) return false;
//                         gb.UseMove(2);
//                     } else {
//                         gb.UseMove(3);
//                     }
//                 } else {
//                     gb.UseMove(1);
//                 }
//             } else {
//                 gb.UseMove(1);
//             }
//         } else if(enemyMon.Name == "MURKROW") {
//             // if(enemyMon.HP == 0) return true;
//             if(battleMon.HP < 12) {
//                 gb.UseItem("FULL RESTORE", 1);
//             } else {
//                 if(battleMon.PP[1] == 0) return false;
//                 gb.UseMove(2);
//             }
//         }

//         gb.ClearText();

//         return true;
//     }

//     public override void TransformInitialState(Crystal gb, ref byte[] state) {
//         gb.LoadState(state);
//         gb.AdvanceFrame(); // adjust?
//         gb.CpuWriteBE<ushort>("wPartyMon1HP", StartHP);
//         gb.CpuWriteBE<ushort>("wBattleMonHP", StartHP);
//         state = gb.SaveState();
//     }

//     public override bool HasSimulationEnded(Crystal gb, Dictionary<string, object> memory) {
//         return gb.BattleMon.HP == 0 || gb.CpuRead("wSpriteUpdatesEnabled") == 1 || gb.CpuRead("wBattleEnded") == 1;
//     }

//     public override void SimulationStart(Crystal gb, Dictionary<string, object> memory) {
//         memory["starthp"] = gb.BattleMon.HP;
//         memory["umbreonTurns"] = 0;
//         memory["kenyaSwap"] = false;
//         gb.RunUntil("PlayCry");
//         gb.RunUntil("GetJoypad");
//     }

//     public override void SimulationEnd(Crystal gb, Dictionary<string, object> memory, ref GscResult result) {
//         result.HpLeft = gb.BattleMon.HP;
//         result.UmbreonTurns = (int) memory["umbreonTurns"];
//         result.KenyaSwap = (bool) memory["kenyaSwap"];
//     }
// }
