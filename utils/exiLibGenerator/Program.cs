var exiref  = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),"../.."),"exi_xml");

var exilib  = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),"../.."),"exi_dotnet");
if (System.IO.Directory.Exists(exilib)) System.IO.Directory.Delete(exilib,true);
System.IO.Directory.CreateDirectory(exilib);

var procfiles = new DirectoryInfo(exiref).GetFiles("*.xml").ToList();
procfiles.ForEach(x => ExiReferenceFile.Process(x,exilib));


