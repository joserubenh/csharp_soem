using Newtonsoft.Json;
namespace DotnetEthercat;

public sealed class EthercatNetwork
{
    private EthercatNetwork() { }

    private static StreamWriter commandPipe = new(
    new FileStream("/root/csharp_soem/socket/rt_command", FileMode.Open, FileAccess.Write,FileShare.Write),
        System.Text.ASCIIEncoding.ASCII)
   ;

    // private static StreamWriter statusPipe = new(
    // new FileStream("/root/csharp_soem/socket/rt_status", FileMode.Open, FileAccess.Write),
    //     System.Text.ASCIIEncoding.ASCII) { AutoFlush = true };

    public static void TryInitializeNetwork()
    {
        commandPipe.Write(Newtonsoft.Json.Linq.JObject.FromObject(
                 new { cmd = "ec_init" }
                 ).ToString());

       commandPipe.Flush();
               


    }

}
