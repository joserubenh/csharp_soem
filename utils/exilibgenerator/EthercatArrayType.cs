
using System.Collections.Concurrent;
using System.Xml.Linq;
using System.Xml.XPath;

internal class EthercatArrayType : EthercatType {
    private XElement cur;
    private string _typeName;
    private readonly int elements;
    private readonly string baseTypeName;
    private readonly int _bitSize;


    public override string TypeName => _typeName;
    public override int BitSize => _bitSize;
    public EthercatType? baseType { get; private set; } = null;

    public EthercatArrayType(XElement cur) {
        this.cur = cur;
        _typeName = cur.XPathSelectElement("./Name")!.Value;
        elements = int.Parse(cur.XPathSelectElement("./ArrayInfo/Elements")!.Value);
        baseTypeName = cur.XPathSelectElement("./BaseType")!.Value;
        _bitSize = int.Parse(cur.XPathSelectElement("./BitSize")!.Value);
        baseType = null;
    }

    internal void BindIndexType(ConcurrentDictionary<string, EthercatType> dic) {
        baseType = dic[baseTypeName];
    }

    internal override void WriteCSharpDefinition(StreamWriter ws) {
            ws.WriteLine($"public class {GetCsharpClassName()} {{");
            Enumerable.Range(0,elements).ToList().ForEach(x=>{                
                var hexString = (x+1).ToString("X2");
               ws.WriteLine($" public readonly {baseType!.GetCsharpClassName() } x{hexString } = new();");
            });            
            ws.WriteLine("}");
        }

}