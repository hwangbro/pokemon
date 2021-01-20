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
    public TcgPkmnCard ActiveCard;
    public TcgBattleCard Active {
        get { return ArenaCards[0]; }
    }

    public List<TcgPkmnCard> Bench;
    public List<TcgCard> BasicsInHand {
        get { return Hand.Where(card => card.IsBasic).ToList<TcgCard>(); }
    }
    public List<TcgBattleCard> ArenaCards;
    public List<TcgBattleCard> SortedArenaCards(TcgBattleCard opp) {
        return ArenaCards.OrderByDescending(item => item.Score(opp)).ToList();
    }

    // public List<TcgPkmnCard> GetActives() {
    //     List<TcgPkmnCard> actives = new List<TcgPkmnCard>();
    //     actives.Add(Active);
    //     actives.AddRange(Bench);
    //     return actives;
    // }
    public void Draw() {
        if(Deck.Count == 0) return;

        TcgCard card = Deck[0];
        Deck.RemoveAt(0);
        Hand.Add(card);
    }

    public List<TcgPkmnCard> SortBasics(TcgPkmnCard oppCard) {
        List<TcgPkmnCard> actives = new List<TcgPkmnCard>();
        List<TcgCard> unsorted = BasicsInHand;
        bool found = false;
        while(!found) {
            for(int i = 0; i < unsorted.Count; i++) {
                TcgPkmnCard card = (TcgPkmnCard) unsorted[i];
                if(card.Type == oppCard.Weakness) {
                    unsorted.Remove(card);
                    actives.Add(card);
                    continue;
                }
            }
            found = true;
        }
        foreach(TcgCard card in unsorted) {
            actives.Add((TcgPkmnCard) card);
        }

        return actives;
    }

    public List<TcgCard> SortHand() {
        // generic priority of cards, mainly for comp search
        // total "junk": lightning energies, basics/evolves that aren't dugtrio/machop
        // potions, switch, full heal, fire/fighting energies
        // dugtrio/oak/bill/pluspower/machop??
        List<TcgCard> cards = new List<TcgCard>();
        List<TcgCard> unsorted = Hand;
        Dictionary<int, int> score = new Dictionary<int, int>();

        List<string> trainers = new List<string> {
            "Potion",
            "Switch",
            "Full Heal",
        };

        for(int i = 0; i < unsorted.Count; i++) {
            TcgCard card = unsorted[i];
            if(card.Type == TcgType.Lightning_E) {
                score[i] = 0;
            } else if(card is TcgPkmnCard && card.Name != "Dugtrio" && card.Name != "Machop") {
                score[i] = 1;
            } else if(trainers.Contains(card.Name)) {
                score[i] = 2;
            } else if(card.IsEnergy){
                score[i] = 3;
            } else {
                score[i] = 4;
            }
        }

        foreach(var pair in score.OrderBy(key => key.Value)) {
            cards.Add(unsorted[pair.Key]);
        }

        return cards;
    }

    public KeyValuePair<byte, byte> GetBestArena(TcgBattleCard opp) {
        byte idx = 0;
        byte score = 0;

        for(byte i = 0; i < ArenaCards.Count; i++) {
            TcgBattleCard card = ArenaCards[i];
            byte curScore = card.Score(opp);
            if(curScore > score) {
                score = curScore;
                idx = i;
            }
        }

        return new KeyValuePair<byte, byte> (idx, score);
    }

    public int GetLowestBenchRetreat() {
        int idx = -1;
        int cost = 100;
        for(int i = 0; i < ArenaCards.Count; i++) {
            if(i == 0) continue;
            TcgBattleCard card = ArenaCards[i];
            if(card.CanRetreat) {
                if(card.Card.RetreatCost < cost) {
                    cost = card.Card.RetreatCost;
                    idx = i;
                }
            }
        }
        return idx;
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
    public byte Flags;
    public byte Substatus1;
    public byte Substatus2;
    public byte Substatus3;
    public bool CanRetreat {
        get { return Energies.Count >= Card.RetreatCost && Status != TcgDuelStatus.Paralyzed; }
    }

    public bool CanAttack {
        get { return CanUseMove1 || CanUseMove2; }
    }

    public bool CanUseMove1 {
        get { return CanUseMove(0).Count == 0; }
    }

    public bool CanUseMove2 {
        get { return CanUseMove(1).Count == 0; }
    }

    public bool CanEvolve {
        get { return (Flags & 0x80) > 0; }
    }

    public bool UsedLeekSlap {
        get { return (Flags & 0x40) > 0; }
    }

    public bool UsedPkmnPower {
        get { return (Flags & 0x20) > 0; }
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
                    } else if(EnergyCopy.Where(item => !item.ToString().Contains(Card.Type.ToString())).Count() > 1) {
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

    // returns how much damage
    public byte CalculateDamage(TcgBattleCard opp, byte moveIndex) {
        byte curDamage = Card.Moves[moveIndex].Damage;
        if(opp.Card.Weakness == Card.Type) {
            curDamage *= 2;
        } else if(opp.Card.Resistance == Card.Type) {
            if(curDamage >= 30) {
                curDamage -= 30;
            } else {
                curDamage = 0;
            }
        }
        curDamage += Pluspower;
        curDamage -= (byte) (opp.Defender * 2);
        return curDamage;
    }

    public byte Score(TcgBattleCard opp) {
        byte score = 0;
        for(byte moveIndex = 0; moveIndex < 2; moveIndex++) {
            if(CanUseMove(moveIndex).Count != 0) {
                continue;
            }
            byte curDamage = CalculateDamage(opp, moveIndex);
            if(curDamage > score) {
                score = curDamage;
            }
        }
        if(opp.Card.Weakness == Card.Type) {
            score += 30;
            score *= 2;
        }
        if(Card.Resistance == opp.Card.Type) {
            score += 30;
        }


        return score;
    }
}
