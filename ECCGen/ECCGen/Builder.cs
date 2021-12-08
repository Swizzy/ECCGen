namespace ECCGen {
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal sealed class Builder : Crypto {
        private const uint CBOffset = 0x8000;
        private const uint KVOffset = 0x4000;
        private const uint Xellbase = 0xc0000;

        private readonly int[] _patchBuilds = new[] {
                                                    4577, 5772, 6572, 9188, 13121
                                                    };

        private readonly int[] _xorHackBuilds = new[] {
                                                      4575, 5773, 6753, 9230, 13180
                                                      };

        internal readonly List<byte> ECCData = new List<byte>();

        private static byte[] Correctendian(byte[] array) {
            if(BitConverter.IsLittleEndian)
                Array.Reverse(array);
            return array;
        }

        internal void AddHeader(uint baseSize, uint smcOffset, uint smcSize, string cbVersion, string hack) {
            var header = new byte[0x200];
            header[0x00] = 0xFF;
            header[0x01] = 0x4F;
            header[0x02] = 0x07;
            header[0x03] = 0x60;
            header[0x0A] = 0x80;
            header[0x62] = 0x40;
            header[0x6E] = 0x40;
            header[0x69] = 0x02;
            header[0x6A] = 0x07;
            header[0x6B] = 0x12;
            var tmp = BitConverter.GetBytes(baseSize);
            tmp = Correctendian(tmp);
            Array.Copy(tmp, 0, header, 0xC, 0x4);
            Array.Copy(tmp, 0, header, 0x64, 0x4);
            tmp = Encoding.ASCII.GetBytes(string.Format("ECCGen v{0} ECC Image, CB={1} {2}", ECCGenerator.Ver, cbVersion, hack));
            Buffer.BlockCopy(tmp, 0, header, 0x10, tmp.Length < 0x40 ? tmp.Length : 0x40);
            tmp = BitConverter.GetBytes(smcOffset);
            tmp = Correctendian(tmp);
            Array.Copy(tmp, 0, header, 0x7C, 0x4);
            tmp = BitConverter.GetBytes(smcSize);
            tmp = Correctendian(tmp);
            Array.Copy(tmp, 0, header, 0x78, 0x4);
            ECCData.AddRange(header);
        }

        internal void PadToFlash(uint pos) {
            if(ECCData.Count > pos)
                throw new ArgumentOutOfRangeException("pos");
            if(ECCData.Count == pos)
                return;
            ECCData.AddRange(new byte[pos - ECCData.Count]);
        }

        internal void AddSMC(byte[] data) {
            var offset = (uint) (0x4000 - data.Length);
            if(ECCData.Count < offset) {
                ECCGenerator.UpdateStatus(string.Format("Padding to SMC Offset: 0x{0:X} Size: 0x{1:X}", ECCData.Count, offset - ECCData.Count));
                PadToFlash(offset);
            }
            ECCGenerator.UpdateStatus(string.Format("Adding SMC Offset: 0x{0:X} Size: 0x{1:X}", ECCData.Count, data.Length));
            ECCData.AddRange(data);
        }

        internal void AddBootLoader(byte[] data, string version) {
            if(ECCData.Count < CBOffset) {
                ECCGenerator.UpdateStatus(string.Format("Padding to {2} Offset: 0x{0:X} Size: 0x{1:X}", ECCData.Count, CBOffset - ECCData.Count, version));
                PadToFlash(CBOffset);
            }
            ECCGenerator.UpdateStatus(string.Format("Adding {2} Offset: 0x{0:X} Size: 0x{1:X}", ECCData.Count, data.Length, version));
            ECCData.AddRange(data);
        }

        internal void AddKV(byte[] data) {
            if(ECCData.Count < KVOffset) {
                ECCGenerator.UpdateStatus(string.Format("Padding to Keyvault Offset: 0x{0:X} Size: 0x{1:X}", ECCData.Count, KVOffset - ECCData.Count));
                PadToFlash(KVOffset);
            }
            ECCGenerator.UpdateStatus(string.Format("Adding Keyvault Offset: 0x{0:X} Size: 0x{1:X}", ECCData.Count, data.Length));
            ECCData.AddRange(data);
        }

        internal void AddXeLL(byte[] data) {
            if(ECCData.Count < Xellbase) {
                ECCGenerator.UpdateStatus(string.Format("Padding to XeLL Offset: 0x{0:X} Size: 0x{1:X}", ECCData.Count, Xellbase - ECCData.Count));
                PadToFlash(Xellbase);
            }
            ECCGenerator.UpdateStatus(string.Format("Adding XeLL Offset: 0x{0:X} Size: 0x{1:X}", ECCData.Count, data.Length));
            ECCData.AddRange(data);
        }
    }
}
