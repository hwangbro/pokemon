using System;
using System.Collections.Generic;

public enum TcgCategory : byte {

    Normal,
    Plus,
    Minus,
    X,
    Power,
    Residual = 1 << 7
}

public enum TcgFlag1 : byte {

    None                = 0,
    InflictPoison       = 1 << 0b000,
    InflictSleep        = 1 << 0b001,
    InflictParalysis    = 1 << 0b010,
    InflictConfusion    = 1 << 0b011,
    LowRecoil           = 1 << 0b100,
    HitBench            = 1 << 0b101,
    HighRecoil          = 1 << 0b110,
    DrawCard            = 1 << 0b111
}

public enum TcgFlag2 : byte {

    None                = 0,
    SwitchOppPokemon        = 1 << 0b000,
    HealUser                = 1 << 0b001,
    NullifyOrWeaken         = 1 << 0b010,
    DiscardEnergy           = 1 << 0b011,
    AttachedEnergyBoost     = 1 << 0b100,
    Unused5                 = 1 << 0b101,
    Unused6                 = 1 << 0b110,
    Unused7                 = 1 << 0b111
}

public enum TcgFlag3 : byte {

    None                = 0,
    BoostIfTakenDamage  = 1 << 0b000,
    Unused1             = 1 << 0b001
}

public class TcgMove : ROMObject {

    public Dictionary<TcgType, byte> Cost;
    public string Description;
    public byte Damage;
    public TcgCategory Category;
    public TcgFlag1 Flag1;
    public TcgFlag2 Flag2;
    public TcgFlag3 Flag3;

    public TcgMove(Tcg game, ByteStream data) {
        uint energy = data.u32be();
        Cost = new Dictionary<TcgType, byte>();
        for(byte i = 0; i < 8; i++) {
            uint mask = (uint) 0xF << (i * 4);
            byte count = (byte) ((energy & mask) >> (i *  4));
            if(count > 0) {
                Cost[(TcgType) (15 - i)] = count;
            }
        }
        Name = game.GetTextFromId(data.u16le());
        string d1 = game.GetTextFromId(data.u16le());
        string d2 = game.GetTextFromId(data.u16le());
        d2 = string.IsNullOrEmpty(d2) ? "" : "\n" + d2;
        Description = d1 + d2;
        Damage = data.u8();
        Category = (TcgCategory) data.u8();
        data.Seek(2); // effects
        Flag1 = (TcgFlag1) data.u8();
        Flag2 = (TcgFlag2) data.u8();
        Flag3 = (TcgFlag3) data.u8();
        data.Seek(2); // animation and last byte
    }

    public override string ToString() {
        string cost = "";
        foreach (KeyValuePair<TcgType, byte> pair in Cost) {
            cost += String.Format("{0} {1}, ", pair.Value, pair.Key);
        }
        char[] charsToTrim  = {',', ' '};
        cost = cost.Trim(charsToTrim);
        return String.Format("{0}: {1} Power; {2}", Name, Damage, cost);
    }
}
