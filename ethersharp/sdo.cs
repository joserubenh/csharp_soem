using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Buffers.Binary;

public sealed class sdo {
    private sdo() { }



    public class SdoReadRequest : commandWriteLoop.CommandRequest {
        public override string cmd => "sdo_read";
        [JsonProperty] public UInt16 slave { get; }
        [JsonProperty] public UInt16 index { get; }
        [JsonProperty] public UInt16 subIndex { get; }
        [JsonProperty] public Boolean CA { get; }
        public override bool ExpectResponse => true;
        public byte[]? PayloadResponseBytes;

        public SdoReadRequest(ushort slave, ushort index) {
            this.slave = slave;
            this.index = index;
            this.subIndex = subIndex;
            CA = true;
        }

        public SdoReadRequest(ushort slave, ushort index, ushort subIndex) {
            this.slave = slave;
            this.index = index;
            this.subIndex = subIndex;
            CA = false;
        }

        public async Task<SdoReadResponse> GetResultAsync() {
            var response = await PostAsync();
            var base64Payload = response!.SelectToken("payload")!.Value<string>() ?? "";
            PayloadResponseBytes = Convert.FromBase64String(base64Payload);
            var readResponse = new sdo.SdoReadResponse(PayloadResponseBytes, this);
            Console.WriteLine(readResponse.ToDebugString());
            return readResponse;
        }
    }

    public class SdoReadResponse {
        public SdoReadResponse(byte[]? payloadResponseBytes, SdoReadRequest readRequest) {
            PayloadResponseBytes = payloadResponseBytes;
            ReadRequest = readRequest;
            //Console.WriteLine(ToDebugString());
        }
        public byte[]? PayloadResponseBytes { get; }
        public SdoReadRequest ReadRequest { get; }


        public string ToDebugString() {
            var req = PayloadResponseBytes!;
            //if(BitConverter.IsLittleEndian) req.Reverse();
            var resultHexString = string.Join(" ", req.Select(x => {
                return x.ToString("X2").ToUpper();
            })).PadLeft(20).Substring(0, 20);

            var resultBinaryString = string.Join(" | ", req.Select(x => {
                var init = Convert.ToString(x, 2).PadLeft(8, '0');
                return ($"{init.Substring(0, 4)} {init.Substring(4)}");
            })).PadLeft(55).Substring(0, 55);

            var sbAddrHex = String.Join("", BitConverter.GetBytes(ReadRequest.subIndex).Reverse().
               Select(x => ReadRequest.CA ? "" : x.ToString("X2"))).PadLeft(4, '0');

            var addrHexStr = string.Join("", BitConverter.GetBytes(ReadRequest.index).Select(x => x.ToString("X2"))).PadLeft(4, '0');
            return $" x{addrHexStr} {sbAddrHex} {resultHexString} h {resultBinaryString} b";
        }

    }



    public class SdoWriteRequest : commandWriteLoop.CommandRequest {
        public override string cmd => "sdo_write";
        [JsonProperty] public UInt16 slave { get; }
        [JsonProperty] public UInt16 index { get; }
        [JsonProperty] public UInt16 subIndex { get; }
        [JsonProperty] public Boolean CA { get; }
        [JsonProperty] public string pld64 => Convert.ToBase64String(binaryData);
        public byte[] binaryData { get; }
        public override bool ExpectResponse => true;
        //public byte[]? PayloadResponseBytes;
        
        public SdoWriteRequest(ushort slave, ushort index, byte[] binaryData) {
            this.slave = slave;
            this.index = index;            
            CA = true;
            this.binaryData = binaryData;
        }

        public SdoWriteRequest(ushort slave, ushort index, ushort subIndex, byte[] binaryData) {
            this.slave = slave;
            this.index = index;
            this.subIndex = subIndex;
            CA = false;
            this.binaryData = binaryData;
        }

        public async Task<SdoWriteResponse> WriteToSDO() {
            var response = await PostAsync();
            return new sdo.SdoWriteResponse(this);
        }

    }




    public class SdoWriteResponse {
        public SdoWriteResponse(SdoWriteRequest readRequest) {
            ReadRequest = readRequest;
        }
        public SdoWriteRequest ReadRequest { get; }
    }


}