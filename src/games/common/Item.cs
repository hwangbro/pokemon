using System.Collections;
using System.Collections.Generic;

public class Item : ROMObject {

    public int ExecutionPointer;
    public string ExecutionPointerLabel;
}

public class ItemStack {

    public Item Item;
    public byte Quantity;
    public ItemStack(Item item, byte quantity = 1) => (Item, Quantity) = (item, quantity);
}
