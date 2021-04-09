using System;
using System.Collections.Generic;

public class YellowResult : SimulationResult {

    public bool Victory {
        get { return HpLeft > 0; }
    }

    public ushort HpLeft;
}

public class YellowSimulation : FightSimulation<Yellow, YellowResult> {

    public ushort StartHP;
    public YellowSimulation(ushort startHP) => StartHP = startHP;

    public YellowSimulation() { StartHP = 65535; }

    public bool BugCatcher(Yellow gb, Dictionary<string, object> memory) {
        gb.UseMove(2);
        gb.ClearText();

        return true;
    }

    public override void TransformInitialState(Yellow gb, ref byte[] state) {
        gb.LoadState(state);
        gb.AdvanceFrame();
        if(StartHP != 65535 && StartHP != gb.BattleMon.HP) {
            gb.CpuWriteBE<ushort>("wPartyMon1HP", StartHP);
            gb.CpuWriteBE<ushort>("wBattleMonHP", StartHP);
        }
        gb.RunUntil("PlayCry");
        gb.RunUntil("Joypad");
        state = gb.SaveState();
    }

    public override bool HasSimulationEnded(Yellow gb, Dictionary<string, object> memory) {
        return gb.BattleMon.HP == 0 || gb.CpuRead("wIsInBattle") == 0;
    }

    public override void SimulationStart(Yellow gb, Dictionary<string, object> memory) {
        memory["starthp"] = gb.BattleMon.HP;
    }

    public override void SimulationEnd(Yellow gb, Dictionary<string, object> memory, ref YellowResult result) {
        memory["hpLeft"] = gb.BattleMon.HP;
        result.HpLeft = gb.BattleMon.HP;
    }
}
