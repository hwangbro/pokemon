using System;
using System.Collections.Generic;
using System.Linq;

public class RbyIntroStrat {

    public string Name;
    public int Cost;
    public int[] Addrs;
    public Joypad[] Inputs;
    public int[] AdvanceFrames;

    public RbyIntroStrat(string name, int cost, int[] addrs, Joypad[] inputs, int[] advanceFrames) => (Name, Cost, Addrs, Inputs, AdvanceFrames) = (name, cost, addrs, inputs, advanceFrames);

    public virtual void Execute(Rby game) {
        for(int i = 0; i < Addrs.Length; i++) {
            game.RunUntil(Addrs[i]);
            game.Inject(Inputs[i]);
            for(int j = 0; j < AdvanceFrames[i]; j++) {
                game.AdvanceFrame();
            }
        }
    }
}

public class RbyPalStrat : RbyIntroStrat {

    public RbyPalStrat(string name, int cost, int[] addrs, Joypad[] inputs, int[] advanceFrames) : base(name, cost, addrs, inputs, advanceFrames) { }

    public override void Execute(Rby game) {
        for(int i = 0; i < Addrs.Length; i++) {
            game.Hold(Inputs[i], Addrs[i]);
            for(int j = 0; j < AdvanceFrames[i]; j++) {
                game.AdvanceFrame(Inputs[i]);
            }
        }
    }
}

public class RbyIntroSequence : List<RbyIntroStrat>, IComparable<RbyIntroSequence> {

    public int Cost {
        get { return this.Sum(x => x.Cost); }
    }

    public override string ToString() {
        string ret = "";
        foreach(RbyIntroStrat strat in this) {
            ret += strat.Name;
        }
        return ret;
    }

    public int CompareTo(RbyIntroSequence other) {
        return Cost - other.Cost;
    }

}