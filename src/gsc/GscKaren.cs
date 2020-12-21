using System.Collections.Generic;
using System;

public class KarenResult : SimulationResult {
    public bool Victory {
        get { return HpLeft > 0; }
    }

    public ushort HpLeft;
    // public ushort DamageTaken;
    public byte FullRestoresUsed;
}

public class GscKaren : GscFightSimulation<Crystal, KarenResult> {
    public ushort StartHP;
    public GscKaren(ushort startHP) => StartHP = startHP;

    public bool xacc(Crystal gb, Dictionary<string, object> memory) {
        GscPokemon enemyMon = gb.GetBattleMon(true);
        GscPokemon battleMon = gb.GetBattleMon(false);
        Dictionary<string, byte> bag = gb.GetBag();


        byte lastSlot = 1;

        // confuseray check
        if(battleMon.Confused) {
            if(bag.ContainsKey("FULL HEAL")) {
                gb.UseItem("FULL HEAL", 1);
            } else {
                gb.UseItem("FULL RESTORE", 1);
            }
            lastSlot = 2;
        } else if(!battleMon.XAccSetup && battleMon.AccStage != 7) {
            gb.UseItem("X ACCURACY");
            lastSlot = 2;
        } else if(battleMon.SAtkStage != 9) {
            gb.UseItem("X SPECIAL");
            lastSlot = 2;
        } else if(enemyMon.Species.Name == "UMBREON" || enemyMon.Species.Name == "VILEPLUME") {
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

        byte lastSlot = 1;
        List<string> sparkPokemon = new List<string> {"HOUNDOOM", "GENGAR"};

        if(enemyMon.Species.Name == "UMBREON") {
            if(enemyMon.HP == enemyMon.MaxHP) {
                gb.UseMove(1);
            } else if (battleMon.HP < 18) {
                gb.UseItem("FULL RESTORE", 1);
                lastSlot = 2;
            } else if(battleMon.SAtkStage != 9) {
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
                gb.UseMove(1);
            }
        } else if(sparkPokemon.Contains(enemyMon.Species.Name)) {
            gb.UseMove(1);
        } else if(enemyMon.Species.Name == "VILEPLUME") {
            gb.UseMove(4);
        } else {
            gb.UseMove(2);
        }

        gb.ClearBattleText(lastSlot);

        return true;
    }

    public override void TransformInitialState(Crystal gb, ref byte[] state) {
        gb.LoadState(state);
        gb.AdvanceFrame(); // adjust?
        gb.CpuWriteWord(gb.SYM["wPartyMon1HP"], StartHP);
        gb.CpuWriteWord(gb.SYM["wBattleMonHP"], StartHP);
        gb.RunUntil("PlayCry");
        gb.RunUntil("GetJoypad");
        state = gb.SaveState();
    }

    public override bool HasSimulationEnded(Crystal gb, Dictionary<string, object> memory) {
        return gb.GetBattleMon(false).HP == 0 || gb.CpuRead("wSpriteUpdatesEnabled") == 1;
    }

    public override void SimulationStart(Crystal gb, Dictionary<string, object> memory) {
        memory["starthp"] = gb.GetBattleMon(false).HP;
    }

    public override void SimulationEnd(Crystal gb, Dictionary<string, object> memory, ref KarenResult result) {
        result.HpLeft = gb.GetBattleMon(false).HP;
    }
}