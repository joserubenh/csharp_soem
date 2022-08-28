using Newtonsoft.Json;
public sealed class EthercatNetwork
{
    private EthercatNetwork() { }

    private static StreamWriter commandPipe = new(
    new FileStream("/root/csharp_soem/socket/rt_command", FileMode.Open, FileAccess.Write,FileShare.Write),
        System.Text.ASCIIEncoding.ASCII){AutoFlush = true}
   ;



    public async static void SdoRead(ushort device,UInt16 index){
        var d = new sdo.SdoReadRequest(device,index,0,true);
        d.Post();        
    }

    public async static Task<sdo.SdoResponse> SdoRead(ushort device,UInt16 index, UInt16 subIndex){
        var semSlim = new SemaphoreSlim(0);
        var readRequest = new sdo.SdoReadRequest(device,index,subIndex,false);
        readRequest.ResponseCallback = ()=> semSlim.Release();
        readRequest.Post();        
        await semSlim.WaitAsync();
        return new sdo.SdoResponse(readRequest.PayloadResponseBytes,readRequest);
    }

}
