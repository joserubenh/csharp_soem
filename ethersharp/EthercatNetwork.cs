using Newtonsoft.Json;
public sealed class EthercatNetwork {
    private EthercatNetwork() { }

    public static EthercatContext DefaultContext = new EthercatContext();

    //     private static StreamWriter commandPipe = new(
    //     new FileStream("/root/csharp_soem/socket/rt_command", FileMode.Open, FileAccess.Write, FileShare.Write),
    //         System.Text.ASCIIEncoding.ASCII) { AutoFlush = true }
    //    ;
    // }


    // public async static Task<sdo.SdoWriteResponse> SdoWrite(ushort device, UInt16 index, byte[] payload) {        

    //     var readRequest = new sdo.SdoWriteRequest(device, index, 0, true,payload);
    //     await readRequest.PostAsync();
    //     return new sdo.SdoWriteResponse(readRequest);     
    // }




    // public async static Task<sdo.SdoReadResponse> SdoRead(ushort device, UInt16 index) {
    //     var readRequest = new sdo.SdoReadRequest(device, index, 0, true);
    //     return 

    // }


    // public async static Task<sdo.SdoReadResponse> SdoRead(ushort device, UInt16 index, UInt16 subIndex) {        
    //     var readRequest = new sdo.SdoReadRequest(device, index, subIndex, false);                
    //     await readRequest.PostAsync();
    //     return new sdo.SdoReadResponse(readRequest.PayloadResponseBytes, readRequest);
    // }

}

public class EthercatContext {

}

[AttributeUsage(System.AttributeTargets.Field)]
public class SubIndexAttribute : System.Attribute {
    public ushort SubIndexAddress { get; }
    public SubIndexAttribute(UInt16 SubIndex) {
        this.SubIndexAddress = SubIndex;
    }
}

[AttributeUsage(System.AttributeTargets.Field)]
public class ArrayOffsetAttribute : System.Attribute {
    public ushort SubIndexOffset { get; }
    public ArrayOffsetAttribute(UInt16 SubIndex) {
        this.SubIndexOffset = SubIndex;
    }
}

[AttributeUsage(System.AttributeTargets.Field)]
public class ObjectIndexAttribute : System.Attribute {
    public ushort Address { get; }
    public ObjectIndexAttribute(UInt16 SubIndex) {
        this.Address = SubIndex;
    }
}