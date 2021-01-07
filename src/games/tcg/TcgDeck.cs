using System.Collections.Generic;

public class TcgDeck : ROMObject {

    public List<TcgCard> Cards;

    public TcgDeck(Tcg game, byte id, ByteStream data) {
        Id = id;
        byte count;
        Cards = new List<TcgCard>();

        do {
            count = data.u8();
            if(count > 0) {
                for(int j = 0; j < count; j++) {
                    Cards.Add(game.Cards[data.Peek()]);
                }
                data.Seek(1);
            }
        } while(count != 0);


        Name = game.GetTextFromId(data.u16le());
    }
}
