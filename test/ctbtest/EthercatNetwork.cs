using Newtonsoft.Json;
public sealed class EthercatNetwork {
    private EthercatNetwork() { }

    private static StreamWriter commandPipe = new(
    new FileStream("/root/csharp_soem/socket/rt_command", FileMode.Open, FileAccess.Write, FileShare.Write),
        System.Text.ASCIIEncoding.ASCII) { AutoFlush = true }
   ;



    // public async static void SdoRead(ushort device,UInt16 index){
    //     var d = new sdo.SdoReadRequest(device,index,0,true);
    //     d.Post();        
    // }
    

    public async static Task<sdo.SdoWriteResponse> SdoWrite(ushort device, UInt16 index, byte[] payload) {        
        var semSlim = new SemaphoreSlim(0);
        var readRequest = new sdo.SdoWriteRequest(device, index, 0, true,payload);
        readRequest.ResponseCallback = () => semSlim.Release();
        readRequest.Post();
        await semSlim.WaitAsync();
        return new sdo.SdoWriteResponse(readRequest);     
    }




    public async static Task<sdo.SdoReadResponse> SdoRead(ushort device, UInt16 index) {
        var semSlim = new SemaphoreSlim(0);
        var readRequest = new sdo.SdoReadRequest(device, index, 0, true);
        readRequest.ResponseCallback = () => semSlim.Release();
        readRequest.Post();
        await semSlim.WaitAsync();
        return new sdo.SdoReadResponse(readRequest.PayloadResponseBytes, readRequest);
    }


    public async static Task<sdo.SdoReadResponse> SdoRead(ushort device, UInt16 index, UInt16 subIndex) {
        var semSlim = new SemaphoreSlim(0);
        var readRequest = new sdo.SdoReadRequest(device, index, subIndex, false);
        readRequest.ResponseCallback = () => semSlim.Release();
        readRequest.Post();
        await semSlim.WaitAsync();
        return new sdo.SdoReadResponse(readRequest.PayloadResponseBytes, readRequest);
    }

}
