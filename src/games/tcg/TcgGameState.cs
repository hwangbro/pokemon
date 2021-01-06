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

    public List<TcgBattleCard> GetBattleCards(bool enemy = false) {
        string baseName = enemy ? "wOpponent" : "wPlayer";
        TcgDuelDeck deck = enemy ? OppDeck : MyDeck;
        RAMStream cardLocations = From(baseName + "CardLocations");

        List<TcgBattleCard> cards = new List<TcgBattleCard>();

        byte battleCards = CpuRead(baseName + "NumberOfPokemonInPlayArea");
        for(int i = 0; i < battleCards; i++) {
            TcgBattleCard card = new TcgBattleCard();
            string cur = baseName;
            if(i == 0) {
                cur += "ArenaCard";
                card.Status = (TcgDuelStatus) CpuRead(cur + "Status");
                card.Substatus1 = CpuRead(cur + "Substatus1");
                card.Substatus2 = CpuRead(cur + "Substatus2");
                card.Substatus3 = CpuRead(cur + "Substatus3");
            } else {
                cur += "Bench" + battleCards.ToString() + "Card";
            }

            card.Flags = CpuRead(SYM[baseName + "ArenaCardFlags"] + i);

            card.CurHP = CpuRead(cur + "HP");
            card.Pluspower = CpuRead(cur + "AttachedPluspower");
            card.Defender = CpuRead(cur + "AttachedDefender");

            card.Energies = new List<TcgType>();
            cards.Add(card);
        }

        for(int i = 0; i < 60; i++) {
            TcgCard card = deck.Cards[i];
            byte location = cardLocations.u8();
            if(location >= 0x10 && location <= 0x10 + cards.Count) {
                if(card.IsEnergy) {
                    cards[location - 0x10].Energies.Add(card.Type);
                } else if(card is TcgPkmnCard) {
                    cards[location - 0x10].Card = (TcgPkmnCard) card;
                }
            }
        }

        return cards;
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

        if(cardsInBench != 0) {
            deck.Active = (TcgPkmnCard) deck.Cards[data.u8()];

            int benchCount = Math.Max(0, cardsInBench - 1);
            for(int i = 0; i < benchCount; i++) {
                deck.Bench.Add((TcgPkmnCard) deck.Cards[data.u8()]);
            }
        }


        return deck;
    }

    public TcgPkmnCard PredictOppActive() {
        return (TcgPkmnCard) OppDeck.Hand.Where(card => card is TcgPkmnCard && ((TcgPkmnCard) card).Stage == TcgStage.Basic).First();
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