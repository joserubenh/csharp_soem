// See https://aka.ms/new-console-template for more information
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

internal class Tools {
    internal static object GetPathSafePascalCase(string name) {
        name = ToPascalCase(name);
        var invalidChars = System.IO.Path.GetInvalidFileNameChars().ToList();
        foreach (char c in invalidChars) {
            name = name.Replace(c.ToString(), "");
        }
        return name;
    }
    public static string ToPascalCase(string original) {
        Regex invalidCharsRgx = new Regex("[^_a-zA-Z0-9]");
        Regex whiteSpace = new Regex(@"(?<=\s)");
        Regex startsWithLowerCaseChar = new Regex("^[a-z]");
        Regex firstCharFollowedByUpperCasesOnly = new Regex("(?<=[A-Z])[A-Z0-9]+$");
        Regex lowerCaseNextToNumber = new Regex("(?<=[0-9])[a-z]");
        Regex upperCaseInside = new Regex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

        // replace white spaces with undescore, then replace all invalid chars with empty string
        var pascalCase = invalidCharsRgx.Replace(whiteSpace.Replace(original, "_"), string.Empty)
            // split by underscores
            .Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
            // set first letter to uppercase
            .Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()))
            // replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
            .Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()))
            // set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
            .Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()))
            // lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
            .Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()));
        return string.Concat(pascalCase);
    }

    internal static void SetBits(ref ushort flag, int bitpos, bool bitval) {
        if (!BitConverter.IsLittleEndian) throw new NotSupportedException("Please implmeent bigEndian in Tools.SetBits");

        Byte[] array = new Byte[16]; //For some weird reason BitConverter fails if array is 4 bytes.
        BitConverter.GetBytes(flag).CopyTo(array,0);
        var uint64 = BitConverter.ToUInt64(array,0);
        
        var mask = 1UL << bitpos;
        if (!bitval) uint64 &= ~mask; else uint64 |= mask;
        var uint64byte = BitConverter.GetBytes(uint64);
        flag = BitConverter.ToUInt16(uint64byte.Take(2).ToArray());        
    }
}