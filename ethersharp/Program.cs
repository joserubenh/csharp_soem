statusReadLoop.Init();
commandWriteLoop.Init();

BeijingCtbGhnewAcServoDriver sb = new();
sb.BindToDevice(EthercatNetwork.DefaultContext, 1);



//var mpk = new FileStream("./fulldebug.txt", FileMode.Append);
//var wrt = new StreamWriter(mpk);
// var st = new System.Text.StringBuilder();
// sb.WriteFullDebugInfo(wrt);
// wrt.Flush();



