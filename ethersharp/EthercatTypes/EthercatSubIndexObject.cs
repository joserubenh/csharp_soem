using System.Reflection;
using System.Text;

namespace EthercatTypes;

public class EthercatSubIndexObject {
    public FieldInfo? ReflectionField {get;private set;}
   
    internal void AttachField(FieldInfo x, EthercatContext context, ushort slave, ushort address) {
        ReflectionField = x;

        this.GetType().GetFields().
        Where(x => x.GetCustomAttributes(typeof(SubIndexAttribute), false).Any()).
        ToList().ForEach(x => {

            SubIndexAttribute index = (x.GetCustomAttributes(typeof(SubIndexAttribute), false).First() as SubIndexAttribute)!;        
            switch (x.GetValue(this)!) {
                case EthercatTypes.EthercatFieldBase fieldBase:
                    fieldBase.AttachField(x, context, slave, address, index.SubIndexAddress, false);
                    break;

                case EthercatTypes.EthercatArrayBase arrayField:
                    arrayField.AttachArray(x, context, slave, address, index.SubIndexAddress);
                    break;
            }

        });
    }

    internal void WriteFullDebugInfo(System.IO.TextWriter st) {
        this.GetType().GetFields().
        Where(x => x.GetCustomAttributes(typeof(SubIndexAttribute), false).Any()).
        ToList().ForEach(x => {

            SubIndexAttribute index = (x.GetCustomAttributes(typeof(SubIndexAttribute), false).First() as SubIndexAttribute)!;        
            switch (x.GetValue(this)!) {
                case EthercatTypes.EthercatFieldBase fieldBase:
                    st.WriteLine(fieldBase.GetDebugString().Result);                    
                    break;
                case EthercatTypes.EthercatArrayBase arrayField:
                     arrayField.WriteFullDebugInfo(st);
                    break;
            }

        });
    }
}