using System;
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

public class TcgBattleCard {
    public byte CurHP;
    public TcgPkmnCard Card;
    public List<TcgType> Energies;
    public byte Pluspower;
    public byte Defender;
    public TcgDuelStatus Status;
    public byte Substatus1;
    public byte Substatus2;
    public byte Substatus3;
    public bool CanRetreat {
        get { return Energies.Count >= Card.RetreatCost && Status != TcgDuelStatus.Paralyzed; }
    }
    public bool CanUseMove1 {
        get { return CanUseMove(0).Count == 0; }
    }

    public bool CanUseMove2 {
        get { return CanUseMove(1).Count == 0; }
    }

    // returns missing energies
    public List<TcgType> CanUseMove(int moveIndex) {
        // sorted dictionary makes colorless last
        List<TcgType> EnergyCopy = new List<TcgType>(Energies);
        SortedDictionary<TcgType, byte> costDict = new SortedDictionary<TcgType, byte>(Card.Moves[moveIndex].Cost);
        List<TcgType> missingEnergy = new List<TcgType>();
        foreach(KeyValuePair<TcgType, byte> cost in costDict) {
            for(byte count = 0; count < cost.Value; count++) {
                if(cost.Key == TcgType.DoubleColorless_E) {
                    if(EnergyCopy.Count == 0) {
                        missingEnergy.Add(TcgType.DoubleColorless_E);
                    } else if (EnergyCopy.Where(item => !item.ToString().Contains(Card.Type.ToString())).Count() > 1) {
                        // if you have energies that are not your type, use those first
                        EnergyCopy.Remove(EnergyCopy.Find(item => !item.ToString().Contains(Card.Type.ToString())));
                    } else {
                        EnergyCopy.Remove(EnergyCopy.First());
                    }
                } else {
                    if(!EnergyCopy.Contains(cost.Key)) {
                        missingEnergy.Add(cost.Key);
                    } else {
                        EnergyCopy.Remove(EnergyCopy.Find(item => item == cost.Key));
                    }
                }
            }
        }
        return missingEnergy;
    }
}