using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public sealed class sdo {
    private sdo() { }
    public class SdoReadRequest : commandWriteLoop.CommandRequest {
        public override string cmd => "sdo_read";
        [JsonProperty] public UInt16 slave { get; }
        [JsonProperty]public UInt16 index { get; }
        [JsonProperty]public UInt16 subIndex { get; }
        [JsonProperty]public Boolean CA { get; }

        public byte[]? PayloadResponseBytes;
      

        public SdoReadRequest(ushort slave, ushort index, ushort subIndex, bool cA) {
            this.slave = slave;
            this.index = index;
            this.subIndex = subIndex;
            CA = cA;
        }

        public override void HanldeResponse(JObject response)
        {
           var base64Payload = response.SelectToken("payload")!.Value<string>()??"";
            PayloadResponseBytes = Convert.FromBase64String(base64Payload);
        }




    }



    public class SdoResponse
    {
        
        public SdoResponse(byte[]? payloadResponseBytes, SdoReadRequest readRequest)
        {
            PayloadResponseBytes = payloadResponseBytes;
            ReadRequest = readRequest;
        }

        public byte[]? PayloadResponseBytes { get; }
        public SdoReadRequest ReadRequest { get; }
    }

}