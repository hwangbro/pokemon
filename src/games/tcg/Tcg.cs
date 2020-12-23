using System;
using System.Collections.Generic;
using System.Linq;

public class TcgData {
    public TcgCharmap Charmap;
    public DataList<TcgCard> Cards;
    public DataList<TcgPkmnCard> PkmnCards;
    public DataList<TcgCard> TrainerCards;
    public DataList<TcgDeck> Decks;

    public TcgData() {
        Charmap = new TcgCharmap("! \" _ ♂ ♀ & ' ( ) _ _ , - . _ " +
                            "0 1 2 3 4 5 6 7 8 9 : ; < = > ? " +
                            "@ A B C D E F G H I J K L M N O " +
                            "P Q R S T U V W X Y Z [ \\ ] ^ _ " +
                            "é a b c d e f g h i j k l m n o " +
                            "p q r s t u v w x y z { | } ");

        Cards = new DataList<TcgCard>();
        PkmnCards = new DataList<TcgPkmnCard>();
        TrainerCards = new DataList<TcgCard>();
        Decks = new DataList<TcgDeck>();

        Cards.IndexCallback = obj => obj.Id;

        PkmnCards.IndexCallback = obj => obj.Id;

        TrainerCards.NameCallback = obj => obj.Name;
        TrainerCards.IndexCallback = obj => obj.Id;

        Decks.NameCallback = obj => obj.Name;
        Decks.IndexCallback = obj => obj.Id;
    }
}

public class Tcg : GameBoy {
    private static Dictionary<int, TcgData> ParsedROMs = new Dictionary<int, TcgData>();
    public TcgData Data;

    public TcgCharmap Charmap {
        get { return Data.Charmap; }
    }

    public DataList<TcgCard> Cards {
        get { return Data.Cards; }
    }

    public DataList<TcgPkmnCard> PkmnCards {
        get { return Data.PkmnCards; }
    }

    public DataList<TcgCard> TrainerCards {
        get { return Data.TrainerCards; }
    }

    public DataList<TcgDeck> Decks {
        get { return Data.Decks; }
    }

    public Tcg(bool speedup = false, string saveName = "roms/poketcg.sav", string rom = "roms/poketcg.gbc")
        : this(rom, saveName, speedup ? SpeedupFlags.NoSound | SpeedupFlags.NoVideo : SpeedupFlags.None) { }
    public Tcg(string rom, string saveName, SpeedupFlags speedupFlags) : base("roms/gbc_bios.bin", rom, saveName, speedupFlags) {
        if(ParsedROMs.ContainsKey(ROM.GlobalChecksum)) {
            Data = ParsedROMs[ROM.GlobalChecksum];
        } else {
            Data = new TcgData();
            LoadCards();
            LoadDecks();
        }
    }

    public override void Inject(Joypad joypad) {
        CpuWrite(0xFF91, (byte) joypad);
    }

    public override void InjectMenu(Joypad joypad) {
        CpuWrite(0xFF90, (byte) joypad);
    }

    public void Press(params Joypad[] joypads) {
        foreach(Joypad joypad in joypads) {
            // 07:536A = input check on intro screen 1 IntroCutsceneJoypad
            // Func_1d078.asm_1d0b8 = input check on title screen TitleScreenJoypad
            // HandleMenuInput.check_A_or_B
            // HandlePlayerModeMoveInput.skipMoving = interacting with ow sprites

            // Step();

            // need to check everywhere that reads from FF91 (hjoypressed)
            RunUntil("IntroCutsceneJoypad",
                            "TitleScreenJoypad",
                            "HandleMenuInput.check_A_or_B", // a/b press on regular menu
                            "HandlePlayerMoveModeInput.skipMoving", // overworld movement
                            "HandleYesOrNoMenu.wait_Joypad" // a/b for yes no
                            );
            Inject(joypad);
            AdvanceFrame();
        }
    }

    private void LoadCards() {
        const int numCards = 228;
        ByteStream pointerStream = ROM.From("CardPointers");
        pointerStream.Seek(2); // unused pointer

        for(int i = 0; i < numCards; i++) {
            ByteStream cardStream = ROM.From(0xC << 16 | pointerStream.u16le());
            TcgType type = (TcgType) cardStream.Peek();
            if (type <= TcgType.Colorless) {
                TcgPkmnCard card = new TcgPkmnCard(this, cardStream);
                PkmnCards.Add(card);
                Cards.Add(card);
            } else {
                TcgCard card = new TcgCard(this, cardStream);
                TrainerCards.Add(card);
                Cards.Add(card);
            }
        }
    }

    private void LoadDecks() {
        const int numDecks = 53;
        ByteStream pointerStream = ROM.From("DeckPointers");
        pointerStream.Seek(4); // first two decks are ignored

        for(byte i = 0; i < numDecks; i++) {
            ByteStream cardStream = ROM.From(0xC << 16 | pointerStream.u16le());
            Decks.Add(new TcgDeck(this, i, cardStream));
        }
    }

    public TcgDuelDeck CreateDuelDeck(bool opponent = false) {
        int addr = opponent ? SYM["wOpponentDeck"] : SYM["wPlayerDeck"];
        List<TcgCard> cards = new List<TcgCard>();
        for(int i = 0; i < 60; i++) {
            cards.Add(Cards[CpuRead(addr + i)]);
        }

        List<TcgCard> hand = new List<TcgCard>();
        List<TcgCard> prizes = new List<TcgCard>();
        List<TcgCard> deck = new List<TcgCard>();

        addr = opponent ? 0xC37E : 0xC27E;
        for(int i = 0; i < 7; i++) {
            hand.Add(cards[CpuRead(addr + i)]);
        }

        // cc08 is number of prizes
        int numOfPrizes = CpuRead(0xCC08);
        for(int i = 7; i < numOfPrizes; i++) {
            prizes.Add(cards[CpuRead(addr + i)]);
        }

        for(int i = 7 + numOfPrizes; i < 60; i++) {
            deck.Add(cards[CpuRead(addr + i)]);
        }

        return new TcgDuelDeck() {
            Cards = cards,
            Hand = hand,
            Prizes = prizes,
            Deck = deck
        };
    }

    // ugly 1 to 1 asm code until I refine this
    public string GetTextFromId(ushort id) {
        ushort hl = id;
        ushort de = hl;
        hl = (ushort) (hl * 3);
        hl = (ushort) (hl + 0x4000);

        ushort hl2;
        byte c = 0;

        int textPointer = 0xD << 16 | hl;
        ByteStream textStream = ROM.From(textPointer);

        de = textStream.u16le();
        byte a = textStream.u8();
        hl += 2;

        // ld hl, d
        hl = (ushort) ((hl & 0x00FF) | (de & 0xFF00));

        // rl h
        hl2 = ((ushort)(hl & 0xFF00)).RotateLeft(ref c);
        hl = (ushort) (hl2 | (hl & 0x00FF));

        // rla
        a.RotateLeft(ref c);


        // rl h
        hl2 = ((ushort)(hl & 0xFF00)).RotateLeft(ref c);
        hl = (ushort) (hl2 | (hl & 0x00FF));

        // rla
        a.RotateLeft(ref c);


        byte bank = (byte) (a + 0xD);
        de = (ushort) (de & 0x7FFF); // res 7, d
        de = (ushort) (de | 0x4000); // set 6, d

        hl = de;
        int namePointer = (bank << 16 |  hl) + 1;

        string name = Charmap.Decode(ROM.From(namePointer).Until(TcgCharmap.Terminator));
        // Charmap.Decode(Rom.ReadUntil(TcgCharmap.Terminator, ref namePointer));

        return name;
    }
}