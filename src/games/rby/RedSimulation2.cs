using System;
using System.Collections.Generic;

public class RedResult2 : SilphResult {

    public ushort HpLeft;
    public bool Paralyzed;
}

public class RedSimulation2 : SilphSimulation<Red, RedResult2> {

    public RedSimulation2() { }

    public bool Arbok(Red gb, Dictionary<string, object> memory) {
        RbyPokemon battleMon = gb.BattleMon;
        RbyBag bag = gb.Bag;
        if(!battleMon.XAccuracyEffect) {
            gb.UseItem("X ACCURACY");
        } else if(battleMon.Paralyzed) {
            if(!bag.Contains("PARLYZ HEAL")) {
                return false;
            }
            gb.UseItem("PARLYZ HEAL", 0);
        } else {
            gb.UseMove1();
        }
        gb.ClearText(false);

        return true;
    }

    public bool ArbokThrash(Red gb, Dictionary<string, object> memory) {
        gb.UseMove2();
        gb.ClearText(false);
        return true;
    }

    public bool SilphRival(Red gb, Dictionary<string, object> memory) {
        RbyPokemon enemyMon = gb.EnemyMon;
        RbyPokemon battleMon = gb.BattleMon;
        RbyBag bag = gb.Bag;

        if(enemyMon.Species.Name == "PIDGEOT") {
            if(!battleMon.XAccuracyEffect) {
                gb.UseItem("X ACCURACY");
            } else if(battleMon.SpeedModifider == 7) {
                gb.UseItem("X SPEED");
            } else if(battleMon.SpecialModifider == 7) {
                // perfect setup
                // if(battleMon.HP <= 100)
                gb.UseItem("X SPECIAL");
            } else {
                gb.UseMove1();
            }
        } else if(enemyMon.Species.Name == "GYARADOS") {
            if(battleMon.SpecialModifider == 8) {
                if(battleMon.HP >= 100) {
                    gb.UseItem("POKé FLUTE");
                } else if(battleMon.HP >= 89) {
                    gb.UseItem("POTION", 0); // 0 index?
                } else if(battleMon.HP >= 76) {
                    gb.UseItem("X SPECIAL");
                } else if(battleMon.HP >= 52) {
                    gb.UseItem("SUPER POTION", 0);
                } else {
                    gb.UseMove1();
                }
            } else {
                if(battleMon.HP >= 100) {
                    gb.UseItem("X SPECIAL");
                } else {
                    gb.UseMove1();
                }
            }
        } else if(enemyMon.Species.Name == "GROWLITHE") {
            gb.UseMove4();
        } else {
            gb.UseMove1();
        }
        gb.ClearText(false);

        return true;
    }

    public bool SilphRivalNormal(Red gb, Dictionary<string, object> memory) {
        RbyPokemon enemyMon = gb.EnemyMon;
        RbyPokemon battleMon = gb.BattleMon;
        RbyBag bag = gb.Bag;

        if(enemyMon.Species.Name == "PIDGEOT") {
            if(!battleMon.XAccuracyEffect) {
                gb.UseItem("X ACCURACY");
            } else if(battleMon.SpeedModifider == 7) {
                gb.UseItem("X SPEED");
            } else {
                gb.UseMove1();
            }
        } else {
            gb.UseMove1();
        }
        gb.ClearText(false);

        return true;
    }

    public bool CuboneRocket(Red gb, Dictionary<string, object> memory) {
        RbyPokemon enemyMon = gb.EnemyMon;
        RbyPokemon battleMon = gb.BattleMon;
        RbyBag bag = gb.Bag;

        if(!battleMon.XAccuracyEffect) {
            gb.UseItem("X ACCURACY");
        } else if(enemyMon.Species.Name == "CUBONE") {
            gb.UseMove4();
        } else {
            gb.UseMove1();
        }
        gb.ClearText(false);

        return true;
    }

    public bool CuboneRocketNormal(Red gb, Dictionary<string, object> memory) {
        RbyPokemon enemyMon = gb.EnemyMon;
        RbyPokemon battleMon = gb.BattleMon;
        RbyBag bag = gb.Bag;

        if(!battleMon.XAccuracyEffect) {
            gb.UseItem("X ACCURACY");
        } else if(enemyMon.Species.Name == "CUBONE") {
            if(battleMon.HP < 78 && bag["ELIXER"].Quantity == 3) {
                gb.UseItem("ELIXER", 0);
            } else {
                gb.UseMove4();
            }
        } else if(enemyMon.Species.Name == "DROWZEE") {
            if(battleMon.HP >= 78 && bag["ELIXER"].Quantity == 3) {
                gb.UseItem("ELIXER", 0);
            } else {
                gb.UseMove1();
            }
        } else {
            gb.UseMove1();
        }
        gb.ClearText(false);

        return true;
    }

    public bool SilphGio(Red gb, Dictionary<string, object> memory) {
        RbyPokemon enemyMon = gb.EnemyMon;
        RbyPokemon battleMon = gb.BattleMon;
        RbyBag bag = gb.Bag;

        if(!battleMon.XAccuracyEffect) {
            gb.UseItem("X ACCURACY");
        } else if(enemyMon.Species.Name == "RHYHORN") {
            if(battleMon.PP[2] == 0) {
                gb.UseMove4();
            } else if(battleMon.HP <= 37 && battleMon.HP >= 24 && battleMon.DefenseModifider == 7) {
                gb.UseMove3();
            } else if(battleMon.HP <= 37 && battleMon.HP >= 29 && battleMon.DefenseModifider == 6) {
                gb.UseMove3();
            } else {
                gb.UseMove4();
            }
        } else {
            gb.UseMove1();
        }
        gb.ClearText(false);

        return true;
    }

    public bool Juggler1(Red gb, Dictionary<string, object> memory) {
        RbyPokemon battleMon = gb.BattleMon;

        if(battleMon.PP[1] == 0) return false;
        if(gb.CpuRead("wPlayerDisabledMoveNumber") == gb.Moves["EARTHQUAKE"].Id) {
            gb.UseMove3();
        } else {
            gb.UseMove2();
        }
        gb.ClearText(false);
        return true;
    }

    public bool Hypno(Red gb, Dictionary<string, object> memory) {
        RbyPokemon enemyMon = gb.EnemyMon;
        RbyPokemon battleMon = gb.BattleMon;
        RbyBag bag = gb.Bag;

        if(battleMon.PP[1] == 0) return false;

        if(enemyMon.HP == enemyMon.MaxHP) {
            if(gb.CpuRead("wPlayerDisabledMoveNumber") == gb.Moves["EARTHQUAKE"].Id) {
                gb.UseMove3();
            } else {
                gb.UseMove2();
            }
        } else {
            if(gb.CpuRead("wPlayerDisabledMoveNumber") == gb.Moves["THUNDERBOLT"].Id) {
                gb.UseMove2();
            } else {
                gb.UseMove3();
            }
        }
        gb.ClearText(false);
        return true;
    }

    public bool Koga(Red gb, Dictionary<string, object> memory) {
        RbyPokemon enemyMon = gb.EnemyMon;
        RbyPokemon battleMon = gb.BattleMon;
        RbyBag bag = gb.Bag;

        if(enemyMon.Species.Name == "WEEZING") {
            if(bag["ELIXER"].Quantity == 3) {
                gb.UseItem("ELIXER", 0);
            } else if(bag["X SPECIAL"].Quantity >= 5) {
                gb.UseItem("X SPECIAL");
            } else {
                gb.UseMove4();
            }
        } else {
            if(battleMon.PP[1] == 0) return false;
            if(gb.CpuRead("wPlayerDisabledMoveNumber") == gb.Moves["EARTHQUAKE"].Id) return false;
            gb.UseMove2();
        }
        gb.ClearText(false);
        return true;
    }

    public override void TransformInitialState(Red gb, ushort hp, ref byte[] state) {
        gb.LoadState(state);
        gb.AdvanceFrame();
        gb.CpuWrite("wBattleMonPP", 0xFF);
        gb.CpuWrite("wPartyMon1PP", 0xFF);
        gb.CpuWriteBE<ushort>("wPartyMon1HP", hp);
        gb.CpuWriteBE<ushort>("wBattleMonHP", hp);
        gb.RunUntil("PlayCry");
        gb.RunUntil("Joypad");
        state = gb.SaveState();
    }

    public override bool HasSimulationEnded(Red gb, Dictionary<string, object> memory) {
        return gb.BattleMon.HP == 0 || gb.CpuRead("wIsInBattle") == 0;
    }

    public override void SimulationStart(Red gb, Dictionary<string, object> memory) {
        memory["starthp"] = gb.BattleMon.HP;
    }

    public override void SimulationEnd(Red gb, Dictionary<string, object> memory, ref RedResult2 result) {
        memory["hpLeft"] = gb.BattleMon.HP;
        memory["para"] = gb.Bag.Contains("PARLYZ HEAL");
        result.HpLeft = gb.BattleMon.HP;
        result.Paralyzed = gb.BattleMon.Paralyzed;
    }
}
