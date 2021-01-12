using System;
using System.Collections;
using System.Collections.Generic;

public class GscBag : IEnumerable<GscItemStack> {
    public Gsc Game;
    public GscItemStack[] Items;
    public int NumItems;

    public int IndexOf(string name) {
        return IndexOf(Game.Items[name]);
    }

    public int IndexOf(GscItem item) {
        for(int i = 0; i < NumItems; i++) {
            if(Items[i].Item == item) {
                return i;
            }
        }

        return -1;
    }

    public bool Contains(string name) {
        return Contains(Game.Items[name]);
    }

    public bool Contains(GscItem item) {
        return IndexOf(item) != -1;
    }

    public GscItemStack this[int index] {
        get { return Items[index]; }
        set { Items[index] = value; }
    }

    public GscItemStack this[GscItem item] {
        get { return Items[IndexOf(item)]; }
        set { Items[IndexOf(item)] = value; }
    }

    public GscItemStack this[string name] {
        get { return Items[IndexOf(Game.Items[name])]; }
        set { Items[IndexOf(Game.Items[name])] = value; }
    }

    public IEnumerator<GscItemStack> GetEnumerator() {
        foreach(var item in Items) {
            if(item != null) yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        throw new NotImplementedException();
    }
}
