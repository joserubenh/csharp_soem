using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class commandWriteLoop
{
    private commandWriteLoop() { }

    public static void Init()
    {
        var readThread = new System.Threading.Thread(x => writeLoop());
        readThread.Start();
    }
    private static BufferBlock<CommandRequest> CommandQueue = new();
    public static ConcurrentDictionary<Int64,CommandRequest> RequestDictionary = new();

    private static async void writeLoop()
    {
        StreamWriter statusPipe =
            new(new FileStream("/root/csharp_soem/socket/rt_command",
                FileMode.Open, FileAccess.Write, FileShare.Read),
            System.Text.ASCIIEncoding.ASCII)
            { };

        while (true)
        {
            var nextCommand = await CommandQueue.ReceiveAsync();           
            statusPipe.Write(JsonConvert.SerializeObject(nextCommand!));
            statusPipe.Flush();
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public abstract class CommandRequest
    {
        [JsonProperty] public abstract string cmd { get; }
        [JsonProperty] public Int64 request_id { get => cmdId; }
        private static Int64 cmdId = 1500;
        private Int64 _cmdId = 0;
        public Action? ResponseCallback; 
        public void Post()
        {
            _cmdId = Interlocked.Increment(ref cmdId);
            RequestDictionary.TryAdd(_cmdId,this);
            CommandQueue.Post(this);            
        }

        public abstract void HanldeResponse(JObject response );
    }

}

