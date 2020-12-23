using System.Collections.Generic;
using System;

public class KarenResult : SimulationResult {
    public bool Victory {
        get { return HpLeft > 0; }
    }
    public ushort HpLeft;
    public int UmbreonTurns;
    public bool KenyaSwap;
}

public class GscKaren : GscFightSimulation<Crystal, KarenResult> {
    public ushort StartHP;
    public GscKaren(ushort startHP) => StartHP = startHP;

    public bool xacc(Crystal gb, Dictionary<string, object> memory) {
        GscPokemon enemyMon = gb.GetBattleMon(true);
        GscPokemon battleMon = gb.GetBattleMon(false);
        Dictionary<string, byte> bag = gb.GetBag();

        // 93 - 169

        byte lastSlot = 1;

        if(enemyMon.Name == "UMBREON") {
            memory["umbreonTurns"] = (int) memory["umbreonTurns"] + 1;
            if(battleMon.SAtkStage != 9) {
                gb.UseItem("X SPECIAL");
                lastSlot = 2;
            } else if(!battleMon.XAccSetup && battleMon.AccStage != 7) {
                gb.UseItem("X ACCURACY");
                lastSlot = 2;
            } else if(battleMon.Confused) {
                if(bag.ContainsKey("FULL HEAL")) {
                    gb.UseItem("FULL HEAL", 1);
                } else {
                    gb.UseItem("FULL RESTORE", 1);
                }
                lastSlot = 2;
            } else {
                gb.UseMove(4);
            }
        } else if(enemyMon.Species.Name == "VILEPLUME") {
            gb.UseMove(4);
        } else if(enemyMon.Species.Name == "MURKROW") {
            gb.UseMove(2);
        } else {
            gb.UseMove(1);
        }

        gb.ClearBattleText(lastSlot);

        return true;
    }

    public bool xacclowhp(Crystal gb, Dictionary<string, object> memory) {
        GscPokemon enemyMon = gb.GetBattleMon(true);
        GscPokemon battleMon = gb.GetBattleMon(false);
        Dictionary<string, byte> bag = gb.GetBag();

        // 52-92

        byte lastSlot = 1;
        List<string> sparkPokemon = new List<string> {"HOUNDOOM", "GENGAR"};

        if(enemyMon.Species.Name == "UMBREON") {
            memory["umbreonTurns"] = (int) memory["umbreonTurns"] + 1;
            if(enemyMon.FullHP) {
                gb.UseMove(1);
            } else if(battleMon.SAtkStage != 9) {
                if(battleMon.HP < 18) {
                    gb.UseItem("FULL RESTORE", 1);
                } else {
                    gb.UseItem("X SPECIAL");
                }
                lastSlot = 2;
            } else if(!battleMon.XAccSetup && battleMon.AccStage != 7) {
                gb.UseItem("X ACCURACY");
                lastSlot = 2;
            } else if(battleMon.Confused) {
                if(bag.ContainsKey("FULL HEAL")) {
                    gb.UseItem("FULL HEAL", 1);
                } else {
                    gb.UseItem("FULL RESTORE", 1);
                }
                lastSlot = 2;
            } else {
                gb.UseMove(1);
            }
        } else if(sparkPokemon.Contains(enemyMon.Species.Name)) {
            gb.UseMove(1);
        } else if(enemyMon.Species.Name == "VILEPLUME") {
            gb.UseMove(4);
        } else if(enemyMon.Species.Name == "MURKROW") {
            if(battleMon.HP < 12) {
                gb.UseItem("FULL RESTORE", 1);
                lastSlot = 2;
            } else {
                gb.UseMove(2);
            }
        }

        gb.ClearBattleText(lastSlot);
        // if(gb.GetBattleMon(false).HP == 0) {
        //     Console.WriteLine("asdfasdf");
        //     return false;
        // }

        return true;
    }

    public bool noxacc(Crystal gb, Dictionary<string, object> memory) {
        // spark, xspec*2, spark, hp, spark, spark, ts
        // sa turn 1, swap kenya  (spam move 2), swap back
        // sand attacked during setup, yolo hit through
        // FR on murkrow if < 12 hp
        // going into houndoom and sanded, less than 3 sparks, strength *2
        //   if more than 3, use spark until you have 2, then strength
        // gengar, if less than 4 sparks, HP if you have 4. if you have 3, then spark
        // if out of pp, gg

        // 52-92

        GscPokemon enemyMon = gb.GetBattleMon(true);
        GscPokemon battleMon = gb.GetBattleMon(false);
        Dictionary<string, byte> bag = gb.GetBag();
        byte lastSlot = 1;

        if(battleMon.Name == "SPEAROW") {
            memory["umbreonTurns"] = (int) memory["umbreonTurns"] + 1;
            gb.UseMove(2);
            gb.ClearBattleText();
            if(gb.GetBattleMon(false).HP == 0) {
                gb.Swap(1, true);
                gb.ClearBattleText();
            }
        } else if(enemyMon.Name == "UMBREON") {
            memory["umbreonTurns"] = (int) memory["umbreonTurns"] + 1;
            if(enemyMon.FullHP) {
                gb.UseMove(1);
            } else if(battleMon.SAtkStage == 7 && battleMon.AccStage != 7) {
                memory["kenyaSwap"] = true;
                gb.Swap(2);
                gb.ClearBattleText();
                if(gb.GetBattleMon(false).HP == 0) {
                    memory["umbreonTurns"] = (int) memory["umbreonTurns"] + 1;
                    gb.Swap(1, true);
                    gb.ClearBattleText();
                }
            } else if(battleMon.HP < 18) {
                if(battleMon.SAtkStage == 9 && battleMon.AccStage == 7) {
                    if(battleMon.PP[0] == 0) return false;
                    gb.UseMove(1);
                } else {
                    gb.UseItem("FULL RESTORE", 1);
                    lastSlot = 2;
                }
            } else if(battleMon.SAtkStage != 9) {
                gb.UseItem("X SPECIAL");
                lastSlot = 2;
            } else if(battleMon.Confused) {
                if(bag.ContainsKey("FULL HEAL")) {
                    gb.UseItem("FULL HEAL", 1);
                } else {
                    gb.UseItem("FULL RESTORE", 1);
                }
                lastSlot = 2;
            } else {
                if(battleMon.PP[0] == 0) {
                    return false;
                }
                gb.UseMove(1);
            }
        } else if(enemyMon.Name == "VILEPLUME") {
            if(battleMon.PP[3] < 3) return false;
            gb.UseMove(4);
        } else if(enemyMon.Name == "GENGAR") {
            if(battleMon.AccStage != 7) {
                if(battleMon.PP[3] > 3) {
                    gb.UseMove(4);
                } else {
                    if(battleMon.PP[0] <= 2) {
                        if(battleMon.PP[1] == 0) return false;
                        gb.UseMove(2);
                    } else {
                        gb.UseMove(1);
                    }
                }
            } else {
                gb.UseMove(1);
            }
        } else if(enemyMon.Name == "HOUNDOOM") { //43-51
            // if 2 sparks/3 hp, double str houndoom
            if(battleMon.AccStage < 7) {
                if(battleMon.PP[0] <= 2) {
                    if(battleMon.HP < 61) {
                        gb.UseItem("FULL RESTORE", 1);
                        lastSlot = 2;
                    } else if(battleMon.PP[2] == 0) {
                        if(battleMon.PP[1] == 0) return false;
                        gb.UseMove(2);
                    } else {
                        gb.UseMove(3);
                    }
                } else {
                    gb.UseMove(1);
                }
            } else {
                gb.UseMove(1);
            }
        } else if(enemyMon.Name == "MURKROW") {
            if(battleMon.HP < 12) {
                gb.UseItem("FULL RESTORE", 1);
                lastSlot = 2;
            } else {
                if(battleMon.PP[1] == 0) return false;
                gb.UseMove(2);
            }
        }

        gb.ClearBattleText(lastSlot);

        return true;
    }

    public override void TransformInitialState(Crystal gb, ref byte[] state) {
        gb.LoadState(state);
        gb.AdvanceFrame(); // adjust?
        gb.CpuWriteWord(gb.SYM["wPartyMon1HP"], StartHP);
        gb.CpuWriteWord(gb.SYM["wBattleMonHP"], StartHP);
        state = gb.SaveState();
    }

    public override bool HasSimulationEnded(Crystal gb, Dictionary<string, object> memory) {
        return gb.GetBattleMon(false).HP == 0 || gb.CpuRead("wSpriteUpdatesEnabled") == 1;
    }

    public override void SimulationStart(Crystal gb, Dictionary<string, object> memory) {
        memory["starthp"] = gb.GetBattleMon(false).HP;
        memory["umbreonTurns"] = 0;
        memory["kenyaSwap"] = false;
    }

    public override void SimulationEnd(Crystal gb, Dictionary<string, object> memory, ref KarenResult result) {
        result.HpLeft = gb.GetBattleMon(false).HP;
        result.UmbreonTurns = (int) memory["umbreonTurns"];
        result.KenyaSwap = (bool) memory["kenyaSwap"];
    }
}