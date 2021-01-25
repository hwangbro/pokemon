public class Yellow : Rby {

    public Yellow(bool speedup = false, string saveName = "roms/pokeyellow.sav") : base("roms/pokeyellow.gbc", saveName, speedup ? SpeedupFlags.NoVideo | SpeedupFlags.NoSound : SpeedupFlags.None) {
        SYM["igtInject"] = 0x1C79D6;
    }
    public bool Yoloball() {
        Hold(Joypad.B, SYM["ManualTextScroll"]);
        // for(int i = 0; i < waitFrames; i++) {
        //     RunUntil(SYM["_Joypad"]);
        //     AdvanceFrame();
        // }
        Inject(Joypad.A);
        AdvanceFrame(Joypad.A);
        RunUntil(SYM["PlayCry"], SYM["PlayPikachuSoundClip"]);
        Press(Joypad.Down, Joypad.A, Joypad.A | Joypad.Left);
        return Hold(Joypad.A, SYM["ItemUseBall.captured"], SYM["ItemUseBall.failedToCapture"]) == SYM["ItemUseBall.captured"];
    }
}
