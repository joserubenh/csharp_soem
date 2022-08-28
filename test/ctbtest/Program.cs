// See https://aka.ms/new-console-template for more information

statusReadLoop.Init();
commandWriteLoop.Init();


while (true)
{
    //Console.WriteLine("SENDING REQUEST");
    var result = await EthercatNetwork.SdoRead(1, 0x1A00, 01);
    Console.WriteLine(Convert.ToHexString (result.PayloadResponseBytes!));
    Console.ReadLine();
}



