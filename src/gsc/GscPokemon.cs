using System;
public class GscPokemon {
    public Gsc Game;
    public GscSpecies Species;
    public GscItem Item;
    public GscMove[] Moves;
    public ushort DVs;
    public byte[] PP;
    public byte Happiness;
    public byte Level;
    public byte Status1;
    public byte Status2;
    public ushort HP;
    public ushort MaxHP;
    public ushort Attack;
    public ushort Defense;
    public ushort Speed;
    public ushort SpecialAttack;
    public ushort SpecialDefense;
    public GscType Type1;
    public GscType Type2;
    public bool Enemy;

    public bool Asleep {
        get { return SleepCounter != 0; }
    }

    public byte SleepCounter {
        get { return (byte) (Status1 & 0b111); }
    }

    public bool Poisoned {
        get { return (Status1 & (1 << 3)) != 0; }
    }

    public bool Burned {
        get { return (Status1 & (1 << 4)) != 0; }
    }

    public bool Frozen {
        get { return (Status1 & (1 << 5)) != 0; }
    }

    public bool Paralyzed {
        get { return (Status1 & (1 << 6)) != 0; }
    }

    public byte AtkStage {
        get { return Enemy ? Game.CpuRead("wEnemyAtkLevel") :
                             Game.CpuRead("wPlayerAtkLevel"); }
    }

    public byte DefStage {
        get { return Enemy ? Game.CpuRead("wEnemyDefLevel") :
                             Game.CpuRead("wPlayerDefLevel"); }
    }

    public byte SpdStage {
        get { return Enemy ? Game.CpuRead("wEnemySpdLevel") :
                             Game.CpuRead("wPlayerSpdLevel"); }
    }

    public byte SAtkStage {
        get { return Enemy ? Game.CpuRead("wEnemySAtkLevel") :
                             Game.CpuRead("wPlayerSAtkLevel"); }
    }

    public byte SDefStage {
        get { return Enemy ? Game.CpuRead("wEnemySDefLevel") :
                             Game.CpuRead("wPlayerSDefLevel"); }
    }

    public byte AccStage {
        get { return Enemy ? Game.CpuRead("wEnemyAccLevel") :
                             Game.CpuRead("wPlayerAccLevel"); }
    }

    public byte EvaStage {
        get { return Enemy ? Game.CpuRead("wEnemyEvaLevel") :
                             Game.CpuRead("wPlayerEvaLevel"); }
    }

    public bool Confused {
        get { return Enemy ? Game.CpuRead("wEnemySubStatus3") >> 7 == 1 :
                             Game.CpuRead("wPlayerSubStatus3") >> 7 == 1; }
    }

    public bool XAccSetup {
        get { return Enemy ? (Game.CpuRead("wEnemySubStatus4") & 0x01) == 1 :
                             (Game.CpuRead("wPlayerSubStatus4") & 0x01) == 1; }
    }

    public GscPokemon(Gsc game, bool enemy = false) {
        Game = game;
        Enemy = enemy;
        string addrName = enemy ? "wEnemyMon" : "wBattleMon";
        int addr = game.SYM[addrName];
        Species = game.Species[game.CpuRead(addr)];
        Item = game.Items[game.CpuRead(addr + 1)];
        Moves = new GscMove[4] { game.Moves[game.CpuRead(addr+2)],
                                    game.Moves[game.CpuRead(addr+3)],
                                    game.Moves[game.CpuRead(addr+4)],
                                    game.Moves[game.CpuRead(addr+5)] };
        DVs = enemy ? (ushort) 0x9888 : (ushort) game.CpuReadWord(addr+6);
        PP = new byte[4] { game.CpuRead(addr+8),
                            game.CpuRead(addr+9),
                            game.CpuRead(addr+10),
                            game.CpuRead(addr+11) };
        Happiness = game.CpuRead(addr+12);
        Level = game.CpuRead(addr+13);
        Status1 = game.CpuRead(addr+14);
        Status2 = game.CpuRead(addr+15);
        HP = (ushort) (game.CpuRead(addr+16) << 16 | game.CpuRead(addr+17));
        MaxHP = (ushort) (game.CpuRead(addr+18) << 16 | game.CpuRead(addr+19));
        Attack = (ushort) (game.CpuRead(addr+20) << 16 | game.CpuRead(addr+21));
        // Attack = CalcStat(DVs.Attack, Species.BaseAttack, 0, 5);
        Defense = (ushort) (game.CpuRead(addr+22) << 16 | game.CpuRead(addr+23));
        Speed = (ushort) (game.CpuRead(addr+24) << 16 | game.CpuRead(addr+25));
        SpecialAttack = (ushort) (game.CpuRead(addr+26) << 16 | game.CpuRead(addr+27));
        SpecialDefense = (ushort) (game.CpuRead(addr+28) << 16 | game.CpuRead(addr+29));
        Type1 = (GscType) game.CpuRead(addr+30);
        Type2 = (GscType) game.CpuRead(addr+31);
    }
}