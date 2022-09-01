using Newtonsoft.Json;
public sealed class EthercatNetwork {
    private EthercatNetwork() { }

    private static StreamWriter commandPipe = new(
    new FileStream("/root/csharp_soem/socket/rt_command", FileMode.Open, FileAccess.Write, FileShare.Write),
        System.Text.ASCIIEncoding.ASCII) { AutoFlush = true }
   ;
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
