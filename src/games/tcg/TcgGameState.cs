using System;
using System.Collections.Generic;

public partial class Tcg {

    public TcgDuelDeck MyDeck {
        get { return ReadDuelDeck(From("wPlayerDeck"), From("wPlayerDeckCards"), CpuRead("wDuelInitialPrizes")); }
    }

    public TcgDuelDeck OppDeck {
        get { return ReadDuelDeck(From("wOpponentDeck"), From("wOpponentDeckCards"), CpuRead("wDuelInitialPrizes")); }
    }

    private TcgDuelDeck ReadDuelDeck(RAMStream cardList, RAMStream deckOrder, byte numOfPrizes) {
        TcgDuelDeck deck = new TcgDuelDeck();
        deck.Cards = new List<TcgCard>();
        deck.Hand = new List<TcgCard>();
        deck.Prizes = new List<TcgCard>();
        deck.Deck = new List<TcgCard>();

        for(int i = 0; i < 60; i++) {
            deck.Cards.Add(Cards[cardList.u8()]);
        }

        for(int i = 0; i < 60; i++) {
            if(i < 7) {
                deck.Hand.Add(deck.Cards[deckOrder.u8()]);
            } else if(i < numOfPrizes) {
                deck.Prizes.Add(deck.Cards[deckOrder.u8()]);
            } else {
                deck.Deck.Add(deck.Cards[deckOrder.u8()]);
            }
        }

        return deck;
    }
}