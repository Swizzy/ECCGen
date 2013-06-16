namespace ECCGen {
    using System;

    internal class NANDParser : Crypto {
        internal static string Source;

        public static ECCGenerator.SpareType GetSpareType(ref byte[] data) {
            throw new NotImplementedException();
        }

        public static ECCGenerator.SpareType GetSpareType(string src) {
            throw new NotImplementedException();
        }

        internal static byte[] GrabData(ref byte[] data, NANDParts part) {
            throw new NotImplementedException();
        }

        internal static byte[] GrabData(NANDParts part) {
            throw new NotImplementedException();
        }

        #region Nested type: NANDParts

        internal enum NANDParts {
            KV,
            SMC,
            CBA,
            CBB,
        }

        #endregion
    }
}