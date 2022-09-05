using System.Collections.Concurrent;
using System.Xml.Linq;
using System.Xml.XPath;

internal class EthercatSubIndexType : EthercatType {
    private XElement cur;
    private readonly string _typeName;
    private readonly int _bitSize;
    public override string TypeName => _typeName;
    public override int BitSize => _bitSize;

    List<SubItem> subItems = new();

    public EthercatSubIndexType(XElement cur) {
        this.cur = cur;
        _typeName = cur.XPathSelectElement("./Name")!.Value;
        _bitSize = int.Parse(cur.XPathSelectElement("./BitSize")!.Value);
        subItems = cur.XPathSelectElements("./SubItem").Select(x => new SubItem(x)).ToList();

    }

        internal override void WriteCSharpDefinition(StreamWriter ws) {
            ws.WriteLine($"public class {GetCsharpClassName()}:EthercatTypes.EthercatSubIndexObject {{");

            int ct = 0;
            subItems.ForEach(x=>{                
               ws.WriteLine($"[SubIndex(0x{ct.ToString("X2")})] public readonly {x.SubIndexType!.GetCsharpClassName() } x{ct.ToString("X2")}_{ Tools.GetPathSafePascalCase(x.Name) } = new();");
               ct++;
            });            
            ws.WriteLine("}");
        }
        

    public class SubItem {
        public string? SubIdx { get; }
        public string Name { get; }
        public string SubIndexTypeStr { get; }
        public int BitSize { get; }
        public int BitOffs { get; }
        public string Access { get; }
        public EthercatType? SubIndexType { get; private set; } = null;

        public SubItem(XElement cur) {
            SubIdx = cur.XPathSelectElement("./SubIdx")?.Value;
            Name = cur.XPathSelectElement("./Name")!.Value;
            SubIndexTypeStr = cur.XPathSelectElement("./Type")!.Value;
            BitSize = int.Parse(cur.XPathSelectElement("./BitSize")!.Value);
            BitOffs = int.Parse(cur.XPathSelectElement("./BitOffs")!.Value);
            Access = cur.XPathSelectElement("./Flags/Access")?.Value ?? "ro";
            SubIndexType = null;
        }





        internal void BindSubType(ConcurrentDictionary<string, EthercatType> dic) {
            SubIndexType = dic[SubIndexTypeStr]!;
        }
    }

    internal void BindSubIndexTypes(ConcurrentDictionary<string, EthercatType> dic) {
        subItems.ForEach(x => x.BindSubType(dic));
    }

}