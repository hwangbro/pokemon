public class GoldSilver : Gsc {

    public GoldSilver(string rom, string saveName, bool speedup = false) : base(rom, saveName, speedup ? SpeedupFlags.NoVideo | SpeedupFlags.NoSound : SpeedupFlags.None) { }
}

public class Gold : GoldSilver {

    public Gold(bool speedup = false, string saveName = "roms/pokegold.sav") : base("roms/pokegold.gbc", saveName, speedup) { }
}

public class Silver : GoldSilver {

    public Silver(bool speedup = false, string saveName = "roms/pokesilver.sav") : base("roms/pokesilver.gbc", saveName, speedup) { }
}