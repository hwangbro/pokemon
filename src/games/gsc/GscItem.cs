public class GscItem : ROMObject {

    public Gsc Game;
    public int ExecutionPointer;
    public string ExecutionPointerLabel;

    public GscItem(Gsc game, byte id, ByteStream name) {
        Game = game;
        Name = game.Charmap.Decode(name.Until(Charmap.Terminator));
        Id = id;
        if(id <= 179) {
            ExecutionPointer = 0x3 << 16 | game.ROM.u16le(game.SYM["ItemEffects"] + (byte) id * 2);
            if(game.SYM.Contains(ExecutionPointer)) ExecutionPointerLabel = game.SYM[ExecutionPointer];
        }
    }
}

public class GscItemStack {

    public GscItem Item;
    public byte Quantity;

    public GscItemStack(GscItem item, byte quantity = 1) => (Item, Quantity) = (item, quantity);
}