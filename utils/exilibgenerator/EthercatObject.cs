// See https://aka.ms/new-console-template for more information
using System.Xml.Linq;
using System.Xml.XPath;

internal class EthercatObject {
    public string ObjectIndex { get; }
    public string Name { get; }
    public EthercatType ObjectType { get; }

    //public object ObjectTypeStr { get; }

    public EthercatObject(XElement cur, TypeDictionary dicType) {
        ObjectIndex = cur.XPathSelectElement("./Index")!.Value.Replace("x","").Replace("#","");
        Name = cur.XPathSelectElement("./Name")!.Value;
        var ObjectTypeStr = cur.XPathSelectElement("./Type")!.Value;
        ObjectType = dicType[ObjectTypeStr]!;
    }

    internal void WriteCsharpObjectDeclaration(StreamWriter ws) {
        ws.WriteLine($"public readonly Def.{this.ObjectType.GetCsharpClassName() } x{this.ObjectIndex}_{Tools.GetPathSafePascalCase(this.Name)}  = new();");
    }
}