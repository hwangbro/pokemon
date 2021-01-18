public class RbyItem : Item {

    public RbyItem(Rby game, byte id, string name) {
        Name = name;
        Id = id;
        ExecutionPointer = 0x3 << 16 | game.ROM.u16le(game.SYM["ItemUsePtrTable"] + (byte) (id - 1) * 2);
        if(game.SYM.Contains(ExecutionPointer)) ExecutionPointerLabel = game.SYM[ExecutionPointer];
    }
}
