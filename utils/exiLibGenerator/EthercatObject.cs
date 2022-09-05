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
        var className = this.ObjectType.GetCsharpClassName();
        if (!className.StartsWith("EthercatTypes")) className = "Def." +  className;
        ws.WriteLine($"[ObjectIndex(0x{this.ObjectIndex})]  public readonly {className } x{this.ObjectIndex}_{Tools.GetPathSafePascalCase(this.Name)}  = new();");
    }
}