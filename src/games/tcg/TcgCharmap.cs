using System.Text;

// separate/duplicate class for now because of how the game handles energy characters
public class TcgCharmap {
    public const byte Terminator = 0x0;

    public Map<byte, string> Map = new Map<byte, string>();

    public TcgCharmap(string characters) {
        string[] arr = characters.Split(" ");
        for(int i = 0; i < arr.Length; i++) {
            Map[(byte) (0x21 + i)] = arr[i];
        }

        Map[(byte) 0x0A] = "\n";
        Map[(byte) 0x20] = " ";

        // energies
        Map[(byte) 0x01] = "\U0001F525"; // fire
        Map[(byte) 0x02] = "\U0001F343"; // grass
        Map[(byte) 0x03] = "\u26A1";     // lightning
        Map[(byte) 0x04] = "\U0001F4A7"; // water
        Map[(byte) 0x05] = "\U0001F44A"; // fighting
        Map[(byte) 0x06] = "\U0001F441"; // psychic
        Map[(byte) 0x07] = "**";     // colorless
    }

    public string Decode(byte b) {
        return Map[b];
    }

    public string Decode(byte[] bytes) {
        bool nextIsEnergy = false;
        StringBuilder sb = new StringBuilder(bytes.Length);
        foreach(byte b in bytes) {
            if(b == Terminator) break;
            else if(b == 0x05 && !nextIsEnergy) continue;
            sb.Append(Map[b]);
        }
        return sb.ToString();
    }
}