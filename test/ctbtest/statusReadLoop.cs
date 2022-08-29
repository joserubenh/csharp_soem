using Newtonsoft.Json.Linq;
public sealed class statusReadLoop {
    private statusReadLoop() { }

    public static void Init() {
        var readThread = new System.Threading.Thread(x => ReadLoop());
        readThread.Start();
    }

    private static async void ReadLoop() {
        byte[] buffer = new byte[1024 * 10];

        while (true) {
            var statusPipe = new FileStream("/root/csharp_soem/socket/rt_status", FileMode.Open, FileAccess.Read);
            Console.WriteLine("Read Pipe connected");
            int inputLength = 0;
            do {
                inputLength = await statusPipe.ReadAsync(buffer, 0, buffer.Length);                
                if (inputLength == 0) break;              
                var responseObject = JObject.Parse(System.Text.Encoding.ASCII.GetString(buffer,0,inputLength));
                var requestId = responseObject.GetValue("request_id")!.Value<Int64>();
                if (commandWriteLoop.RequestDictionary.TryGetValue(requestId, out var request)) {
                    request.HanldeResponse(responseObject);
                    _ = Task.Run(() => request.ResponseCallback?.Invoke());
                };
            } while (inputLength > 0);
        }
    }
}
