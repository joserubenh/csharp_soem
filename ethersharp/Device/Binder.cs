namespace Device;
public class EthercatSlave {
    public void BindToDevice(EthercatContext context, UInt16 slave) {


       var flds = this.GetType().GetFields().ToList();

       flds.Where(x => x.GetCustomAttributes(typeof(ObjectIndexAttribute), false).Any()).ToList().ForEach(x => {
            ObjectIndexAttribute index = (x.GetCustomAttributes(typeof(ObjectIndexAttribute), false).First() as ObjectIndexAttribute)!;

            switch (x.GetValue(this)!) {
                case EthercatTypes.EthercatFieldBase fieldBase:
                    fieldBase.AttachField(x, context, slave, index.Address, 0x0, true);
                    break;

                case EthercatTypes.EthercatSubIndexObject subIndexBase:
                    subIndexBase.AttachField(x, context, slave, index.Address);
                    break;
            }

        });
    }

    internal void WriteFullDebugInfo(System.IO.TextWriter st) {
        this.GetType().GetFields().
        Where(x => x.GetCustomAttributes(typeof(ObjectIndexAttribute), false).Any()).ToList().ForEach(x => {
            ObjectIndexAttribute index = (x.GetCustomAttributes(typeof(ObjectIndexAttribute), false).First() as ObjectIndexAttribute)!;
            switch (x.GetValue(this)!) {
                case EthercatTypes.EthercatFieldBase fieldBase:
                    st.WriteLine(fieldBase.GetDebugString().Result);
                    break;
                case EthercatTypes.EthercatSubIndexObject subIndexBase:
                    st.WriteLine();                    
                    st.WriteLine(subIndexBase.ReflectionField?.Name);
                    subIndexBase.WriteFullDebugInfo(st);
                    st.WriteLine();                    
                    break;
            }
        });






    }

}