using System;

public partial class Gsc {

    public GscPokemon BattleMon {
        get { return ReadBattleStruct(From("wBattleMon"), From("wPlayerStatLevels"), From("wPlayerSubStatus1"), SYM["wPlayerScreens"]); }
    }

    public GscPokemon EnemyMon {
        get { return ReadBattleStruct(From("wEnemyMon"), From("wEnemyStatLevels"), From("wEnemySubStatus1"), SYM["wEnemyScreens"]); }
    }

    public GscPokemon PartyMon1 {
        get { return ReadPartyStruct(From("wPartyMon1")); }
    }

    public GscPokemon PartyMon2 {
        get { return ReadPartyStruct(From("wPartyMon2")); }
    }

    public GscPokemon PartyMon3 {
        get { return ReadPartyStruct(From("wPartyMon3")); }
    }

    public GscPokemon PartyMon4 {
        get { return ReadPartyStruct(From("wPartyMon4")); }
    }

    public GscPokemon PartyMon5 {
        get { return ReadPartyStruct(From("wPartyMon5")); }
    }

    public GscPokemon PartyMon6 {
        get { return ReadPartyStruct(From("wPartyMon6")); }
    }

    public GscPokemon PartyMon(int index) {
        return ReadPartyStruct(From(SYM["wPartyMons"] + index * (SYM["wPartyMon2"] - SYM["wPartyMon1"])));
    }

    public GscPokemon BoxMon(int index) {
        return ReadPartyStruct(From(SYM["wBoxMons"] + index * (SYM["wBoxMon2"] - SYM["wBoxMon1"])));
    }

    public GscMap Map {
        get { return Maps[CpuReadBE<ushort>("wMapGroup")]; }
    }

    public GscTile Tile {
        get { return Map[XCoord, YCoord]; }
    }

    public byte XCoord {
        get { return CpuRead("wXCoord"); }
    }

    public byte YCoord {
        get { return CpuRead("wYCoord"); }
    }

    public GscBag Bag {
        get {
            GscBag bag = new GscBag();
            bag.Items = new GscPocket();
            bag.Balls = new GscPocket();
            bag.KeyItems = new GscPocket();
            bag.TmsHms = new GscPocket();

            bag.Items.Game = bag.Balls.Game = bag.KeyItems.Game = bag.TmsHms.Game = this;

            RAMStream data = From("wTMsHMs");
            int cursorIndex = 0;
            bag.TmsHms.Items = new GscItemStack[57];
            for(byte i = 0; i < bag.TmsHms.Items.Length; i++) {
                byte count = data.u8();
                byte id = i;
                if(id >= 4) id++;
                if(id >= 29) id++;

                if(count > 0) {
                    bag.TmsHms.Items[cursorIndex++] = new GscItemStack(Items[id + 190], count);
                }
            }
            bag.TmsHms.NumItems = cursorIndex;

            bag.Items.NumItems = data.u8();
            bag.Items.Items = new GscItemStack[bag.Items.NumItems];
            for(byte i = 0; i < bag.Items.NumItems; i++) {
                bag.Items.Items[i] = new GscItemStack(Items[data.u8() - 1], data.u8());
            }
            data.Seek((20 - bag.Items.NumItems) * 2 + 1);

            bag.KeyItems.NumItems = data.u8();
            bag.KeyItems.Items = new GscItemStack[bag.KeyItems.NumItems];
            for(byte i = 0; i < bag.KeyItems.NumItems; i++) {
                bag.KeyItems.Items[i] = new GscItemStack(Items[data.u8() - 1], 1);
            }
            data.Seek((25 - bag.KeyItems.NumItems) + 1);

            bag.Balls.NumItems = data.u8();
            bag.Balls.Items = new GscItemStack[bag.Balls.NumItems];
            for(byte i = 0; i < bag.Balls.NumItems; i++) {
                bag.Balls.Items[i] = new GscItemStack(Items[data.u8() - 1], data.u8());
            }

            return bag;
        }
    }

    // TODO: Box structs, Party structs

    private GscPokemon ReadBattleStruct(RAMStream data, RAMStream modifiers, RAMStream battleStatus, int screensAddr) {
        GscPokemon mon = new GscPokemon();
        mon.Species = Species[data.u8()];
        mon.HeldItem = Items[data.u8()];
        mon.Moves = Array.ConvertAll(data.Read(4), m => Moves[m]);
        mon.DVs = data.u16be();
        mon.PP = data.Read(4);
        mon.Happiness = data.u8();
        mon.Level = data.u8();
        mon.Status = data.u8();
        data.Seek(1); // unused
        mon.HP = data.u16be();
        mon.MaxHP = data.u16be();
        mon.Attack = data.u16be();
        mon.Defense = data.u16be();
        mon.Speed = data.u16be();
        mon.SpecialAttack = data.u16be();
        mon.SpecialDefense = data.u16be();
        mon.AttackModifider = modifiers.u8();
        mon.DefenseModifider = modifiers.u8();
        mon.SpeedModifider = modifiers.u8();
        mon.SpecialAttackModifider = modifiers.u8();
        mon.SpecialDefenseModifider = modifiers.u8();
        mon.AccuracyModifider = modifiers.u8();
        mon.EvasionModifider = modifiers.u8();
        mon.BattleStatus1 = battleStatus.u8();
        mon.BattleStatus2 = battleStatus.u8();
        mon.BattleStatus3 = battleStatus.u8();
        mon.BattleStatus4 = battleStatus.u8();
        mon.BattleStatus5 = battleStatus.u8();
        mon.Screens = CpuRead(screensAddr);
        mon.CalculateUnmodifiedStats();
        return mon;
    }

    private GscPokemon ReadBoxStruct(RAMStream data) {
        GscPokemon mon = new GscPokemon();
        mon.Species = Species[data.u8()];
        mon.HeldItem = Items[data.u8()];
        mon.Moves = Array.ConvertAll(data.Read(4), m => Moves[m]);
        data.Seek(2); // ID
        mon.Experience = data.u24be();
        mon.HPExp = data.u16be();
        mon.AttackExp = data.u16be();
        mon.DefenseExp = data.u16be();
        mon.SpeedExp = data.u16be();
        mon.SpecialExp = data.u16be();
        mon.DVs = data.u16be();
        mon.PP = data.Read(4);
        mon.Happiness = data.u8();
        mon.Pokerus = data.u8() > 0;
        data.Seek(2); // unused
        mon.Level = data.u8();
        mon.CalculateUnmodifiedStats();
        return mon;
    }

    private GscPokemon ReadPartyStruct(RAMStream data) {
        GscPokemon mon = ReadBoxStruct(data);
        mon.Status = data.u8();
        data.Seek(1); // unused
        mon.HP = data.u16be();
        mon.MaxHP = data.u16be();
        mon.Attack = data.u16be();
        mon.Defense = data.u16be();
        mon.Speed = data.u16be();
        mon.SpecialAttack = data.u16be();
        mon.SpecialDefense = data.u16be();
        return mon;
    }
}