using NetMQ;
using Newtonsoft.Json.Linq;
public sealed class statusReadLoop {
    private statusReadLoop() { }

    public static void Init() {
        var readThread = new System.Threading.Thread(x => {
            var runtime = new NetMQRuntime();
            runtime.Run(ReadLoop());
        });
        readThread.Start();
    }

    private static async Task ReadLoop() {
        var mq = new NetMQ.Sockets.SubscriberSocket("tcp://localhost:23456");
        mq.SubscribeToAnyTopic();
        while (true) {
            try {
                var strmsg = await mq.ReceiveFrameStringAsync();
                var responseObject = JObject.Parse(strmsg.Item1);
                var requestId = responseObject.GetValue("request_id")!.Value<Int64>();
                if (commandWriteLoop.RequestDictionary.TryGetValue(requestId, out var request)) {
                    _ = Task.Run(() => request.HandleSignalResponse(responseObject)); //Run async.
                };
            } catch (Exception) {

            };
        }
    }

    // private static async void ReadLoop() {
    //     byte[] buffer = new byte[1024 * 10];

    //     while (true) {
    //         using (var statusPipe = new FileStream("/root/csharp_soem/socket/rt_status", FileMode.Open, FileAccess.Read)) {
    //             //Console.WriteLine("Read Pipe connected");
    //             int inputLength = 0;
    //             do {
    //                 inputLength = await statusPipe.ReadAsync(buffer, 0, buffer.Length);
    //                 if (inputLength == 0) break;
    //                 var inputString = System.Text.Encoding.ASCII.GetString(buffer, 0, inputLength);

    //                 try {
    //                     var responseObject = JObject.Parse(inputString);
    //                     var requestId = responseObject.GetValue("request_id")!.Value<Int64>();
    //                     if (commandWriteLoop.RequestDictionary.TryGetValue(requestId, out var request)) {
    //                         _ = Task.Run(() => request.HandleSignalResponse(responseObject)); //Run async.
    //                     };
    //                 } catch (Exception) {

    //                 };

    //             } while (inputLength > 0);
    //         }
    //     }
    // }
}
