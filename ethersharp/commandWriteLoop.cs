using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class commandWriteLoop {
    private commandWriteLoop() { }

    public static void Init() {
        var readThread = new System.Threading.Thread(x => writeLoop());
        readThread.Start();
    }
    private static BufferBlock<CommandRequest> CommandQueue = new();
    public static ConcurrentDictionary<Int64, CommandRequest> RequestDictionary = new();

    private static async void writeLoop() {
        using (var server = new NetMQ.Sockets.PublisherSocket()) {            
            server.Bind("tcp://*:12345");
            while (true) {                                
                var nextCommand = await CommandQueue.ReceiveAsync();                
                server.SendFrame(JsonConvert.SerializeObject(nextCommand!));                
            }
        }
    }



    [JsonObject(MemberSerialization.OptIn)]
    public abstract class CommandRequest {
        [JsonProperty] public abstract string cmd { get; }
        [JsonProperty] public Int64 request_id { get => cmdId; }

        //private SemaphoreSlim lockSemaphore = new(0);
        System.Threading.Tasks.TaskCompletionSource<JObject> completeSignal = new();
        public abstract Boolean ExpectResponse { get; }

        private static Int64 cmdId = 1500;
        private Int64 _cmdId = 0;

        internal void HandleSignalResponse(JObject response) {
            completeSignal.SetResult(response);
        }

        protected async Task<JObject?> PostAsync() {
            _cmdId = Interlocked.Increment(ref cmdId);
            if (ExpectResponse) RequestDictionary.TryAdd(_cmdId, this);
            CommandQueue.Post(this);
            JObject? result = await completeSignal.Task.WaitAsync(TimeSpan.FromSeconds(1));
            return result;
        }

    }

}

