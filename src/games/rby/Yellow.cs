public class Yellow : Rby {

    public Yellow(bool speedup = false, string saveName = "roms/pokeyellow.sav") : base("roms/pokeyellow.gbc", saveName, speedup ? SpeedupFlags.NoVideo | SpeedupFlags.NoSound : SpeedupFlags.None) {
        InitIntroStrats();
        SYM["igtInject"] = 0x1C79D6;
    }

    private void InitIntroStrats() {
        IntroStrats["gfSkip"] = new RbyIntroStrat("_gfSkip", 0, new int[] {SYM["Joypad"]}, new Joypad[] {Joypad.Start}, new [] {1});
        // IntroStrats["gfWait"] = new RbyIntroStrat("_gfWait", ??, new int[] {SYM["PlayShootingStar"] + 0x72}, new Joypad[] {Joypad.None}, new [] {1});
        IntroStrats["intro0"] = new RbyIntroStrat("_intr0", 0, new [] {SYM["Joypad"]}, new Joypad[] {Joypad.A}, new int[] {1});
        // IntroStrats["intro1"] = new RbyIntroStrat("_intro1", ??, new [] {SYM["YellowIntroScene2"], SYM["Joypad"]}, new Joypad[] {Joypad.None, Joypad.A}, new int[] {1, 1});
        IntroStrats["title0"] = new RbyIntroStrat("_title0", 0, new [] {SYM["Joypad"]}, new Joypad[] {Joypad.Start}, new int[] {1});
        IntroStrats["cont"] = new RbyIntroStrat("_cont", 0, new [] {SYM["Joypad"]}, new Joypad[] {Joypad.A}, new int[] {1});
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

