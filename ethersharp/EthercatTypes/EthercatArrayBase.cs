using System.Reflection;
using System.Text;

namespace EthercatTypes;

public class EthercatArrayBase {
    internal void AttachArray(FieldInfo x, EthercatContext context, ushort slave, ushort address, ushort subIndexAddress) {
        this.GetType().GetFields().
        Where(x =>
            x.GetCustomAttributes(typeof(ArrayOffsetAttribute), false).Any()
        ).
        ToList().ForEach(x => {
            var index = (x.GetCustomAttributes(typeof(ArrayOffsetAttribute), false).First() as ArrayOffsetAttribute)!;
            switch (x.GetValue(this)) {
                case EthercatTypes.EthercatFieldBase fieldBase:
                    fieldBase.AttachField(x, context, slave, address, Convert.ToUInt16(subIndexAddress + index.SubIndexOffset), false);
                    break;
            }
        });


    }

    internal void WriteFullDebugInfo(System.IO.TextWriter st) {

        this.GetType().GetFields().
               Where(x => x.GetCustomAttributes(typeof(ArrayOffsetAttribute), false).Any()).
               ToList().ForEach(x => {
                   var index = (x.GetCustomAttributes(typeof(ArrayOffsetAttribute), false).First() as ArrayOffsetAttribute)!;
                   switch (x.GetValue(this)) {
                       case EthercatTypes.EthercatFieldBase fieldBase:
                           st.WriteLine(fieldBase.GetDebugString().Result);
                           break;
                   }
               });


    }
}
