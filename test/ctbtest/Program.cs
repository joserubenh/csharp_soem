// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

statusReadLoop.Init();
commandWriteLoop.Init();

int cycleCount = 0;
//ushort subIndex = 0;
while (true) {
    //Console.WriteLine("SENDING REQUEST");
    cycleCount++;


    //await EthercatNetwork.SdoWrite(1,0x6040, BitConverter.GetBytes((UInt16)0x00FF).Reverse().ToArray() );      

    // await EthercatNetwork.SdoWrite(1,0x6040,BitConverter.GetBytes((UInt16)0xFF));
    // await EthercatNetwork.SdoWrite(1,0x6060,BitConverter.GetBytes((byte)2));
    // await EthercatNetwork.SdoWrite(1,0x60F,BitConverter.GetBytes((Int64)2000)); //TargetVelocity
    
    Console.WriteLine("SyncType Output:" + (new sdo.SdoReadRequest(1, 0x1C32, 03).GetResultAsync().Result.ToDebugString()));
    Console.WriteLine("SyncType Input :" + (new sdo.SdoReadRequest(1, 0x1C33, 03).GetResultAsync().Result.ToDebugString()));
    Console.WriteLine("ControlWord    :" + (new sdo.SdoReadRequest(1, 0x6040)).GetResultAsync().Result.ToDebugString());
    Console.WriteLine("OperationMode  :" + (new sdo.SdoReadRequest(1, 0x6060)).GetResultAsync().Result.ToDebugString());
    Console.WriteLine("StatusWord     :" + (new sdo.SdoReadRequest(1, 0X6041)).GetResultAsync().Result.ToDebugString());
    Console.WriteLine("ActualPosition :" + (new sdo.SdoReadRequest(1, 0x6064)).GetResultAsync().Result.ToDebugString());
    Console.WriteLine();

    // while (true){
    //      Console.WriteLine("ActualPosition :" + (new sdo.SdoReadRequest(1, 0x6064)).GetResultAsync().Result.ToDebugString());
    //      Console.WriteLine("Following Error:" + (new sdo.SdoReadRequest(1, 0x60F4)).GetResultAsync().Result.ToDebugString());
    //      Console.WriteLine();
    //      await Task.Delay(1000);
    // }

    // var ms = new Stopwatch();
    // ms.Start();
    // for (var a = 0; a < 1000; a++) {
    //         var m = await StatusPing.GetInfoAsync();
    // }
    // ms.Stop();
    // Console.WriteLine($"MessageResponseLoop avg: {ms.ElapsedMilliseconds / 1000} ms");



    return 0;
    // await Task.Delay(1000);


}



