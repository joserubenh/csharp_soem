internal class NativeTypes {
    public abstract class NativeTypeBase : EthercatType {
        internal abstract Type GetNativeType();
        internal override void WriteCSharpDefinition(StreamWriter ws) {
            ws.WriteLine($"public class {GetCsharpClassName()} {{");
            var returnType =  GetNativeType();
            ws.WriteLine($"    public {returnType.FullName}? Value {{get; set; }}");
            ws.WriteLine("}");
        }       

      public override string GetCsharpClassName(){
        return "_" + base.GetCsharpClassName().ToLower();
      }
        
    }

    public class dBoolean : NativeTypeBase {
        public override string TypeName => "BOOLEAN";
        public override int BitSize => 8;
        internal override Type GetNativeType() => typeof(Boolean);        
    }

    public class Dint : NativeTypeBase {
        public override string TypeName => "DINT";
        public override int BitSize => 32;
        internal override Type GetNativeType() => typeof(Int32);        
    }

    public class nint : NativeTypeBase {
        public override string TypeName => "INT";
        public override int BitSize => 16;
        internal override Type GetNativeType() => typeof(Int16);        
    }


    public class Sint : NativeTypeBase {
        public override string TypeName => "SINT";
        public override int BitSize => 16;
        internal override Type GetNativeType() => typeof(Int16);
    }

    public class USInt : NativeTypeBase {
        public override string TypeName => "USINT";
        public override int BitSize => 8;
        internal override Type GetNativeType() => typeof(byte);
    }


    public class UInt : NativeTypeBase {
        public override string TypeName => "UINT";
        public override int BitSize => 16;
        internal override Type GetNativeType() =>   typeof(UInt16);
    }

    public class Udint : NativeTypeBase {
        public override string TypeName => "UDINT";
        public override int BitSize => 32;
        internal override Type GetNativeType() =>  typeof(UInt32);
    }


    public class String0 : NativeTypeBase {
        public override string TypeName => "STRING(0)";
        public override int BitSize => 0;
        internal override Type GetNativeType() =>  typeof(string);
    }

    public class String4 : NativeTypeBase {
        public override string TypeName => "STRING(4)";
        public override int BitSize => 32;
        internal override Type GetNativeType() =>  typeof(string);
    }

    public class String11 : NativeTypeBase {
        public override string TypeName => "STRING(11)";
        public override int BitSize => 88;
        internal override Type GetNativeType() =>  typeof(string);
    }

        public class String16 : NativeTypeBase {
        public override string TypeName => "STRING(16)";
        public override int BitSize => 16*8;
        internal override Type GetNativeType() =>  typeof(string);
    }

    public class String30 : NativeTypeBase {
        public override string TypeName => "STRING(30)";
        public override int BitSize => 240;
        internal override Type GetNativeType() =>  typeof(string);
    }

    public class String32 : NativeTypeBase {
        public override string TypeName => "STRING(32)";
        public override int BitSize => 32*8;
        internal override Type GetNativeType() =>  typeof(string);
    }

}