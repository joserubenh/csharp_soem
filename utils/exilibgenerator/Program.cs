var exilib  = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),"../.."),"exi_dotnet");
System.IO.Directory.CreateDirectory(exilib);
new DirectoryInfo(exilib).GetFiles("*.cs").ToList().ForEach(x=> System.IO.File.Delete(x.FullName));

var exiref  = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),"../.."),"exi_xml");

if (System.IO.Directory.Exists(exiref)) System.IO.Directory.Delete(exiref,true);
System.IO.Directory.CreateDirectory(exiref);
new DirectoryInfo(exiref).GetFiles("*.xml").ToList().
ForEach(x => ExiReferenceFile.Process(x,exilib));


