using System;
using System.Collections.Generic;
using System.Linq;

public partial class Tcg {

    public TcgDuelDeck MyDeck {
        get { return ReadDuelDeck(From("wPlayerDeck"), From("wPlayerPrizeCards"), CpuRead("wPlayerNumberOfCardsNotInDeck"), CpuRead("wPlayerNumberOfCardsInHand"), CpuRead("wPlayerNumberOfPokemonInPlayArea")); }
    }

    public TcgDuelDeck OppDeck {
        get { return ReadDuelDeck(From("wOpponentDeck"), From("wOpponentPrizeCards"), CpuRead("wOpponentNumberOfCardsNotInDeck"), CpuRead("wOpponentNumberOfCardsInHand"), CpuRead("wOpponentNumberOfPokemonInPlayArea")); }
    }

    public TcgDuelStatus Status {
        get { return (TcgDuelStatus) CpuRead("wPlayerArenaCardStatus"); }
    }

    // make generic for cards like pp/defender?
    // probably remove
    public Dictionary<TcgType, byte> GetAttachedEnergy(byte slot) {
        Dictionary<TcgType, byte> energies = new Dictionary<TcgType, byte>();

        RAMStream cardLocations = From("wPlayerCardLocations");
        TcgDuelDeck myDeck = MyDeck;
        for(int i = 0; i < 60; i++) {
            TcgCard card = myDeck.Cards[i];
            byte target = (byte) (0x10 + slot);
            if(cardLocations.u8() == target && card.IsEnergy) {
                if(!energies.ContainsKey(card.Type)) {
                    energies[card.Type] = 0;
                }
                energies[card.Type] += 1;
            }
        }
        return energies;
    }

    public TcgBattleCard GetBattleCard(byte slot, bool enemy) {
        string baseName = enemy ? "wEnemy" : "wPlayer";
        TcgDuelDeck deck = enemy ? OppDeck : MyDeck;
        RAMStream cardLocations = From(baseName + "CardLocations");
        if(slot == 0) {
            baseName += "ArenaCard";
        } else {
            baseName += "Bench" + slot.ToString() + "Card";
        }

        TcgBattleCard battleCard = new TcgBattleCard();
        battleCard.CurHP = CpuRead(baseName + "HP");
        battleCard.Pluspower = CpuRead(baseName + "AttachedPluspower");
        battleCard.Defender = CpuRead(baseName + "AttachedDefender");
        if(slot == 0) {
            battleCard.Status = (TcgDuelStatus) CpuRead(baseName + "Status");
            battleCard.Substatus1 = CpuRead(baseName + "Substatus1");
            battleCard.Substatus2 = CpuRead(baseName + "Substatus2");
            battleCard.Substatus3 = CpuRead(baseName + "Substatus3");
        }

        battleCard.Energies = new List<TcgType>();

        // change function to parse all battlecards at once?
        for(int i = 0; i < 60; i++) {
            TcgCard card = deck.Cards[i];
            byte target = (byte) (0x10 + slot);
            if(cardLocations.u8() == target) {
                if(card.IsEnergy) {
                    battleCard.Energies.Add(card.Type);
                } else if(card is TcgPkmnCard) {
                    battleCard.Card = (TcgPkmnCard) card;
                }
            }
        }

        return battleCard;
    }

    private TcgDuelDeck ReadDuelDeck(RAMStream cardList, RAMStream data, byte cardsNotInDeck, byte cardsInHand, byte cardsInBench) {
        TcgDuelDeck deck = new TcgDuelDeck();
        deck.Cards = new List<TcgCard>();
        deck.Hand = new List<TcgCard>();
        deck.Prizes = new List<TcgCard>();
        deck.Deck = new List<TcgCard>();
        deck.Bench = new List<TcgPkmnCard>();

        // Base index 0-59 for cards in deck, stores card ids
        for(int i = 0; i < 60; i++) {
            deck.Cards.Add(Cards[cardList.u8()]);
        }

        for(int i = 0; i < 6; i++) {
            if(i < CpuRead("wDuelInitialPrizes")) {
                deck.Prizes.Add(deck.Cards[data.u8()]);
            } else {
                data.Seek(1);
            }
        }

        for(int i = 0; i < 60; i++) {
            if(i < cardsInHand) {
                deck.Hand.Add(deck.Cards[data.u8()]);
            } else {
                data.Seek(1);
            }
        }
        deck.Hand.Reverse();

        for(int i = 0; i < 60; i++) {
            // prizes are not assigned at the start of the duel, so they should not be in the initial "deck"
            if(i < cardsNotInDeck + CpuRead("wDuelInitialPrizes")) {
                data.Seek(1);
            } else {
                deck.Deck.Add(deck.Cards[data.u8()]);
            }
        }
        data.Seek(1); // wPlayerNumberOfCardsNotInDeck

        deck.Active = (TcgPkmnCard) deck.Cards[data.u8()];

        int benchCount = Math.Max(0, cardsInBench - 1);
        for(int i = 0; i < benchCount; i++) {
            deck.Bench.Add((TcgPkmnCard) deck.Cards[data.u8()]);
        }

        return deck;
    }

    // probably remove
    public bool OneTurnWin() {
        // add more trainer card checking
        bool goingFirst = PredictCoinFlip();
        TcgDuelDeck myDeck = MyDeck;
        TcgDuelDeck oppDeck = OppDeck;

        MyDeck.Draw();
        if(!goingFirst && oppDeck.Deck[0].IsBasic) {
            return false; // fix later
        }
        if(oppDeck.BasicsInHand.Count > 1) return false;

        TcgPkmnCard oppActive = PredictOppActive();
        byte damage = 0;
        foreach(KeyValuePair<TcgCard, TcgMove> pair in PotentialOneTurnMoves()) {
            byte curDamage = pair.Value.Damage;
            if(oppActive.Weakness == pair.Key.Type) {
                curDamage *= 2;
            } else if(oppActive.Resistance == pair.Key.Type) {
                if(curDamage >= 30) {
                    curDamage -= 30;
                } else {
                    curDamage = 0;
                }
            }
            if(myDeck.Hand.Contains(TrainerCards["PlusPower"])) {
                curDamage += 10;
            }
            if(curDamage > damage) damage = curDamage;
        }
        return damage >= oppActive.HP;
    }

    public TcgPkmnCard PredictOppActive() {
        return (TcgPkmnCard) OppDeck.Hand.Where(card => card is TcgPkmnCard && ((TcgPkmnCard) card).Stage == TcgStage.Basic).First();
    }

    // should move to execution probably
    public void EquipNeededEnergy(byte slot) {
        if(CpuRead("wAlreadyPlayedEnergy") == 1) return;
        TcgDuelDeck myDeck = MyDeck;
        TcgPkmnCard card = myDeck.GetActives()[slot];

        for(byte i = 0; i < 2; i++) {
            SortedDictionary<TcgType, byte> remainingCost = CanUseMove(slot, i);
            foreach(KeyValuePair<TcgType, byte> cost in remainingCost.Where(item => item.Value > 0)) {
                TcgCard energyCard = myDeck.Hand.FirstOrDefault(item => item.Type == cost.Key || (cost.Key == TcgType.DoubleColorless_E && item.IsEnergy));
                if(energyCard != null) {
                    UseHandCard((byte) myDeck.Hand.IndexOf(energyCard), slot);
                    return;
                }
            }
        }
    }

    // should move to execution
    public bool UseBestMove() {
        byte damage = 0;
        TcgPkmnCard active = MyDeck.Active;
        int moveSlot = -1;
        bool discard = false;
        for(byte i = 0; i < 2; i++) {
            SortedDictionary<TcgType, byte> moveCost = CanUseMove(0, i);
            if(moveCost.Where(item => item.Value > 0).Count() == 0) {
                if(active.Moves[i].Damage > damage) {
                    damage = active.Moves[i].Damage;
                    moveSlot = i;
                    discard = active.Moves[i].Flag2 == TcgFlag2.DiscardEnergy;
                }
            }
        }
        if(moveSlot == -1) return false;
        UseAttack(moveSlot, discard);
        return true;
    }

    // returns dictionary of remaining energy costs required for a move
    // if item values are all 0, you can use the move
    // now redundant because of tcgbattlecard?
    public SortedDictionary<TcgType, byte> CanUseMove(byte slot, byte moveIndex) {
        Dictionary<TcgType, byte> attached = GetAttachedEnergy(slot);
        TcgPkmnCard card = MyDeck.GetActives()[slot];
        SortedDictionary<TcgType, byte> costDict = new SortedDictionary<TcgType, byte>(card.Moves[moveIndex].Cost);

        int totalCostSum = costDict.Sum(item => item.Value);
        while(costDict.Where(item => item.Value > 0).Count() > 0) {
            for(int costIndex = 0; costIndex < costDict.Count; costIndex++) {
                KeyValuePair<TcgType, byte> cost = costDict.ElementAt(costIndex);
                if(cost.Value == 0) continue;

                if(cost.Key == TcgType.DoubleColorless_E) {
                    // skip parsing doublecolorless until last
                    if(costDict.Where(item => item.Value > 0).Count() > 1) {
                        continue;
                    } else if(attached.Where(item => item.Value > 0).Count() == 0) {
                        // out of energies
                        continue;
                    } else {
                        // remove random energy from attached
                        KeyValuePair<TcgType, byte> randomAttached = attached.First(item => item.Value > 0);
                        attached[randomAttached.Key]--;
                        costDict[cost.Key]--;
                    }
                } else {
                    if(attached.ContainsKey(cost.Key) && attached[cost.Key] > 0) {
                        attached[cost.Key]--;
                        costDict[cost.Key]--;
                    }
                }
            }
            int newCostSum = costDict.Sum(item => item.Value);
            if(newCostSum == totalCostSum) {
                return costDict;
            } else {
                totalCostSum = newCostSum;
            }
        }

        return costDict;
    }

    // only checks first moves
    // probably remove
    public Dictionary<TcgCard, TcgMove> PotentialOneTurnMoves() {
        TcgDuelDeck myDeck = MyDeck;
        Dictionary<TcgCard, TcgMove> ret = new Dictionary<TcgCard, TcgMove>();
        foreach(TcgPkmnCard card in myDeck.BasicsInHand) {
            TcgMove move = card.Moves[0];
            foreach(KeyValuePair<TcgType, byte> energy in move.Cost) {
                int energyCount = myDeck.Hand.Where(card => card.Type == energy.Key || (energy.Key == TcgType.DoubleColorless_E && card.Type.ToString().EndsWith("_E"))).Count();
                if(energyCount >= energy.Value && energy.Value == 1) {
                    ret[card] = move;
                }
            }
        }
        return ret;
    }

    // main "AI" routine
    // move to execution
    public void DoTurn() {
        TcgDuelDeck myDeck = MyDeck;
        // add trainer cards
        if(myDeck.Hand.Contains(TrainerCards["Bill"])) {
            UseHandCard(myDeck.Hand.IndexOf(TrainerCards["Bill"]));
            ClearText();
        }
        if(myDeck.Hand.Contains(TrainerCards["Potion"]) && CpuRead("wPlayerArenaCardHP") != myDeck.Active.HP) {
            UseHandCard(myDeck.Hand.IndexOf(TrainerCards["Potion"]), 0);
            ClearText();
        }
        if(Status != TcgDuelStatus.None && myDeck.Hand.Contains(TrainerCards["Full Heal"])) {
            UseHandCard(myDeck.Hand.IndexOf(TrainerCards["Full Heal"]), 0);
            ClearText();
        }

        // equip energy
        EquipNeededEnergy(0);

        // always try to attack
        // make attack more intelligent by using weakest attack that can kill
        //    to avoid unnecessary discards?
        if(Status == TcgDuelStatus.Paralyzed) {
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
    }

    public bool PredictCoinFlip() {
        byte wRNG1 = CpuRead("wRNG1");
        byte wRNG2 = CpuRead("wRNG2");
        byte wRNGCounter = CpuRead("wRNGCounter");

        byte ahi = (byte) ((wRNG2 >> 6));
        byte a = (byte) (wRNG2 << 2);
        a = (byte) ((a + ahi) ^ wRNG1 );
        byte storeF = (byte) (a & 0b1);
        // byte storeA = (byte) ((a >> 1) & 0xFF);

        byte d = (byte) (wRNG2 ^ wRNG1);
        byte e = (byte) (wRNGCounter ^ wRNG1);

        byte ehi = (byte) ((e >> 7) & 0x1);

        e = (byte) ((e << 1) + storeF); // wrng1
        d = (byte) ((d << 1) + ehi); // wrng2
        a = (byte) (d ^ e);

        // Console.WriteLine("{0:X2}, {1:X2}, {2:X2}", e, d, wRNGCounter+1);

        return a % 2 == 0;
    }
}