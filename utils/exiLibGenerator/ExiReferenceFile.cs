// See https://aka.ms/new-console-template for more information
using System.Xml.Linq;
using System.Xml.XPath;

internal class ExiReferenceFile {
    internal static void Process(FileInfo x, string exilibPath) {
        var xDoc = XDocument.Load(new FileStream(x.FullName, FileMode.Open));
        var xGroups = xDoc.XPathSelectElements("EtherCATInfo/Descriptions/Devices/Device");
        xGroups.ToList().ForEach(x => ProcessDevice(x, exilibPath));
    }

    private static void ProcessDevice(XElement xDevice, string exilibPath) {
        var name = xDevice.XPathSelectElement("./Name")!.Value;
        var className = $"{Tools.GetPathSafePascalCase(name)}";
        var ws = new StreamWriter(new FileStream(System.IO.Path.Combine(exilibPath, $"{className}.cs"), FileMode.Create));

        var dicType = new TypeDictionary();
        dicType.Load(xDevice.XPathSelectElements("./Profile/Dictionary/DataTypes/DataType"));
        ws.WriteLine($"public class {className}:Device.EthercatSlave {{");

        ws.WriteLine($"public sealed class Def {{");
        dicType.Values.ToList().ForEach(x => {
            x.WriteCSharpDefinition(ws);
            ws.WriteLine();
        });
        ws.WriteLine("}");

        var objects = xDevice.XPathSelectElements("./Profile/Dictionary/Objects/Object").
        ToList().Select(x => new EthercatObject(x, dicType)).ToList();

        objects.ForEach(x=> x.WriteCsharpObjectDeclaration(ws) );
        


        ws.WriteLine("}");
        ws.Close();
    }
}