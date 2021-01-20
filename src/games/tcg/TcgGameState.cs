using System;
using System.Collections.Generic;
using System.Linq;

public partial class Tcg {

    public TcgDuelDeck MyDeck {
        get { return ReadDuelDeck(From("wPlayerDeck"), From("wPlayerCardLocations"), CpuRead("wPlayerNumberOfCardsNotInDeck"), CpuRead("wPlayerNumberOfCardsInHand"), CpuRead("wPlayerNumberOfPokemonInPlayArea")); }
    }

    public TcgDuelDeck OppDeck {
        get { return ReadDuelDeck(From("wOpponentDeck"), From("wOpponentCardLocations"), CpuRead("wOpponentNumberOfCardsNotInDeck"), CpuRead("wOpponentNumberOfCardsInHand"), CpuRead("wOpponentNumberOfPokemonInPlayArea")); }
    }

    private TcgDuelDeck ReadDuelDeck(RAMStream cardList, RAMStream data, byte numCardsNotInDeck, byte numCardsInHand, byte numArenaCards) {
        TcgDuelDeck deck = new TcgDuelDeck();
        deck.Cards = new List<TcgCard>();
        deck.Hand = new List<TcgCard>();
        deck.Prizes = new List<TcgCard>();
        deck.Deck = new List<TcgCard>();
        deck.Bench = new List<TcgPkmnCard>();

        List<byte> locations = new List<byte>();

        // Base index 0-59 for cards in deck, stores card ids
        for(int i = 0; i < 60; i++) {
            deck.Cards.Add(Cards[cardList.u8()]);
        }

        // card locations
        for(int i = 0; i < 60; i++) {
            locations.Add(data.u8());
        }

        // prizes
        for(int i = 0; i < 6; i++) {
            if(i < CpuRead("wDuelInitialPrizes")) {
                deck.Prizes.Add(deck.Cards[data.u8()]);
            } else {
                data.Seek(1);
            }
        }

        // hand
        for(int i = 0; i < 60; i++) {
            if(i < numCardsInHand) {
                deck.Hand.Add(deck.Cards[data.u8()]);
            } else {
                data.Seek(1);
            }
        }
        deck.Hand.Reverse();

        // deckcards
        for(int i = 0; i < 60; i++) {
            // prizes are not assigned at the start of the duel, so they should not be in the initial "deck"
            if(i < numCardsNotInDeck + CpuRead("wDuelInitialPrizes")) {
                data.Seek(1);
            } else {
                deck.Deck.Add(deck.Cards[data.u8()]);
            }
        }
        data.Seek(1); // wPlayerNumberOfCardsNotInDeck

        // arenacard + bench
        if(numArenaCards != 0) {
            deck.ActiveCard = (TcgPkmnCard) deck.Cards[data.u8()];

            int benchCount = Math.Max(0, numArenaCards - 1);
            for(int i = 0; i < 5; i++) {
                if(i < benchCount) {
                    deck.Bench.Add((TcgPkmnCard) deck.Cards[data.u8()]);
                } else {
                    data.Seek(1);
                }

            }
        }
        data.Seek(1);

        // parse arena card data
        List<TcgBattleCard> arenaCards = new List<TcgBattleCard>();
        for(int i = 0; i < numArenaCards; i++) {
            arenaCards.Add(new TcgBattleCard());
            arenaCards[i].Energies = new List<TcgType>();
        }

        for(int i = 0; i < 6; i++) {
            if(i < arenaCards.Count) {
                arenaCards[i].Flags = data.u8();
            } else {
                data.Seek(1);
            }
        }

        for(int i = 0; i < 6; i++) {
            if(i < arenaCards.Count) {
                arenaCards[i].CurHP = data.u8();
            } else {
                data.Seek(1);
            }
        }
        data.Seek(6); // cardstage
        data.Seek(6); // changedtype

        for(int i = 0; i < 6; i++) {
            if(i < arenaCards.Count) {
                arenaCards[i].Defender = data.u8();
            } else {
                data.Seek(1);
            }
        }

        for(int i = 0; i < 6; i++) {
            if(i < arenaCards.Count) {
                arenaCards[i].Pluspower = data.u8();
            } else {
                data.Seek(1);
            }
        }
        data.Seek(1);

        if(arenaCards.Count > 0) {
            arenaCards[0].Substatus1 = data.u8();
            arenaCards[0].Substatus2 = data.u8();
            data.Seek(2); // changed weakness, changed resistance
            arenaCards[0].Substatus3 = data.u8();
            data.Seek(4);
            arenaCards[0].Status = (TcgDuelStatus) data.u8();
        }

        for(int i = 0; i < 60; i++) {
            TcgCard card = deck.Cards[i];
            byte location = locations[i];
            if(location >= 0x10 && location <= 0x10 + arenaCards.Count) {
                if(card.IsEnergy) {
                    arenaCards[location - 0x10].Energies.Add(card.Type);
                } else if(card is TcgPkmnCard) {
                    arenaCards[location - 0x10].Card = (TcgPkmnCard) card;
                }
            }
        }

        deck.ArenaCards = arenaCards;
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
        a = (byte) ((a + ahi) ^ wRNG1);
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
