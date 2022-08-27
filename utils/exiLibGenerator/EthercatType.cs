// See https://aka.ms/new-console-template for more information
internal abstract class EthercatType {
    public abstract string TypeName { get; }
    public abstract int BitSize { get; }
    internal abstract void WriteCSharpDefinition(StreamWriter ws) ;

    public virtual string GetCsharpClassName() {
        return  Tools.ToPascalCase(TypeName);
    }

}