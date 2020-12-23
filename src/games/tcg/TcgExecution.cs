public static class TcgExecution {

    public static void ScrollYesNoMenu(this Tcg game, Joypad joypad) {
        game.RunUntil("HandleYesOrNoMenu.wait_DPadJoypad");
        game.CpuWrite(0xFF8F, (byte) joypad);
        game.AdvanceFrame();
    }

    public static void SayYes(this Tcg game) {
        game.ScrollYesNoMenu(Joypad.Left);
        game.Press(Joypad.A);
        game.AdvanceFrame();
    }

    public static void SayNo(this Tcg game) {
        game.ScrollYesNoMenu(Joypad.Right);
        game.Press(Joypad.A);
        game.AdvanceFrame();
    }

    public static void RunMovement(this Tcg game, params Joypad[] joypads) {
        foreach(Joypad joypad in joypads) {
            game.RunUntil("HandlePlayerMoveModeInput");
            game.InjectMenu(Joypad.B | joypad);
            game.AdvanceFrame();
        }
    }

    public static void ClearText(this Tcg game, params Joypad[] joypads) {
        int ret = game.RunUntil("WaitForButtonAorB.Joypad", "WaitForWideTextBoxInput.Joypad");
        while(ret == game.SYM["WaitForButtonAorB.Joypad"] || ret == game.SYM["WaitForWideTextBoxInput.Joypad"]) {
            game.Inject(Joypad.A | Joypad.B);
            game.AdvanceFrame();
            ret = game.RunUntil("WaitForButtonAorB.Joypad",
                                   "WaitForWideTextBoxInput.Joypad",
                                   "HandleYesOrNoMenu.wait_Joypad",
                                   "HandlePlayerMoveModeInput.skipMoving",
                                   "StartDuel",
                                   "CardListFunction"); //wGameEvent
        }
    }
}