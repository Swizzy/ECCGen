namespace ECCGen {
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;

    internal abstract class Crypto : Utils {
        private static void Rc4(ref byte[] bytes, IList<byte> key) {
            if(bytes == null)
                throw new ArgumentNullException("bytes");
            if(key == null)
                throw new ArgumentNullException("key");
            if(bytes.Length == 0 || key.Count == 0)
                throw new InvalidOperationException("You must supply data and key!");
            var s = new byte[256];
            var k = new byte[256];
            byte temp;
            int i;
            for(i = 0; i < 256; i++) {
                s[i] = (byte) i;
                k[i] = key[i % key.Count];
            }
            var j = 0;
            for(i = 0; i < 256; i++) {
                j = (j + s[i] + k[i]) % 256;
                temp = s[i];
                s[i] = s[j];
                s[j] = temp;
            }
            i = j = 0;
            for(var x = 0; x < bytes.Length; x++) {
                i = (i + 1) % 256;
                j = (j + s[i]) % 256;
                temp = s[i];
                s[i] = s[j];
                s[j] = temp;
                var t = (s[i] + s[j]) % 256;
                bytes[x] ^= s[t];
            }
        }

        internal static byte[] EncryptBootloader(ref byte[] data, byte[] key, bool useDefaultEncryption = true, byte[] secondaryKey = null) {
            if(data.Length <= 0)
                throw new InvalidOperationException("You must supply data to encrypt!");
            if(key == null || key.Length <= 0) {
                key = new byte[] {
                                 0xDD, 0x88, 0xAD, 0x0C, 0x9E, 0xD6, 0x69, 0xE7, 0xB5, 0x67, 0x94, 0xFB, 0x68, 0x56, 0x3E, 0xFA
                                 }; //1BL Key 
            }
            var header = new byte[0x10];
            Buffer.BlockCopy(header, 0, data, 0x10, 0x10);
            var ret = new byte[0];
            if(!useDefaultEncryption) {
                if(secondaryKey == null)
                    throw new ArgumentNullException("secondaryKey", "You must supply a secondary key when not using default encryption!");
                var type = Swap16(BitConverter.ToUInt16(data, 0x6));
                if(type == 0x801)
                    key = new byte[0x10];
                switch(type) {
                    case 0x801:
                    case 0x800:
                        Array.Resize(ref header, 0x20);
                        Array.Copy(key, 0x0, header, 0x10, key.Length);
                        ret = new HMACSHA1(secondaryKey).ComputeHash(header);
                        break;
                    case 0x1800:
                        throw new NotSupportedException("This type of encryption is currently not supported...");
                }
            }
            else
                ret = new HMACSHA1(key).ComputeHash(header);
            Array.Resize(ref ret, 0x10);
            var encrypted = new byte[data.Length - 0x20];
            Buffer.BlockCopy(data, 0x20, encrypted, 0x0, encrypted.Length);
            Rc4(ref encrypted, ret);
            Buffer.BlockCopy(encrypted, 0x0, data, 0x20, encrypted.Length);
            return ret;
        }

        internal static void EncryptSmc(ref byte[] data) {
            if(data.Length <= 0)
                throw new InvalidOperationException("You must supply data to encrypt!");
            var key = new byte[] {
                                 0x42, 0x75, 0x4E, 0x79
                                 };
            for(var i = 0; i < data.Length; i++) {
                var num = data[i] ^ (key[i & 3] & 0xff);
                var num2 = num * 0xfb;
                data[i] = Convert.ToByte(num);
                key[(i + 1) & 3] = (byte) (key[(i + 1) & 3] + ((byte) num2));
                key[(i + 2) & 3] = (byte) (key[(i + 2) & 3] + Convert.ToByte(num2 >> 8));
            }
            try {
                var tmp = new List<byte>(data).ToArray();
                DecryptSmc(ref tmp);
            }
            catch(Exception ex) {
                if(ex.Message == "Decryption Failed!")
                    throw new Exception("Encryption failed!");
                throw;
            }
        }

        internal static void DecryptSmc(ref byte[] data) {
            if(data.Length <= 0)
                throw new InvalidOperationException("You must supply data to encrypt!");
            var key = new byte[] {
                                 0x42, 0x75, 0x4E, 0x79
                                 };
            for(var i = 0; i < data.Length; i++) {
                var num = data[i];
                var num2 = num * 0xFB;
                data[i] = Convert.ToByte(num ^ (key[i & 3] & 0xFF));
                key[(i + 1) & 3] += (byte) num2;
                key[(i + 2) & 3] += Convert.ToByte(num2 >> 8);
            }
            if((data[data.Length - 4] != 0x00) && (data[data.Length - 3] != 0x00) && (data[data.Length - 2] != 0x00) && (data[data.Length - 1] != 0x00))
                throw new Exception("Decryption Failed!");
        }

        internal static bool CheckBootloaderDecrypted(ref byte[] data, int offset, int length)
        {
            if(data.Length < offset + length)
                throw data.Length < offset ? new ArgumentOutOfRangeException("offset") : new ArgumentOutOfRangeException("length");
            for (var i = offset; i < length + offset; i++)
                if (data[i] != 0x00)
                    return false;
            return true;
        }
    }
}