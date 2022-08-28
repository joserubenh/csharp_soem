// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

statusReadLoop.Init();
commandWriteLoop.Init();

int cycleCount = 0;
while (true)
{
    //Console.WriteLine("SENDING REQUEST");
    cycleCount++;
    var ms = new Stopwatch();
    ms.Start();
    var result = await EthercatNetwork.SdoRead(1, 0x1A00, 01);
    ms.Stop();
    Console.WriteLine($"{cycleCount.ToString().PadLeft(5)} (Hex): {Convert.ToHexString (result.PayloadResponseBytes!).PadLeft(10) } in {ms.Elapsed.TotalMilliseconds.ToString("# ##0").PadLeft(6)} ms ");
    await Task.Delay(1000);


}



