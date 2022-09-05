using System.Reflection;

namespace EthercatTypes;



public abstract class EthercatFieldBase {
    protected const int debugLengthName = 25;
    protected const int debugValueLength = 25;

    public FieldInfo? FieldInfoReflection { get; private set; }
    public EthercatContext? EcContext { get; private set; }
    public ushort slave { get; private set; }
    public ushort index { get; private set; }
    public UInt16 subIndex { get; private set; }
    public bool CA { get; private set; }

    public void AttachField(FieldInfo reflection,
                            EthercatContext context,
                            UInt16 slave,
                            UInt16 index,
                            UInt16 SubIndex,
                            Boolean CA) {
        this.EcContext = context;
        this.slave = slave;
        this.index = index;
        subIndex = SubIndex;
        this.CA = CA;
        this.FieldInfoReflection = reflection;
    }

    protected virtual async Task<string> DoGetDebugString() {
        return await Task.FromResult("");
    }

    public async Task<string> GetDebugString() {
        try {
            return await DoGetDebugString();
        } catch (Exception ex) {
            return string.Format("{0} H: {1}",
             (FieldInfoReflection?.Name ?? "").PadRight(debugLengthName, '·').Substring(0, debugLengthName),
             "ERR." + ex.Message.PadLeft(debugValueLength).Substring(0, debugValueLength))
             ;
        }
    }

    protected byte[] LastGetBytes {get; private set;} = Array.Empty<byte>();
    public async Task<byte[]> GetBytesAsync() {
        Task<sdo.SdoReadResponse> request;
        if (CA) request = new sdo.SdoReadRequest(slave, index).GetResultAsync();
        else request = new sdo.SdoReadRequest(slave, index, subIndex).GetResultAsync();
        LastGetBytes = (await request).PayloadResponseBytes ?? Array.Empty<byte>();
        return LastGetBytes;
    }

}

public abstract class EthercatFieldBase<T> : EthercatFieldBase {
    public virtual async Task<T> GetValue() {
        return await Task.FromResult(default(T)!);
    }
    
    protected override async Task<string> DoGetDebugString() {
        var resultHexString = string.Join(" ", (await GetBytesAsync()).Select(x => x.ToString("X2").ToUpper()));
        return string.Format("{0} H: {1}",
        (FieldInfoReflection?.Name ?? "").PadRight(debugLengthName, '·').Substring(0, debugLengthName),
        resultHexString.PadLeft(debugValueLength).Substring(0, debugValueLength)
    );
    }
}

public abstract class EthercatNumericBase<T> : EthercatFieldBase<T> {
    protected override async Task<string> DoGetDebugString() {
        var rstBytes = (await GetValue());
        long value = Convert.ToInt64(rstBytes);       

        var resultHexString = string.Join(" ", (LastGetBytes).Reverse().Select(x => x.ToString("X2").ToUpper()));
        return string.Format("{0} H: {1}  D: {2}",
        (FieldInfoReflection?.Name ?? "").PadRight(debugLengthName, '·').Substring(0, debugLengthName),
        resultHexString.PadLeft(debugValueLength).Substring(0, debugValueLength),
        value.ToString("### ### ### ##0").PadLeft(debugValueLength).Substring(0, debugValueLength));
        
        // return string.Format("{0} D: {1}",
        // (FieldInfoReflection?.Name ?? "").PadRight(debugLengthName).Substring(0, debugLengthName),
         
        // );
    }
}

public class DINT : EthercatNumericBase<Int32> {
    public override async Task<Int32> GetValue() {
        var bits = await GetBytesAsync();
        return BitConverter.ToInt32(bits);
    }
}

public class USINT : EthercatNumericBase<byte> {
    public override async Task<byte> GetValue() => (await GetBytesAsync()).First();
}

public class INT : EthercatNumericBase<Int16> {
    public override async Task<Int16> GetValue() => BitConverter.ToInt16(await GetBytesAsync());
}

public class UDINT : EthercatNumericBase<UInt32> {
    public override async Task<UInt32> GetValue() => BitConverter.ToUInt32(await GetBytesAsync());
}
public class BOOLEAN : EthercatNumericBase<Boolean> {
    public override async Task<Boolean> GetValue() => BitConverter.ToBoolean(await GetBytesAsync());
}
public class SINT : EthercatNumericBase<sbyte> {
    public override async Task<sbyte> GetValue() {
        var b = (await GetBytesAsync()).First();
        sbyte sb = unchecked((sbyte)b);
        return sb;
    }
};

public class UINT : EthercatNumericBase<UInt16> {
    public override async Task<UInt16> GetValue() => BitConverter.ToUInt16(await GetBytesAsync());
}


public abstract class EthercatStringBase : EthercatFieldBase<string> {
    public override async Task<string> GetValue() => System.Text.Encoding.ASCII.GetString(await GetBytesAsync());
}

public class STRING0 : EthercatStringBase { }
public class STRING4 : EthercatStringBase { }
public class STRING11 : EthercatStringBase { }
public class STRING16 : EthercatStringBase { }
public class STRING30 : EthercatStringBase { }
public class STRING32 : EthercatStringBase { }

