using System;

public partial class Rby {

    public RbyPokemon BattleMon {
        get { return ReadBattleStruct(From("wBattleMon"), From("wPlayerBattleStatus1"), From("wPlayerMonStatMods"), "wPlayerMonUnmodified"); }
    }

    public RbyPokemon EnemyMon {
        get { return ReadBattleStruct(From("wEnemyMon"), From("wEnemyBattleStatus1"), From("wEnemyMonStatMods"), "wEnemyMonUnmodified"); }
    }

    public RbyPokemon PartyMon1 {
        get { return ReadPartyStruct(From("wPartyMon1")); }
    }

    public RbyPokemon PartyMon2 {
        get { return ReadPartyStruct(From("wPartyMon2")); }
    }

    public RbyPokemon PartyMon3 {
        get { return ReadPartyStruct(From("wPartyMon3")); }
    }

    public RbyPokemon PartyMon4 {
        get { return ReadPartyStruct(From("wPartyMon4")); }
    }

    public RbyPokemon PartyMon5 {
        get { return ReadPartyStruct(From("wPartyMon5")); }
    }

    public RbyPokemon PartyMon6 {
        get { return ReadPartyStruct(From("wPartyMon6")); }
    }

    public RbyPokemon PartyMon(int index) {
        return ReadPartyStruct(From(SYM["wPartyMons"] + index * (SYM["wPartyMon2"] - SYM["wPartyMon1"])));
    }

    public RbyPokemon BoxMon(int index) {
        return ReadPartyStruct(From(SYM["wBoxMons"] + index * (SYM["wBoxMon2"] - SYM["wBoxMon1"])));
    }

    public RbyMap Map {
        get { return Maps[CpuRead("wCurMap")]; }
    }

    public RbyTile Tile {
        get { return Map[XCoord, YCoord]; }
    }

    public byte XCoord {
        get { return CpuRead("wXCoord"); }
    }

    public byte YCoord {
        get { return CpuRead("wYCoord"); }
    }

    public byte XBlockCoord {
        get { return CpuRead("wXBlockCoord"); }
    }

    public byte YBlockCoord {
        get { return CpuRead("wYBlockCoord"); }
    }

    public RbyBag Bag {
        get {
            RbyBag bag = new RbyBag();
            bag.Game = this;
            bag.NumItems = CpuRead("wNumBagItems");
            bag.Items = new ItemStack[bag.NumItems];
            RAMStream data = From("wBagItems");
            for(int i = 0; i < bag.Items.Length; i++) {
                bag.Items[i] = new ItemStack(Items[data.u8()], data.u8());
            }
            return bag;
        }
    }

    private RbyPokemon ReadBattleStruct(RAMStream data, RAMStream battleStatus, RAMStream modifier, string unmodifiedStatsLabel) {
        RbyPokemon mon = new RbyPokemon();
        mon.Species = Species[data.u8()];
        mon.HP = data.u16be();
        data.Seek(1); // party pos
        mon.Status = data.u8();
        data.Seek(3); // type and catch rate (for transform, but unimportant to us right now)
        mon.Moves = Array.ConvertAll(data.Read(4), m => Moves[m]);
        mon.DVs = data.u16be();
        mon.Level = data.u8();
        mon.MaxHP = data.u16be();
        mon.Attack = data.u16be();
        mon.Defense = data.u16be();
        mon.Speed = data.u16be();
        mon.Special = data.u16be();
        mon.PP = data.Read(4);
        mon.BattleStatus1 = battleStatus.u8();
        mon.BattleStatus2 = battleStatus.u8();
        mon.BattleStatus3 = battleStatus.u8();
        mon.AttackModifider = modifier.u8();
        mon.DefenseModifider = modifier.u8();
        mon.SpeedModifider = modifier.u8();
        mon.SpecialModifider = modifier.u8();
        mon.AccuracyModifider = modifier.u8();
        mon.EvasionModifider = modifier.u8();
        mon.UnmodifiedMaxHP = CpuReadBE<ushort>(unmodifiedStatsLabel + "MaxHP");
        mon.UnmodifiedAttack = CpuReadBE<ushort>(unmodifiedStatsLabel + "Attack");
        mon.UnmodifiedDefense = CpuReadBE<ushort>(unmodifiedStatsLabel + "Defense");
        mon.UnmodifiedSpeed = CpuReadBE<ushort>(unmodifiedStatsLabel + "Speed");
        mon.UnmodifiedSpecial = CpuReadBE<ushort>(unmodifiedStatsLabel + "Special");
        return mon;
    }

    private RbyPokemon ReadBoxStruct(RAMStream data) {
        RbyPokemon mon = new RbyPokemon();
        mon.Species = Species[data.u8()];
        mon.HP = data.u16be();
        mon.Level = data.u8();
        mon.Status = data.u8();
        data.Seek(3);
        mon.Moves = Array.ConvertAll(data.Read(4), m => Moves[m]);
        data.Seek(2);
        mon.Experience = data.u24be();
        mon.HPExp = data.u16be();
        mon.AttackExp = data.u16be();
        mon.DefenseExp = data.u16be();
        mon.SpeedExp = data.u16be();
        mon.SpecialExp = data.u16be();
        mon.DVs = data.u16be();
        mon.PP = data.Read(4);
        mon.CalculateUnmodifiedStats();
        return mon;
    }

    private RbyPokemon ReadPartyStruct(RAMStream data) {
        RbyPokemon mon = ReadBoxStruct(data);
        mon.Level = data.u8();
        mon.MaxHP = data.u16be();
        mon.Attack = data.u16be();
        mon.Defense = data.u16be();
        mon.Speed = data.u16be();
        mon.Special = data.u16be();
        return mon;
    }
}