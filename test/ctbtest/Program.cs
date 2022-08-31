// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

statusReadLoop.Init();
commandWriteLoop.Init();

int cycleCount = 0;
ushort subIndex = 0;
while (true) {
    //Console.WriteLine("SENDING REQUEST");
    cycleCount++;
    var ms = new Stopwatch();
    ms.Start();

    //await EthercatNetwork.SdoWrite(1,0x6040, BitConverter.GetBytes((UInt16)0x00FF).Reverse().ToArray() );      

    // await EthercatNetwork.SdoWrite(1,0x6040,BitConverter.GetBytes((UInt16)0xFF));
    // await EthercatNetwork.SdoWrite(1,0x6060,BitConverter.GetBytes((byte)2));
    // await EthercatNetwork.SdoWrite(1,0x60FF,BitConverter.GetBytes((Int64)2000)); //TargetVelocity

Console.WriteLine("BN/06:" +(await EthercatNetwork.SdoRead(1, 0x2004,0x07)).ToDebugString());
Console.WriteLine("BN/21:" +(await EthercatNetwork.SdoRead(1, 0x2004,0x16)).ToDebugString());
Console.WriteLine("HN/33:" +(await EthercatNetwork.SdoRead(1, 0x2009,0x22)).ToDebugString());
Console.WriteLine("BN/33:" +(await EthercatNetwork.SdoRead(1, 0x2003,0x0E)).ToDebugString());


    Console.WriteLine("SyncType Output:" + (await EthercatNetwork.SdoRead(1, 0x1C32,03)).ToDebugString());
    Console.WriteLine("SyncType Input :" + (await EthercatNetwork.SdoRead(1, 0x1C33,03)).ToDebugString());
    Console.WriteLine("ControlWord    :" + (await EthercatNetwork.SdoRead(1, 0x6040)).ToDebugString());
    Console.WriteLine("OperationMode  :" + (await EthercatNetwork.SdoRead(1, 0x6060)).ToDebugString());
    Console.WriteLine("StatusWord     :" + (await EthercatNetwork.SdoRead(1, 0X6041)).ToDebugString());
    Console.WriteLine("ActualPosition :" + (await EthercatNetwork.SdoRead(1, 0x6064)).ToDebugString());
    Console.WriteLine();
    subIndex++;
    return 0;
   // await Task.Delay(1000);


}



