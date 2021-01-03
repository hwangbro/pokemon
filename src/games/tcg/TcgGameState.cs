using System;
using System.Collections.Generic;
using System.Linq;

public partial class Tcg {

    public TcgDuelDeck MyDeck {
        get { return ReadDuelDeck(From("wPlayerDeck"), From("wPlayerPrizeCards"), CpuRead("wPlayerNumberOfCardsNotInDeck"), CpuRead("wPlayerNumberOfCardsInHand")); }
    }

    public TcgDuelDeck OppDeck {
        get { return ReadDuelDeck(From("wOpponentDeck"), From("wOpponentPrizeCards"), CpuRead("wOpponentNumberOfCardsNotInDeck"), CpuRead("wOpponentNumberOfCardsInHand")); }
    }

    public Dictionary<TcgType, byte> EnergiesOnActive() {
        Dictionary<TcgType, byte> energies = new Dictionary<TcgType, byte>();

        RAMStream cardLocations = From("wPlayerCardLocations");
        TcgDuelDeck myDeck = MyDeck;
        for(int i = 0; i < 60; i++) {
            TcgCard card = myDeck.Cards[i];
            if(cardLocations.u8() == 0x10 && card.IsEnergy) {
                if(!energies.ContainsKey(card.Type)) {
                    energies[card.Type] = 0;
                }
                energies[card.Type] += 1;
            }
        }
        return energies;
    }

    private TcgDuelDeck ReadDuelDeck(RAMStream cardList, RAMStream data, byte cardsNotInDeck, byte cardsInHand) {
        TcgDuelDeck deck = new TcgDuelDeck();
        deck.Cards = new List<TcgCard>();
        deck.Hand = new List<TcgCard>();
        deck.Prizes = new List<TcgCard>();
        deck.Deck = new List<TcgCard>();

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
            if(i < cardsNotInDeck + CpuRead("wDuelInitialPrizes")) {
                data.Seek(1);
            } else {
                deck.Deck.Add(deck.Cards[data.u8()]);
            }
        }
        return deck;
    }

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

    // only checks first moves
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