using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

public static class Extensions {

    // Returns a sub-section of an array starting at 'index' and consisting of 'length' elements.
    public static T[] Subarray<T>(this T[] source, int index, int length) {
        T[] subarray = new T[length];
        Array.Copy(source, index, subarray, 0, length);
        return subarray;
    }

    public static T ReadStruct<T>(this byte[] array, int index, bool bigEndian = false) where T : unmanaged {
        if(bigEndian) EndianSwap(typeof(T), array);
        int structSize = Marshal.SizeOf<T>();
        IntPtr ptr = Marshal.AllocHGlobal(structSize);
        Marshal.Copy(array, index, ptr, structSize);
        T str = Marshal.PtrToStructure<T>(ptr);
        Marshal.FreeHGlobal(ptr);
        return str;
    }

    public static byte[] ToBytes<T>(this T str, bool bigEndian = false) where T : unmanaged {
        int size = Marshal.SizeOf(str);
        byte[] array = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, array, 0, size);
        Marshal.FreeHGlobal(ptr);
        if(bigEndian) EndianSwap(typeof(T), array);
        return array;
    }

    private static void EndianSwap(Type type, byte[] data) {
        foreach(FieldInfo field in type.GetFields()) {
            Type fieldType = field.FieldType;
            if(field.IsStatic || fieldType == typeof(string)) continue;
            int offset = Marshal.OffsetOf(type, field.Name).ToInt32();
            object[] attr = field.GetCustomAttributes(typeof(FixedBufferAttribute), false);
            if(attr.Length == 0) Array.Reverse(data, offset, Marshal.SizeOf(fieldType));
        }
    }

    public static ushort RotateLeft(this ushort value, ref byte carry) {
        ushort res = (ushort) (((value << 1) & 0xFFFE) | carry);
        carry = (byte) (value >> 15);
        return res;
    }

    public static (ushort, byte) RotateRight(ref this ushort value, ref byte carry) {
        byte carryO = (byte) (value << 15);
        return ((ushort) (carry | ((value >> 1) & 0x7FFF)), carryO);
    }

    public static void RotateLeft(ref this byte value, ref byte carry) {
        byte carryO = (byte) (value >> 7);
        value = (byte) (((value << 1) & 0xFE) | carry);
        carry = carryO;
    }

    public static (byte, byte) RotateRight(ref this byte value, ref byte carry) {
        byte carryO = (byte) (value << 7);
        return ((byte) (carry | ((value >> 1) & 0x7F)), carryO);
    }
}