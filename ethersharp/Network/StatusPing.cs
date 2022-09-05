using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public sealed class StatusPing {
    private StatusPing() { }

    public async static Task<StatusInfo> GetInfoAsync() {
        var request = new StatusPingRequest();
        return await request.GetInfo();        
    }

    public sealed class StatusInfo {
        public StatusInfo() {

        }
    }

    private class StatusPingRequest : commandWriteLoop.CommandRequest {
        public override string cmd => "ec_status";
        public override bool ExpectResponse => true;
        public async Task<StatusInfo> GetInfo(){
            var obj = await PostAsync();
            return new StatusInfo();
        }
    }


}