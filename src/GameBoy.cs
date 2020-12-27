using System;
using System.IO;
using System.Runtime.InteropServices;

public enum LoadFlags : int {

    GcbMode = 0x1,          // Treat the ROM as having CGB support regardless of what its header advertises.
    GbaFlag = 0x2,          // Use GBA initial CPU register values in CGB mode.
    MultiCartCompat = 0x4,  // Use heuristics to detect and support multicart MBCs disguised as MBC1.
    SgbMode = 0x8,          // Treat the ROM as having SGB support regardless of what its header advertises.
    ReadOnlySav = 0x10,     // Prevent implicit saveSavedata calls for the ROM.
}

public struct Registers {

    public int PC;
    public int SP;
    public int A;
    public int B;
    public int C;
    public int D;
    public int E;
    public int F;
    public int H;
    public int L;
}

public enum SpeedupFlags : uint {

    None = 0x0,
    NoSound = 0x1,   // Skip generating sound samples.
    NoPPUCall = 0x2, // Skip PPU calls. (breaks LCD interrupt)
    NoVideo = 0x4,   // Skip writing to the video buffer.
    All = 0xffffffff,
}

public enum Joypad : byte {

    None = 0x0,
    A = 0x1,
    B = 0x2,
    Select = 0x4,
    Start = 0x8,
    Right = 0x10,
    Left = 0x20,
    Up = 0x40,
    Down = 0x80,
    All = 0xff,
}

public class GameBoy : IDisposable {

    public const int SamplesPerFrame = 35112;

    public IntPtr Handle;
    public byte[] VideoBuffer;
    public byte[] AudioBuffer;
    public InputGetter InputGetter;
    public Joypad CurrentJoypad;
    public int BufferSamples;
    public int StateSize;

    public ROM ROM;
    public SYM SYM;
    public Scene Scene;
    public ulong EmulatedSamples;

    // Returns the current cycle-based time counter as dividers. (2^21/sec)
    public int TimeNow {
        get { return Libgambatte.gambatte_timenow(Handle); }
    }

    public GameBoy(string biosFile, string romFile, SpeedupFlags speedupFlags = SpeedupFlags.None) {
        ROM = new ROM(romFile);
        Debug.Assert(ROM.HeaderChecksumMatches(), "Cartridge header checksum mismatch!");

        Handle = Libgambatte.gambatte_create();
        Debug.Assert(Libgambatte.gambatte_loadbios(Handle, biosFile, 0x900, 0x31672598) == 0, "Unable to load BIOS!");
        Debug.Assert(Libgambatte.gambatte_load(Handle, romFile, LoadFlags.GbaFlag | LoadFlags.GcbMode | LoadFlags.ReadOnlySav) == 0, "Unable to load ROM!");

        VideoBuffer = new byte[160 * 144 * 4];
        AudioBuffer = new byte[(SamplesPerFrame + 2064) * 2 * 2]; // Stereo 16-bit samples

        InputGetter = () => CurrentJoypad;
        Libgambatte.gambatte_setinputgetter(Handle, InputGetter);

        SetSpeedupFlags(speedupFlags);

        StateSize = Libgambatte.gambatte_savestate(Handle, null, 160, null);

        string symPath = "sym/" + Path.GetFileNameWithoutExtension(romFile) + ".sym";
        if(File.Exists(symPath)) {
            SYM = new SYM(symPath);
            ROM.Symbols = SYM;
        }
    }

    public void Dispose() {
        if(Scene != null) Scene.Dispose();
        Libgambatte.gambatte_destroy(Handle);
    }

    // Emulates 'runsamples' number of samples, or until a video frame has to be drawn. (1 sample = 2 cpu cycles)
    public int RunFor(int runsamples) {
        int videoFrameDoneSampleCount = Libgambatte.gambatte_runfor(Handle, VideoBuffer, 160, AudioBuffer, ref runsamples);
        int outsamples = videoFrameDoneSampleCount >= 0 ? BufferSamples + videoFrameDoneSampleCount : BufferSamples + runsamples;
        BufferSamples += runsamples;
        BufferSamples -= outsamples;
        EmulatedSamples += (ulong) outsamples;

        if(Scene != null) {
            Scene.OnAudioReady(outsamples);
            // returns a positive value if a video frame needs to be drawn.
            if(videoFrameDoneSampleCount >= 0) {
                Scene.Begin();
                Scene.Render();
                Scene.End();
            }
        }

        return Libgambatte.gambatte_gethitinterruptaddress(Handle);
    }

    // Emulates until the next video frame has to be drawn. Returns the hit address.
    public int AdvanceFrame(Joypad joypad = Joypad.None) {
        CurrentJoypad = joypad;
        int hitaddress = RunFor(SamplesPerFrame - BufferSamples);
        CurrentJoypad = Joypad.None;
        return hitaddress;
    }

    public void AdvanceFrames(int frames) {
        for(int i = 0; i < frames; i++) {
            AdvanceFrame();
        }
    }

    // Emulates while holding the specified input until the program counter hits one of the specified breakpoints.
    public unsafe int Hold(Joypad joypad, params int[] addrs) {
        fixed(int* addrPtr = addrs) { // Note: Not fixing the pointer causes an AccessValidationException.
            Libgambatte.gambatte_setinterruptaddresses(Handle, addrPtr, addrs.Length);
            int hitaddress;
            do {
                hitaddress = AdvanceFrame(joypad);
            } while(Array.IndexOf(addrs, hitaddress) == -1);
            Libgambatte.gambatte_setinterruptaddresses(Handle, null, 0);
            return hitaddress;
        }
    }

    // Helper function that emulates with no joypad held.
    public int RunUntil(params int[] addrs) {
        return Hold(Joypad.None, addrs);
    }

    // Writes one byte of data to the CPU bus.
    public void CpuWrite(int addr, byte data) {
        Libgambatte.gambatte_cpuwrite(Handle, (ushort) addr, data);
    }

    public void CpuWriteWord(int addr, ushort data) {
        Libgambatte.gambatte_cpuwrite(Handle, (ushort) addr, (byte) ((data >> 8) & 0xFF));
        Libgambatte.gambatte_cpuwrite(Handle, (ushort) (addr + 1u), (byte) ((data) & 0xFF));
    }

    // Reads one byte of data from the CPU bus.
    public byte CpuRead(int addr) {
        return Libgambatte.gambatte_cpuread(Handle, (ushort) addr);
    }

    public ushort CpuReadWord(int addr) {
        return (ushort) ((CpuRead(addr) << 8) | CpuRead(addr + 1));
    }

    // Returns the emulator state as a buffer.
    public byte[] SaveState() {
        byte[] state = new byte[StateSize];
        Libgambatte.gambatte_savestate(Handle, null, 160, state);
        return state;
    }

    // Helper function that writes the buffer directly to disk.
    public void SaveState(string file) {
        File.WriteAllBytes(file, SaveState());
    }

    // Loads the emulator state given by a buffer.
    public void LoadState(byte[] buffer) {
        Libgambatte.gambatte_loadstate(Handle, buffer, buffer.Length);
    }

    // Helper function that reads the buffer directly from disk.
    public void LoadState(string file) {
        LoadState(File.ReadAllBytes(file));
    }

    public int GetCycleCount() {
        return Libgambatte.gambatte_timenow(Handle);
    }

    public virtual void RandomizeRNG(Random random) {
        throw new NotImplementedException();
    }

    // Sets flags to control non-critical processes for CPU-concerned emulation.
    public void SetSpeedupFlags(SpeedupFlags flags) {
        Libgambatte.gambatte_setspeedupflags(Handle, flags);
    }

    // Injects an input by overwriting the hardware register.
    // Only useful after the GameBoy polled the joypad status but before the inputs are processed.
    public virtual void Inject(Joypad joypad) {
        throw new NotImplementedException();
    }

    // Same concept as above, but generation 2 games read a different hardware register when processing inputs in a menu for some reason.
    public virtual void InjectMenu(Joypad joypad) {
        throw new NotImplementedException();
    }

    // Wrapper for advancing to joypad polling and injecting an input
    public virtual void Press(params Joypad[] joypads) {
        throw new NotImplementedException();
    }

    // Executes the specified actions and returns the last hit breakpoint.
    public virtual int Execute(params Action[] actions) {
        throw new NotImplementedException();
    }

    // Helper function that executes the specified string path.
    public int Execute(string path) {
        return Execute(Array.ConvertAll(path.Split(" "), e => e.ToAction()));
    }

    // Helper functions that translate SYM labels to their respective addresses.
    public int RunUntil(params string[] addrs) {
        return RunUntil(Array.ConvertAll(addrs, e => SYM[e]));
    }

    public int Hold(Joypad joypad, params string[] addrs) {
        return Hold(joypad, Array.ConvertAll(addrs, e => SYM[e]));
    }

    public void CpuWrite(string addr, byte data) {
        CpuWrite(SYM[addr], data);
    }

    public byte CpuRead(string addr) {
        return CpuRead(SYM[addr]);
    }

    // Helper function that creates a basic scene graph with a video buffer component.
    public void Show() {
        Scene s = new Scene(this, 160, 144);
        s.AddComponent(new VideoBufferComponent(0, 0, 160, 144));
    }

    public void Record(string movieName) {
        Show();
        RecordingComponent recorder = new RecordingComponent(movieName);
        Scene.AddComponent(recorder);
        recorder.RecordingNow = EmulatedSamples;
        SetSpeedupFlags(SpeedupFlags.None);
    }

    public void Dispose() {
        if(Scene != null) {
            Scene.Dispose();
        }
    }

    // Helper function that creates a basic scene graph with a video buffer component and a record component.
    public void Record(string movie) {
        Show();
        Scene.AddComponent(new RecordingComponent(movie));
    }

    // Reads the game's font from the ROM. Each game overrides this function and implements it in its own way.
    public virtual Font ReadFont() {
        return null;
    }
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate Joypad InputGetter();

public static unsafe class Libgambatte {

    public const string dll = "libgambatte.dll";

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int gambatte_revision();

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr gambatte_create();

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void gambatte_destroy(IntPtr gb);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int gambatte_load(IntPtr gb, string romfile, LoadFlags flags);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int gambatte_loadbios(IntPtr gb, string biosfile, int size, int crc);

    // Emulates until at least 'samples' audio samples are produced in the supplied audio buffer, or until a video frame has been drawn.
    // There are 35112 audio (stereo) samples in a video frame.
    // May run up to 2064 audio samples too long.
    // The video buffer must have space for at least 160x144 RGB32 (native endian) pixels.
    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int gambatte_runfor(IntPtr gb, byte[] videoBuf, int pitch, byte[] audioBuf, ref int samples);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void gambatte_setrtcdivisoroffset(IntPtr gb, int rtcDivisorOffset);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void gambatte_reset(IntPtr gb, int samplesToStall);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void gambatte_setinputgetter(IntPtr gb, InputGetter inputgetter);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int gambatte_savestate(IntPtr gb, byte[] videoBuf, int pitch, byte[] stateBuf);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool gambatte_loadstate(IntPtr gb, byte[] stateBuf, int size);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte gambatte_cpuread(IntPtr gb, ushort addr);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void gambatte_cpuwrite(IntPtr gb, ushort addr, byte value);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void gambatte_getregs(IntPtr gb, out Registers regs);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void gambatte_setregs(IntPtr gb, Registers regs);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void gambatte_setinterruptaddresses(IntPtr gb, int* addrs, int numAddrs);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int gambatte_gethitinterruptaddress(IntPtr gb);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int gambatte_timenow(IntPtr gb);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int gambatte_getdivstate(IntPtr gb);

    [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void gambatte_setspeedupflags(IntPtr gb, SpeedupFlags falgs);
}