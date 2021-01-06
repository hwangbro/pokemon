using System;
using System.Collections.Generic;
using System.Linq;

public partial class Tcg {

    // ReadJoypad, at various points, reads data from joypad and stores
    // into hKeysHeld, hKeysPressed, and hKeysReleased
    // these values are not read right away, and are not read at the top of
    // ReadJoypad.

    // SaveButtonsHeld is also a major part of where these inputs are stored,
    // at least for hKeysHeld
    public override void Inject(Joypad joypad) {
        // readjoypad = 00:04de
        // 00:050E
        // = readjoypad + 0x30
        CpuWrite("hKeysPressed", (byte) joypad);
    }

    public void InjectKeysHeld(Joypad joypad) {
        //00:0510
        // = ReadJoypad + 0x32
        // savebuttonsheld = 00:0522
        // stored in hkeysheld = 00:0523
        // 00:5227 = savebuttonsheld + 0x04
        CpuWrite("hKeysHeld", (byte) joypad);
    }

    public void InjectDPadRepeat(Joypad joypad) {
        Inject(joypad);
        InjectKeysHeld(joypad);
        CpuWrite("hDPadHeld", (byte) joypad);
    }

    public override void Press(params Joypad[] joypads) {
        foreach(Joypad joypad in joypads) {
            // 07:536A = input check on intro screen 1 IntroCutsceneJoypad
            // Func_1d078.asm_1d0b8 = input check on title screen TitleScreenJoypad
            // HandleMenuInput.check_A_or_B
            // HandlePlayerModeMoveInput.skipMoving = interacting with ow sprites

            // need to check everywhere that reads from FF91 (hjoypressed)

            int[] addrs = {
                // 0x01D36A, // introcutscenejoypad, 07:536a
                // 0x01D0b8, // titlescreenjoypad 07:50b8
                // SYM["HandleMenuInput.check_A_or_B"], // a/b press on regular menu
                SYM["SaveButtonsHeld"] + 0x05, // overworld movement
                // SYM["HandleYesOrNoMenu"] + 0x1b, //a/b for yes/no
            };

            RunUntil(addrs);
            Inject(joypad);
            AdvanceFrame();
        }
    }

    public void ScrollYesNoMenu(Joypad joypad) {
        RunUntil("HandleDPadRepeat");
        InjectDPadRepeat(joypad);
        AdvanceFrame();
    }

    public void SayYes() {
        AdvanceFrame();
        if(CpuRead("wCurMenuItem") != 0) ScrollYesNoMenu(Joypad.Left);
        Press(Joypad.A);
        AdvanceFrame();
    }

    public void SayNo() {
        AdvanceFrame();
        if(CpuRead("wCurMenuItem") != 1) ScrollYesNoMenu(Joypad.Right);
        Press(Joypad.A);
        AdvanceFrame();
    }

    public void RunMovement(params Joypad[] joypads) {
        foreach(Joypad joypad in joypads) {
            do {
                RunFor(1);
                Hold(Joypad.B, "ReadJoypad"); // requires B to be held every frame to run
            } while(CpuRead(SYM["wPlayerCurrentlyMoving"]) != 0);
            RunUntil(SYM["SaveButtonsHeld"] + 0x05);
            InjectKeysHeld(joypad);
        }
    }

    public void ClearText() {
        // CardListFunction wGameEvent
        // return the exit address?
        // need to exit on duel end screen
        int[] addr = {
            SYM["WaitForButtonAorB"], // clear OW textbox breakpoint
            SYM["HandleYesOrNoMenu"], // yes/no breaks and are handled separately
            SYM["HandlePlayerMoveModeInput"], // OW loop should break
            SYM["CheckSkipDelayAllowed"], // in duel skippable with B
            SYM["WaitForWideTextBoxInput.wait_A_or_B_loop"], //
            SYM["DisplayCardList.wait_button"], // in hand/display
            SYM["HandleDuelMenuInput"], // in main duel menu
            SYM["HandleMenuInput"], // bench selection screen / misc menu screens
            SYM["Func_8aaa"] + 0x4f, // prize check
            SYM["MainDuelLoop.duel_finished"] + 0x27,
        };

        while(true) {
            int ret = RunUntil(addr);
            if (ret == SYM["WaitForButtonAorB"] || ret == SYM["WaitForWideTextBoxInput.wait_A_or_B_loop"]) {
                Press(Joypad.A);
            } else if(ret == SYM["CheckSkipDelayAllowed"]) {
                InjectKeysHeld(Joypad.B);
                AdvanceFrame();
            } else {
                break;
            }
        }
    }

    // run if opp arena card hp is 0?
    public void PickPrize() {
        RunUntil(SYM["Func_8aaa"] + 0x4f);
        Press(Joypad.A);
        RunUntil(SYM["OpenCardPage"] + 0x36);
        Press(Joypad.B);
    }

    public void HandScroll(int slot) {
        RunUntil("HandleMenuInput");
        int curSlot = CpuRead("wCurMenuItem") + CpuRead("wListScrollOffset");

        Joypad direction = slot > curSlot ? Joypad.Down : Joypad.Up;
        int numScrolls = Math.Abs(slot - curSlot);
        for(int i = 0; i < numScrolls; i++) {
            RunUntil("HandleMenuInput");
            RunUntil(SYM["SaveButtonsHeld"] + 0x05);
            InjectDPadRepeat(direction);
            AdvanceFrame();
        }
    }

    public void MenuInput(Joypad joypad) {
        RunUntil("HandleMenuInput");
        Press(joypad);
    }

    public void MenuScroll(int slot) {
        int maxItemIndex = CpuRead("wNumMenuItems") - 1;
        int numScrolls = slot;
        Joypad dir = Joypad.Down;
        if(slot > maxItemIndex / 2) {
            dir = Joypad.Up;
            numScrolls = Math.Max(1, maxItemIndex - slot + 1);
        }
        for(int i = 0; i < numScrolls; i++) {
            RunUntil("HandleMenuInput");
            RunUntil(SYM["SaveButtonsHeld"] + 0x05);
            InjectDPadRepeat(dir);
            AdvanceFrame();
        }
    }

    // scrolls down to specified slot and presses A twice on the item
    public void UseHandCard(int slot, int cardSlot = -1, bool inMenu = false) {
        if(!inMenu) UseDuelMenuOption(TcgDuelMenu.Hand);
        HandScroll(slot);
        MenuInput(Joypad.A);
        RunUntil("HandleMenuInput");
        Press(Joypad.A);
        if(cardSlot != -1) {
            RunUntil("HandleMenuInput");
            MenuScroll(cardSlot);
            Press(Joypad.A);
            ClearText();
        }
    }

    // Presses A on one of the main duel options
    public void UseDuelMenuOption(TcgDuelMenu option) {
        DuelMenuScroll((byte) option);
        DuelMenuInput(Joypad.A);
    }

    public void UseAttack(int slot, bool discard) {
        UseDuelMenuOption(TcgDuelMenu.Attack);
        MenuScroll(slot);
        MenuInput(Joypad.A);
        if(discard) {
            // RunUntil("HandleMenuInput");
            MenuInput(Joypad.A);
        }
    }

    private void DuelMenuInput(Joypad joypad) {
        RunUntil("HandleDuelMenuInput");
        Press(joypad);
    }

    private void DuelMenuScroll(byte slot) {
        int curSlot = CpuRead("wCurrentDuelMenuItem");
        if(curSlot == slot) return;

        if(slot % 2 != curSlot % 2) {
            RunUntil("HandleDuelMenuInput");
            InjectDPadRepeat(Joypad.Up);
            AdvanceFrame();
        }
        int numScrolls = Math.Abs(curSlot - slot) / 2;
        Joypad direction = curSlot > slot ? Joypad.Left : Joypad.Right;

        if(numScrolls > 1) {
            numScrolls = 1;
            direction ^= (Joypad) 0x30;
        }
        for(int i = 0; i < numScrolls; i++) {
            RunUntil("HandleDuelMenuInput");
            InjectDPadRepeat(direction);
            AdvanceFrame();
        }
    }

    public void ClearIntro() {
        RunUntil("Start");
        Press(Joypad.A, Joypad.A);
        RunUntil("Func_1d078.asm_1d0b8"); // title screen joypad
        Press(Joypad.A);
        RunUntil("HandleMenuInput.check_A_or_B");
        Press(Joypad.A);
        RunUntil("HandlePlayerMoveModeInput");
    }

    public bool EquipNeededEnergy(byte slot) {
        TcgDuelDeck myDeck = MyDeck;
        TcgBattleCard card = GetBattleCards()[slot];

        for(byte i = 0; i < 2; i++) {
            List<TcgType> remainingCost = card.CanUseMove(i);
            foreach(TcgType energy in remainingCost) {
                TcgCard energyCard;
                if(energy == TcgType.DoubleColorless_E) {
                    // try using preferred type for ** energies
                    energyCard = myDeck.Hand.FirstOrDefault(item => item.Type.ToString().Contains(card.Card.Type.ToString()));
                    if(energyCard == null) {
                        energyCard = myDeck.Hand.FirstOrDefault(item => energy == TcgType.DoubleColorless_E && item.IsEnergy);
                    }
                } else {
                    energyCard = myDeck.Hand.FirstOrDefault(item => item.Type == energy);
                }

                if(energyCard == null) {
                    continue;
                }

                UseHandCard((byte) myDeck.Hand.IndexOf(energyCard), slot);
                return true;
            }
        }

        return false;
    }

    public bool UseBestMove() {
        byte damage = 0;
        TcgBattleCard active = GetBattleCards()[0];
        TcgBattleCard oppActive = GetBattleCards(true)[0];
        int moveSlot = -1;
        bool discard = false;
        for(byte i = 0; i < 2; i++) {
            List<TcgType> moveCost = active.CanUseMove(i);
            if(moveCost.Count == 0) {
                byte curDamage = active.CalculateDamage(oppActive, i);
                if(curDamage >= oppActive.CurHP) {
                    moveSlot = i;
                    discard = active.Card.Moves[i].Flag2 == TcgFlag2.DiscardEnergy;
                    break;
                }
                if(curDamage > damage) {
                    damage = curDamage;
                    moveSlot = i;
                    discard = active.Card.Moves[i].Flag2 == TcgFlag2.DiscardEnergy;
                }
            }
        }
        if(moveSlot == -1) return false;
        UseAttack(moveSlot, discard);
        return true;
    }

    // main "AI" routine
    // move to execution
    // returns false if duel is finished
    public bool DoTurn() {
        TcgBattleCard active = GetBattleCards()[0];
        if(MyDeck.Hand.Contains(TrainerCards["Bill"])) {
            UseHandCard(MyDeck.Hand.IndexOf(TrainerCards["Bill"]));
            ClearText();
        }
        if(MyDeck.Hand.Contains(TrainerCards["Potion"]) && CpuRead("wPlayerArenaCardHP") != active.CurHP) {
            UseHandCard(MyDeck.Hand.IndexOf(TrainerCards["Potion"]), 0);
            active = GetBattleCards()[0];
            ClearText();
        }

        /*
        TODO
            implement computer search
                needs to have some sort of ranking of hand cards to know which are okay to discard
                basic example: discard random energies, random basics/evo cards?
            implement oak
                needs to know what situation to use oak
            implement pokeballs
            implement switch/retreat strats
            implement pluspower/defender
        */

        // try evolving
        EvolveCards();

        if(active.Status != TcgDuelStatus.None && MyDeck.Hand.Contains(TrainerCards["Full Heal"])) {
            UseHandCard(MyDeck.Hand.IndexOf(TrainerCards["Full Heal"]));
            ClearText();
            active = GetBattleCards()[0];
        }

        // play every basic in our hand
        //    maybe not every?
        PlayBasics(false);

        // equip energy
        // prioritize stronger cards?
        for(byte i = 0; i < CpuRead("wPlayerNumberOfPokemonInPlayArea"); i++) {
            if(CpuRead("wAlreadyPlayedEnergy") == 1) {
                break;
            }
            EquipNeededEnergy(i);
        }

        // always try to attack
        // make attack more intelligent by using weakest attack that can kill
        //    to avoid unnecessary discards?
        // if confused, don't attack if next coin flip is tails
        if(active.Status == TcgDuelStatus.Paralyzed) {
            UseDuelMenuOption(TcgDuelMenu.Done);
        } else if(GetBattleCards(true)[0].Substatus1 == 0x0d) {
            UseDuelMenuOption(TcgDuelMenu.Done);
        } else if(UseBestMove()) {
            ClearText();
            if(CpuRead("wOpponentArenaCardHP") == 0) {
                PickPrize();
            }
        } else {
            UseDuelMenuOption(TcgDuelMenu.Done);
        }
        ClearText();

        if(CpuRead("wPlayerNumberOfPokemonInPlayArea") == 0) {
            return true;
        }

        active = GetBattleCards()[0];

        // todo prefer cards that are stronger to send in rather than always first
        if(active.CurHP == 0) {
            // automatically selects next bench pokemon
            MenuInput(Joypad.A);
            ClearText();
        }

        return CpuRead("wDuelFinished") != 0;
    }

    // todo prefer certain basics over others?
    // limit how many basics to place so i can have "junk" cards in hand?
    // if sending out initial active from duel start, predict opp active and send out best card?
    //    this idea also needs to take into account energies in hand
    public void PlayBasics(bool inMenu) {
        while(MyDeck.BasicsInHand.Count > 0 && CpuRead("wPlayerNumberOfPokemonInPlayArea") < 6) {
            TcgCard basicCard = MyDeck.BasicsInHand[0];
            UseHandCard(MyDeck.Hand.IndexOf(basicCard), -1, inMenu);
            ClearText();
        }
    }

    public void EvolveCards() {
        List<TcgBattleCard> cards = GetBattleCards();
        foreach(TcgCard handCard in MyDeck.Hand) {
            if(handCard is TcgPkmnCard) {
                TcgPkmnCard evoCard = (TcgPkmnCard) handCard;
                TcgBattleCard candidate = cards.Find(item => item.Card.Name == evoCard.PreEvoName);
                if(candidate != null && candidate.CanEvolve) {
                    UseHandCard(MyDeck.Hand.IndexOf(evoCard), cards.IndexOf(candidate));
                }
            }
        }
    }

    // bench pokemon (normally indexed from 1) are indexed from 0 in this menu
    public void Retreat(byte slot) {
        UseDuelMenuOption(TcgDuelMenu.Retreat);
        RunUntil("HandleMenuInput");
        for(int i = 0; i < GetBattleCards()[0].Card.RetreatCost; i++) {
            MenuInput(Joypad.A);
        }
        ClearText();

        MenuScroll(slot - 1);
        Press(Joypad.A);
        ClearText();
    }
}
