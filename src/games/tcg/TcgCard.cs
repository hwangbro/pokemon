public enum TcgType : byte {

    Fire,
    Grass,
    Lightning,
    Water,
    Fighting,
    Psychic,
    Colorless,
    UNUSED_TYPE,
    Fire_E,
    Grass_E,
    Lightning_E,
    Water_E,
    Fighting_E,
    Psychic_E,
    DoubleColorless_E,
    Unused_E,
    Trainer_E,
    UnusedTrainer_E
}

public enum TcgRarity : byte {

    Circle,
    Diamond,
    Star,
    Promostar = 0xFF
}

public enum TcgSet : byte {

    Colosseum,
    Evolution       = 0x1 << 4,
    Mystery         = 0x2 << 4,
    Laboratory      = 0x3 << 4,
    Promotional     = 0x4 << 4,
    Energy          = 0x5 << 4
}

public enum TcgSet2 : byte {

    Pro             = 0x8,
    GB              = 0x7,
    Fossil          = 0x2,
    Jungle          = 0x1,
    None            = 0
}

public enum TcgStage : byte {

    Basic,
    Stage1,
    Stage2
}

public enum TcgWeakOrResist : byte {

    Fire        = 0x80,
    Grass       = 0x40,
    Lightning   = 0x20,
    Water       = 0x10,
    Fighting    = 0x8,
    Psychic     = 0x4,
    None        = 0
}


public class TcgCard : ROMObject {

    public TcgType Type;
    public TcgRarity Rarity;
    public TcgSet Set1;
    public TcgSet2 Set2;
    public string Description;

    public TcgCard(Tcg game, ByteStream data) {
        Type = (TcgType) data.u8();
        data.Seek(2); // gfxpointer
        Name = game.GetTextFromId(data.u16le());
        Rarity = (TcgRarity) data.u8();
        TcgSet set1 = (TcgSet) data.Nybble();
        TcgSet2 set2 = (TcgSet2) data.Nybble();
        Id = data.u8();

        if (!this.IsPkmn) {
            data.Seek(2); // effectcommands
            string d1 = game.GetTextFromId(data.u16le());
            string d2 = game.GetTextFromId(data.u16le());
            d2 = string.IsNullOrEmpty(d2) ? "" : "\n" + d2;
            Description = d1 + d2;
        }
    }

    public bool IsPkmn {
        get { return Type <= TcgType.Colorless; }
    }
}

public class TcgPkmnCard : TcgCard {

    public byte HP;
    public TcgStage Stage;
    public string PreEvoName;
    public TcgMove[] Moves;
    public byte RetreatCost;
    public TcgWeakOrResist Weakness;
    public TcgWeakOrResist Resistance;
    public string Category;
    public byte PokedexNumber;
    public byte Level;
    public ushort Length;
    public ushort Weight;

    public TcgPkmnCard(Tcg game, ByteStream data) : base(game, data) {
        HP = data.u8();
        Stage = (TcgStage) data.u8();
        PreEvoName = game.GetTextFromId(data.u16le());
        Moves = new TcgMove[2] {
            new TcgMove(game, data),
            new TcgMove(game, data) };
        RetreatCost = data.u8();
        Weakness = (TcgWeakOrResist) data.u8();
        Resistance = (TcgWeakOrResist) data.u8();
        Category = game.GetTextFromId(data.u16le());
        PokedexNumber = data.u8();
        data.Seek(1); // unused
        Level = data.u8();
        Length = data.u16be(); // byte x byte
        Weight = data.u16le();
        Description = game.GetTextFromId(data.u16le());
    }
}
