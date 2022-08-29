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

    await EthercatNetwork.SdoWrite(1,0x6040,BitConverter.GetBytes((UInt16)0xFF));
    await EthercatNetwork.SdoWrite(1,0x6060,BitConverter.GetBytes((byte)2));
    await EthercatNetwork.SdoWrite(1,0x60FF,BitConverter.GetBytes((Int64)2000));

    Console.WriteLine("ST:" +(await EthercatNetwork.SdoRead(1, 0X6041)).ToDebugString());
    Console.WriteLine("AP:" + (await EthercatNetwork.SdoRead(1, 0x6064)).ToDebugString());
    Console.WriteLine();
    subIndex++;
    await Task.Delay(1000);


}



