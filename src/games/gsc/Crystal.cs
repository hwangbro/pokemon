public class Crystal : Gsc {

    public Crystal(bool speedup = false, string saveName = "roms/pokecrystal.sav") : base("roms/pokecrystal.gbc", saveName, speedup ? SpeedupFlags.NoVideo | SpeedupFlags.NoSound : SpeedupFlags.None) { }
}