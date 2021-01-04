using System.Collections.Generic;
using System.Linq;

public class TcgDuelDeck {

    // C27E my deck order
    // C400 my deck indexes

    // C37E opponent deck order
    // C480 opponent deck indexes

    // wDuelInitialPrizes cc08 has number of prizes
    // TcgDuelDeck deck = new TcgDuelDeck();

    public List<TcgCard> Cards;
    public List<TcgCard> Hand;
    public List<TcgCard> Prizes;
    public List<TcgCard> Deck;
    // public List<TcgCard> Discard;
    public TcgPkmnCard Active;
    public List<TcgPkmnCard> Bench;
    public List<TcgCard> BasicsInHand {
        get { return Hand.Where(card => card.IsBasic).ToList<TcgCard>(); }
    }
    public List<TcgPkmnCard> GetActives() {
        List<TcgPkmnCard> actives = new List<TcgPkmnCard>();
        actives.Add(Active);
        actives.AddRange(Bench);
        return actives;
    }
    public void Draw() {
        if (Deck.Count == 0) return;

        TcgCard card = Deck[0];
        Deck.RemoveAt(0);
        Hand.Add(card);
    }
}

public enum TcgDuelStatus {
    None,
    Confused,
    Asleep,
    Paralyzed,
    Poisoned = 0x80,
    DoublePoisoned = 0xc0,
}
