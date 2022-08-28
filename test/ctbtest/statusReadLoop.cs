using Newtonsoft.Json.Linq;
public sealed class statusReadLoop
{
    private statusReadLoop() { }

    public static void Init()
    {
        var readThread = new System.Threading.Thread(x => ReadLoop());
        readThread.Start();
    }

    private static async void ReadLoop()
    {
        while (true)
        {
            using (StreamReader statusPipe = new(
                        new FileStream("/root/csharp_soem/socket/rt_status", FileMode.Open, FileAccess.Read),
                                System.Text.ASCIIEncoding.ASCII))
            {
                int inputLength = 0;
                do
                {
                    var input = await statusPipe.ReadToEndAsync();
                    inputLength = input.Length;
                    if (input.Length == 0) break;
                    var responseObject = JObject.Parse(input);
                    var requestId = responseObject.GetValue("request_id")!.Value<Int64>();
                    if (commandWriteLoop.RequestDictionary.TryGetValue(requestId,out var request)) {
                        request.HanldeResponse(responseObject);
                        _ = Task.Run(()=> request.ResponseCallback?.Invoke() );
                    };                    
                } while (inputLength > 0);
            }
        }
    }
}