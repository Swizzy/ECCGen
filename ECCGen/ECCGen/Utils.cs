namespace ECCGen {
    using System;
    using System.Reflection;

    public class Utils {
        internal static UInt32 Swap32(UInt32 input) {
            return (input & 0x000000FFU) << 24 | (input & 0x0000FF00U) << 8 | (input & 0x00FF0000U) >> 8 | (input & 0xFF000000U) >> 24;
        }

        internal static UInt16 Swap16(UInt16 input) {
            return (UInt16) ((0xFF00 & input) >> 8 | (0x00FF & input) << 8);
        }


        internal static byte[] GetBuiltInData(string fileName) {
            if(string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");
            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("{0}.{1}", typeof(ECCGenerator).Namespace, fileName))) {
                if(stream != null) {
                    var data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                    return data;
                }
                throw new Exception(string.Format("Can't find {0}!", fileName));
            }
        }
    }
}