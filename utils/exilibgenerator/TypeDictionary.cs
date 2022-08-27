// See https://aka.ms/new-console-template for more information
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using System.Xml.XPath;

internal class TypeDictionary : IReadOnlyDictionary<string, EthercatType> {
    public TypeDictionary() {

        typeof(NativeTypes).GetNestedTypes().Where(x => x.IsSubclassOf(typeof(EthercatType))).
        Where(x => x.IsAbstract == false).ToList().
        ForEach(x => {
            var instance = (EthercatType)Activator.CreateInstance(x)!;
            dic.TryAdd(instance.TypeName, instance);
        });


    }

    private ConcurrentDictionary<string, EthercatType> dic = new();
    public EthercatType this[string key] => dic[key];
    public IEnumerable<string> Keys => dic.Keys;
    public IEnumerable<EthercatType> Values => dic.Values;
    public int Count => dic.Count();
    public bool ContainsKey(string key) => dic.ContainsKey(key);
    public IEnumerator<KeyValuePair<string, EthercatType>> GetEnumerator() => dic.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => dic.GetEnumerator();
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out EthercatType value) =>
    dic.TryGetValue(key, out value);

    internal void Load(IEnumerable<XElement> dataTypes) {

        dataTypes.ToList().ForEach(cur => {
            string curTypeName = cur.XPathSelectElement("./Name")!.Value;
            if (ContainsKey(curTypeName)) return;
            int bitSize = int.Parse(cur.XPathSelectElement("./BitSize")!.Value);

            //Probe for array type.
            XElement? arrayInfo = cur.XPathSelectElement("./ArrayInfo");
            if (arrayInfo is not null) {
                EthercatType curArrayType = new EthercatArrayType(cur);
                dic.TryAdd(curArrayType.TypeName, curArrayType);
                return;
            }

            var subItems = cur.XPathSelectElements("./SubItem").ToList();
            if (subItems.Any()) {
                EthercatType curSubItemType = new EthercatSubIndexType(cur);
                dic.TryAdd(curSubItemType.TypeName, curSubItemType);
                return;
            }

            throw new Exception($@"Could not determine if {curTypeName} is Index, SubItem.
            If it's a native type, it is not currently delcared ");

        });

        //After we have loaded all the types to the dictionary, it's time to bind to their respective baseTypes.
        dic.Values.OfType<EthercatArrayType>().ToList().ForEach(x=> x.BindIndexType(dic));
        dic.Values.OfType<EthercatSubIndexType>().ToList().ForEach(x=> x.BindSubIndexTypes(dic));






    }
}